﻿using Coocoo3D.Components;
using Coocoo3D.Core;
using Coocoo3D.Present;
using Coocoo3DGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Coocoo3D.RenderPipeline
{
    public class ForwardRenderPipeline2 : RenderPipeline
    {
        public const int c_materialDataSize = 256;
        public const int c_offsetMaterialData = 0;
        public const int c_lightingDataSize = 512;
        public const int c_offsetLightingData = c_offsetMaterialData + c_materialDataSize;

        public const int c_presentDataSize = 512;
        public const int c_offsetPresentData = c_offsetLightingData + c_lightingDataSize;
        public void Reload(DeviceResources deviceResources)
        {
            Ready = true;
        }

        Random randomGenerator = new Random();

        struct _Counters
        {
            public int material;
            public int vertex;
        }

        bool HasMainLight;
        public override void PrepareRenderData(RenderPipelineContext context, GraphicsContext graphicsContext)
        {
            var deviceResources = context.deviceResources;
            var cameras = context.dynamicContextRead.cameras;
            ref var settings = ref context.dynamicContextRead.settings;
            ref var inShaderSettings = ref context.dynamicContextRead.inShaderSettings;
            var rendererComponents = context.dynamicContextRead.rendererComponents;
            var lightings = context.dynamicContextRead.lightings;

            #region passSettings
            var passSetting = context.RPAssetsManager.defaultPassSetting;
            foreach (var pass in passSetting.CombinedPasses)
            {
                if (pass.PassMatch1.Name == "Pass1")
                {
                    pass.PSODefault = context.RPAssetsManager.POPass1;
                }
                else if (pass.PassMatch1.Name == "ShadowMapPass")
                {
                    pass.PSODefault = context.RPAssetsManager.PObjectMMDShadowDepth;
                }
                pass.DrawObjects = true;
                pass.depthSencil = _GetRenderTargetByName(pass.PassMatch1.DepthStencil);
                pass.renderTarget = _GetRenderTargetByName(pass.PassMatch1.RenderTarget);
            }
            RenderTexture2D _GetRenderTargetByName(string name)
            {
                if (name == "_ShadowMap0")
                    return context.ShadowMap;
                else if (name == "_Output0")
                    return context.outputRTV;
                else if (name == "_ScreenDepth0")
                    return context.ScreenSizeDSVs[0];
                return null;
            }
            #endregion

            int numMaterials = 0;
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                numMaterials += rendererComponents[i].Materials.Count;
            }
            context.DesireMaterialBuffers(numMaterials);
            ref var bigBuffer = ref context.bigBuffer;
            #region Lighting
            int lightCount = 0;
            var camera = context.dynamicContextRead.cameras[0];
            Matrix4x4 lightCameraMatrix0 = Matrix4x4.Identity;
            //Matrix4x4 lightCameraMatrix1 = Matrix4x4.Identity;
            IntPtr pBufferData = Marshal.UnsafeAddrOfPinnedArrayElement(bigBuffer, c_offsetPresentData);
            HasMainLight = false;
            var LightCameraDataBuffers = context.LightCameraDataBuffer;
            if (lightings.Count > 0 && lightings[0].LightingType == LightingType.Directional)
            {
                //lightCameraMatrix0 = Matrix4x4.Transpose(lightings[0].GetLightingMatrix(2, camera.LookAtPoint, camera.Distance));
                lightCameraMatrix0 = Matrix4x4.Transpose(lightings[0].GetLightingMatrix(settings.ExtendShadowMapRange, camera.LookAtPoint, camera.Angle, camera.Distance)); ;
                Marshal.StructureToPtr(lightCameraMatrix0, pBufferData, true);

                //lightCameraMatrix1 = Matrix4x4.Transpose(lightings[0].GetLightingMatrix(settings.ExtendShadowMapRange, camera.LookAtPoint, camera.Angle, camera.Distance));
                //Marshal.StructureToPtr(lightCameraMatrix1, pBufferData + 256, true);
                graphicsContext.UpdateResource(LightCameraDataBuffers, bigBuffer, c_presentDataSize, c_offsetPresentData);
                HasMainLight = true;
            }

            IntPtr p0 = Marshal.UnsafeAddrOfPinnedArrayElement(bigBuffer, c_offsetLightingData);
            Array.Clear(bigBuffer, c_offsetLightingData, c_lightingDataSize);
            pBufferData = p0 + 256;
            Marshal.StructureToPtr(lightCameraMatrix0, p0, true);
            //Marshal.StructureToPtr(lightCameraMatrix1, p0 + 64, true);
            for (int i = 0; i < lightings.Count; i++)
            {
                LightingData data1 = lightings[i];
                Marshal.StructureToPtr(data1.GetPositionOrDirection(), pBufferData, true);
                Marshal.StructureToPtr((uint)data1.LightingType, pBufferData + 12, true);
                Marshal.StructureToPtr(data1.Color, pBufferData + 16, true);

                lightCount++;
                pBufferData += 32;
                if (lightCount >= 8)
                    break;
            }
            #endregion

            #region Update material data
            int matIndex = 0;
            pBufferData = Marshal.UnsafeAddrOfPinnedArrayElement(bigBuffer, c_offsetMaterialData);
            for (int i = 0; i < rendererComponents.Count; i++)
            {
                var Materials = rendererComponents[i].Materials;
                for (int j = 0; j < Materials.Count; j++)
                {
                    Marshal.StructureToPtr(Materials[j].innerStruct, pBufferData, true);
                    context.MaterialBufferGroup.UpdateSlience(graphicsContext, bigBuffer, c_offsetMaterialData, c_materialDataSize + c_lightingDataSize, matIndex);
                    matIndex++;
                }
            }
            if (matIndex > 0)
                context.MaterialBufferGroup.UpdateSlienceComplete(graphicsContext);
            #endregion

            pBufferData = Marshal.UnsafeAddrOfPinnedArrayElement(bigBuffer, c_offsetPresentData);

            PresentData cameraPresentData = new PresentData();
            cameraPresentData.PlayTime = (float)context.dynamicContextRead.Time;
            cameraPresentData.DeltaTime = (float)context.dynamicContextRead.DeltaTime;

            cameraPresentData.UpdateCameraData(cameras[0]);
            cameraPresentData.RandomValue1 = randomGenerator.Next(int.MinValue, int.MaxValue);
            cameraPresentData.RandomValue2 = randomGenerator.Next(int.MinValue, int.MaxValue);
            cameraPresentData.inShaderSettings = inShaderSettings;
            Marshal.StructureToPtr(cameraPresentData, pBufferData, true);
            graphicsContext.UpdateResource(context.CameraDataBuffers, bigBuffer, c_presentDataSize, c_offsetPresentData);

        }
        //you can fold local function in your editor
        public override void RenderCamera(RenderPipelineContext context, GraphicsContext graphicsContext)
        {
            var rendererComponents = context.dynamicContextRead.rendererComponents;
            ref var settings = ref context.dynamicContextRead.settings;
            ref var inShaderSettings = ref context.dynamicContextRead.inShaderSettings; ;
            Texture2D texLoading = context.TextureLoading;
            Texture2D texError = context.TextureError;
            Texture2D _Tex(Texture2D _tex) => TextureStatusSelect(_tex, texLoading, texError, texError);
            var rpAssets = context.RPAssetsManager;
            var RSBase = rpAssets.rootSignature;
            var deviceResources = context.deviceResources;

            PObject skinningPO;
            skinningPO = rpAssets.PObjectMMDSkinning;

            graphicsContext.SetRootSignature(rpAssets.rootSignatureSkinning);
            graphicsContext.SetSOMesh(context.SkinningMeshBuffer);
            void EntitySkinning(MMDRendererComponent rendererComponent, CBuffer cameraPresentData, CBuffer entityBoneDataBuffer)
            {
                var Materials = rendererComponent.Materials;
                graphicsContext.SetCBVR(entityBoneDataBuffer, 0);
                graphicsContext.SetCBVR(cameraPresentData, 2);
                var POSkinning = PObjectStatusSelect(deviceResources, rpAssets.rootSignatureSkinning, ref context.SkinningDesc, rendererComponent.POSkinning, skinningPO, skinningPO, skinningPO);
                SetPipelineStateVariant(deviceResources, graphicsContext, rpAssets.rootSignatureSkinning, ref context.SkinningDesc, POSkinning);
                graphicsContext.SetMeshVertex1(rendererComponent.mesh);
                graphicsContext.SetMeshVertex(rendererComponent.meshAppend);
                int indexCountAll = rendererComponent.meshVertexCount;
                graphicsContext.Draw(indexCountAll, 0);
            }
            for (int i = 0; i < rendererComponents.Count; i++)
                EntitySkinning(rendererComponents[i], context.CameraDataBuffers, context.CBs_Bone[i]);
            graphicsContext.SetSOMeshNone();

            //if (HasMainLight && inShaderSettings.EnableShadow)
            //{
            //    void _RenderEntityShadow(MMDRendererComponent rendererComponent, CBuffer cameraPresentData, int bufferOffset, CBuffer entityBoneDataBuffer, ref _Counters counter)
            //    {
            //        var Materials = rendererComponent.Materials;
            //        graphicsContext.SetCBVR(entityBoneDataBuffer, 0);
            //        graphicsContext.SetCBVR(cameraPresentData, bufferOffset, 1, 2);
            //        graphicsContext.SetMeshIndex(rendererComponent.mesh);

            //        graphicsContext.DrawIndexed(rendererComponent.meshIndexCount, 0, counter.vertex);
            //        counter.vertex += rendererComponent.meshVertexCount;
            //    }

            //    graphicsContext.SetMesh(context.SkinningMeshBuffer);
            //    graphicsContext.SetRootSignature(RSBase);
            //    SetPipelineStateVariant(deviceResources, graphicsContext, RSBase, ref context.shadowDesc, rpAssets.PObjectMMDShadowDepth);
            //    graphicsContext.SetDSV(context.ShadowMapCube, 0, true);
            //    _Counters counterShadow0 = new _Counters();
            //    var LightCameraDataBuffers = context.LightCameraDataBuffer;
            //    for (int i = 0; i < rendererComponents.Count; i++)
            //        _RenderEntityShadow(rendererComponents[i], LightCameraDataBuffers, 0, context.CBs_Bone[i], ref counterShadow0);
            //    graphicsContext.SetDSV(context.ShadowMapCube, 1, true);
            //    _Counters counterShadow1 = new _Counters();
            //    for (int i = 0; i < rendererComponents.Count; i++)
            //        _RenderEntityShadow(rendererComponents[i], LightCameraDataBuffers, 1, context.CBs_Bone[i], ref counterShadow1);
            //}

            graphicsContext.SetRootSignature(RSBase);
            graphicsContext.SetRTVDSV(context.outputRTV, context.ScreenSizeDSVs[0], Vector4.Zero, false, true);
            graphicsContext.SetCBVR(context.CameraDataBuffers, 2);
            graphicsContext.SetSRVT(context.ShadowMap, 5);
            graphicsContext.SetSRVT(context.SkyBox, 6);
            graphicsContext.SetSRVT(context.IrradianceMap, 7);
            graphicsContext.SetSRVT(context.BRDFLut, 8);
            #region Render Sky box

            PSODesc descSkyBox = new PSODesc
            {
                blendState = EBlendState.none,
                cullMode = ECullMode.back,
                depthBias = 0,
                slopeScaledDepthBias = 0,
                dsvFormat = context.depthFormat,
                inputLayout = EInputLayout.postProcess,
                ptt = ED3D12PrimitiveTopologyType.TRIANGLE,
                rtvFormat = context.outputFormat,
                renderTargetCount = 1,
                streamOutput = false,
                wireFrame = false,
            };
            SetPipelineStateVariant(deviceResources, graphicsContext, RSBase, ref descSkyBox, rpAssets.PObjectSkyBox);
            graphicsContext.SetMesh(context.ndcQuadMesh);
            graphicsContext.DrawIndexed(context.ndcQuadMeshIndexCount, 0, 0);
            #endregion

            graphicsContext.SetSRVT(context.EnvironmentMap, 6);
            graphicsContext.SetMesh(context.SkinningMeshBuffer);

            foreach (var combinedPass in context.RPAssetsManager.defaultPassSetting.CombinedPasses)
            {
                graphicsContext.SetMesh(context.SkinningMeshBuffer);
                graphicsContext.SetRootSignature(RSBase);
                CBuffer cBuffer = null;
                if (combinedPass.PassMatch1.Name == "ShadowMapPass")
                {
                    graphicsContext.SetDSV(combinedPass.depthSencil, true);
                }
                else if (combinedPass.PassMatch1.Name == "Pass1")
                {
                    graphicsContext.SetRTVDSV(combinedPass.renderTarget, combinedPass.depthSencil, Vector4.Zero, false, true);
                }
                if (combinedPass.Pass.Camera == "Main")
                {
                    cBuffer = context.CameraDataBuffers;
                }
                else if (combinedPass.Pass.Camera == "ShadowMap")
                {
                    if (!(HasMainLight && inShaderSettings.EnableShadow)) continue;
                    cBuffer = context.LightCameraDataBuffer;
                }
                PSODesc passPsoDesc;
                passPsoDesc.blendState = EBlendState.alpha;
                passPsoDesc.cullMode = ECullMode.back;
                passPsoDesc.depthBias = combinedPass.PassMatch1.DepthBias;
                passPsoDesc.slopeScaledDepthBias = combinedPass.PassMatch1.SlopeScaledDepthBias;
                passPsoDesc.dsvFormat = context.depthFormat;
                passPsoDesc.inputLayout = EInputLayout.skinned;
                passPsoDesc.ptt = ED3D12PrimitiveTopologyType.TRIANGLE;
                passPsoDesc.rtvFormat = context.outputFormat;
                passPsoDesc.renderTargetCount = 1;
                passPsoDesc.streamOutput = false;
                passPsoDesc.wireFrame = context.dynamicContextRead.settings.Wireframe;

                _Counters counterX = new _Counters();
                for (int i = 0; i < rendererComponents.Count; i++)
                {
                    _PassRender(rendererComponents[i], cBuffer, context.CBs_Bone[i], ref counterX);
                }
                void _PassRender(MMDRendererComponent rendererComponent, CBuffer cameraPresentData, CBuffer entityBoneDataBuffer, ref _Counters counter)
                {
                    if (combinedPass.DrawObjects)
                    {
                        graphicsContext.SetCBVR(entityBoneDataBuffer, 0);
                        graphicsContext.SetCBVR(cameraPresentData, 2);
                        graphicsContext.SetMeshIndex(rendererComponent.mesh);
                    }
                    if (combinedPass.PassMatch1.Name == "ShadowMapPass")
                    {
                        SetPipelineStateVariant(deviceResources, graphicsContext, RSBase, ref context.shadowDesc, combinedPass.PSODefault);
                        var Materials = rendererComponent.Materials;

                        graphicsContext.DrawIndexed(rendererComponent.meshIndexCount, 0, counter.vertex);
                    }
                    else if (combinedPass.PassMatch1.Name == "Pass1")
                    {
                        var PSODraw = PObjectStatusSelect(deviceResources, RSBase, ref passPsoDesc, rendererComponent.PODraw, rpAssets.PObjectMMDLoading, combinedPass.PSODefault, rpAssets.PObjectMMDError);
                        var Materials = rendererComponent.Materials;
                        List<Texture2D> texs = rendererComponent.textures;
                        int indexOffset = 0;
                        for (int i = 0; i < Materials.Count; i++)
                        {
                            if (Materials[i].innerStruct.DiffuseColor.W > 0)
                            {
                                Texture2D tex1 = null;
                                if (Materials[i].texIndex != -1 && Materials[i].texIndex < Materials.Count)
                                    tex1 = texs[Materials[i].texIndex];
                                context.MaterialBufferGroup.SetCBVR(graphicsContext, counter.material, 1);

                                graphicsContext.SetSRVT(_Tex(tex1), 3);

                                passPsoDesc.cullMode = Materials[i].DrawFlags.HasFlag(DrawFlag.DrawDoubleFace) ? ECullMode.none : ECullMode.back;
                                SetPipelineStateVariant(deviceResources, graphicsContext, RSBase, ref passPsoDesc, PSODraw);

                                graphicsContext.DrawIndexed(Materials[i].indexCount, indexOffset, counter.vertex);
                            }
                            counter.material++;
                            indexOffset += Materials[i].indexCount;
                        }
                    }
                    if (combinedPass.DrawObjects)
                    {
                        counter.vertex += rendererComponent.meshVertexCount;
                    }
                }
            }
        }
    }
}