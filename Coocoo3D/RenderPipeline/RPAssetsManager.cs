﻿using Coocoo3D.Present;
using Coocoo3DGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using GSD = Coocoo3DGraphics.GraphicSignatureDesc;

namespace Coocoo3D.RenderPipeline
{
    public class RPAssetsManager
    {
        public GraphicsSignature rootSignature = new GraphicsSignature();
        public GraphicsSignature rootSignatureSkinning = new GraphicsSignature();
        public GraphicsSignature rootSignaturePostProcess = new GraphicsSignature();
        public GraphicsSignature rootSignatureCompute = new GraphicsSignature();
        public VertexShader VSMMDTransform = new VertexShader();
        public PixelShader PSMMD = new PixelShader();
        public PixelShader PSMMDTransparent = new PixelShader();
        public PixelShader PSMMD_DisneyBrdf = new PixelShader();
        public PixelShader PSMMD_Toon1 = new PixelShader();
        public PixelShader PSMMDAlphaClip = new PixelShader();
        public PixelShader PSMMDAlphaClip1 = new PixelShader();

        public Dictionary<string, VertexShader> VSAssets = new Dictionary<string, VertexShader>();
        public Dictionary<string, PixelShader> PSAssets = new Dictionary<string, PixelShader>();

        public PObject PObjectMMDSkinning = new PObject();
        public PObject PObjectMMD = new PObject();
        public PObject PObjectMMDTransparent = new PObject();
        public PObject PObjectMMD_DisneyBrdf = new PObject();
        public PObject PObjectMMD_Toon1 = new PObject();
        public PObject PObjectMMDShadowDepth = new PObject();
        public PObject PObjectMMDDepth = new PObject();
        public PObject PObjectMMDLoading = new PObject();
        public PObject PObjectMMDError = new PObject();
        public PObject PObjectDeferredRenderGBuffer = new PObject();
        public PObject PObjectDeferredRenderIBL = new PObject();
        public PObject PObjectDeferredRenderDirectLight = new PObject();
        public PObject PObjectDeferredRenderPointLight = new PObject();
        public PObject PObjectSkyBox = new PObject();
        public PObject PObjectPostProcess = new PObject();
        public PObject PObjectWidgetUI1 = new PObject();
        public PObject PObjectWidgetUI2 = new PObject();
        public PObject PObjectWidgetUILight = new PObject();
        public bool Ready;
        public void InitializeRootSignature(DeviceResources deviceResources)
        {
            rootSignature.ReloadMMD(deviceResources);
            rootSignatureSkinning.ReloadSkinning(deviceResources);
            rootSignaturePostProcess.Reload(deviceResources, new GraphicSignatureDesc[] { GSD.CBV, GSD.SRVTable, GSD.SRVTable, GSD.CBV });
            rootSignatureCompute.ReloadCompute(deviceResources, new GraphicSignatureDesc[] { GSD.CBV, GSD.CBV, GSD.CBV, GSD.SRV, GSD.UAV, GSD.UAV });
        }
        public async Task LoadAssets()
        {
            await ReloadVertexShader(VSMMDTransform, "ms-appx:///Coocoo3DGraphics/VSMMDTransform.cso");
            await ReloadPixelShader(PSMMD, "ms-appx:///Coocoo3DGraphics/PSMMD.cso");
            await ReloadPixelShader(PSMMDTransparent, "ms-appx:///Coocoo3DGraphics/PSMMDTransparent.cso");
            await ReloadPixelShader(PSMMD_DisneyBrdf, "ms-appx:///Coocoo3DGraphics/PSMMD_DisneyBRDF.cso");
            await ReloadPixelShader(PSMMD_Toon1, "ms-appx:///Coocoo3DGraphics/PSMMD_Toon1.cso");
            await ReloadPixelShader(PSMMDAlphaClip, "ms-appx:///Coocoo3DGraphics/PSMMDAlphaClip.cso");
            await ReloadPixelShader(PSMMDAlphaClip1, "ms-appx:///Coocoo3DGraphics/PSMMDAlphaClip1.cso");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DefaultResource));
            DefaultResource d = (DefaultResource)xmlSerializer.Deserialize(await OpenReadStream("ms-appx:///DefaultResources/DefaultResourceList.xml"));
            foreach(var vertexShader in d.vertexShaders)
            {
                RegVSAssets(vertexShader.Name, vertexShader.Path);
            }
            foreach(var pixelShader in d.pixelShaders)
            {
                RegPSAssets(pixelShader.Name, pixelShader.Path);
            }

            //await RegPSAssets("PSDeferredRenderGBuffer.cso");
            //await RegPSAssets("PSDeferredRenderIBL.cso");
            //await RegPSAssets("PSDeferredRenderDirectLight.cso");
            //await RegPSAssets("PSDeferredRenderPointLight.cso");

            //await RegPSAssets("PSSkyBox.cso");
            //await RegPSAssets("PSPostProcess.cso");
            //await RegPSAssets("PSWidgetUI1.cso");
            //await RegPSAssets("PSWidgetUI2.cso");
            //await RegPSAssets("PSWidgetUILight.cso");

            //await RegPSAssets("PSLoading.cso");
            //await RegPSAssets("PSError.cso");
        }
        public void InitializePipelineState()
        {
            Ready = false;

            PObjectMMDSkinning.Initialize(VSAssets["VSMMDSkinning.cso"], null, null);

            PObjectMMD.Initialize(VSMMDTransform, null, PSMMD);
            PObjectMMD_DisneyBrdf.Initialize(VSMMDTransform, null, PSMMD_DisneyBrdf);
            PObjectMMD_Toon1.Initialize(VSMMDTransform, null, PSMMD_Toon1);

            PObjectMMDTransparent.Initialize(VSMMDTransform, null, PSMMDTransparent);
            PObjectMMDLoading.Initialize(VSMMDTransform, null, PSAssets["PSLoading.cso"]);
            PObjectMMDError.Initialize(VSMMDTransform, null, PSAssets["PSError.cso"]);

            PObjectDeferredRenderGBuffer.Initialize(VSMMDTransform, null, PSAssets["PSDeferredRenderGBuffer.cso"]);
            PObjectDeferredRenderIBL.Initialize(VSAssets["VSSkyBox.cso"], null, PSAssets["PSDeferredRenderIBL.cso"]);
            PObjectDeferredRenderDirectLight.Initialize(VSAssets["VSSkyBox.cso"], null, PSAssets["PSDeferredRenderDirectLight.cso"]);
            PObjectDeferredRenderPointLight.Initialize(VSAssets["VSDeferredRenderPointLight.cso"], null, PSAssets["PSDeferredRenderPointLight.cso"]);

            PObjectMMDShadowDepth.Initialize(VSMMDTransform, null, null);
            PObjectMMDDepth.Initialize(VSMMDTransform, null, PSMMDAlphaClip1);

            PObjectSkyBox.Initialize(VSAssets["VSSkyBox.cso"], null, PSAssets["PSSkyBox.cso"]);
            PObjectPostProcess.Initialize(VSAssets["VSPostProcess.cso"], null, PSAssets["PSPostProcess.cso"]);
            PObjectWidgetUI1.Initialize(VSAssets["VSWidgetUI1.cso"], null, PSAssets["PSWidgetUI1.cso"]);
            PObjectWidgetUI2.Initialize(VSAssets["VSWidgetUI2.cso"], null, PSAssets["PSWidgetUI2.cso"]);
            PObjectWidgetUILight.Initialize(VSAssets["VSWidgetUILight.cso"], null, PSAssets["PSWidgetUILight.cso"]);
            Ready = true;
        }
        protected async Task ReloadVertexShader(VertexShader vertexShader, string uri)
        {
            vertexShader.Initialize(await ReadFile(uri));
        }
        protected async Task ReloadPixelShader(PixelShader pixelShader, string uri)
        {
            pixelShader.Initialize(await ReadFile(uri));
        }
        protected async Task ReloadComputeShader(ComputePO computeShader, string uri)
        {
            computeShader.Initialize(await ReadFile(uri));
        }
        static string assetsUri = "ms-appx:///Coocoo3DGraphics/";
        protected async Task RegVSAssets(string name, string path)
        {
            VertexShader vertexShader = new VertexShader();
            vertexShader.Initialize(await ReadFile(path));
            VSAssets.Add(name, vertexShader);
        }
        protected async Task RegPSAssets(string name,string path)
        {
            PixelShader pixelShader = new PixelShader();
            pixelShader.Initialize(await ReadFile(path));
            PSAssets.Add(name, pixelShader);
        }
        protected async Task<IBuffer> ReadFile(string uri)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            return await FileIO.ReadBufferAsync(file);
        }

        protected async Task<Stream> OpenReadStream(string uri)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            return (await file.OpenAsync(FileAccessMode.Read)).AsStreamForRead();
        }

    }
    [Serializable]
    public class DefaultResource
    {
        [XmlElement(ElementName = "VertexShader")]
        public List<_ResourceStr2> vertexShaders;
        [XmlElement(ElementName = "PixelShader")]
        public List<_ResourceStr2> pixelShaders;
    }
    public struct _ResourceStr2
    {
        public string Name;
        public string Path;
    }
}
