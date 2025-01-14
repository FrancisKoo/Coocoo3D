﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Coocoo3D.Components;
using Coocoo3D.Present;
using Coocoo3DGraphics;
using System.ComponentModel;
using Coocoo3D.RenderPipeline;
using Coocoo3D.ResourceWarp;

namespace Coocoo3D.Present
{
    //public class MMD3DEntity : ISceneObject, INotifyPropertyChanged
    //{
    //    public Vector3 Position;
    //    public Quaternion Rotation = Quaternion.Identity;
    //    public Vector3 PositionNextFrame;
    //    public Quaternion RotationNextFrame = Quaternion.Identity;

    //    public string Name;
    //    public string Description;
    //    public string ModelPath;

    //    public MMDRendererComponent rendererComponent = new MMDRendererComponent();
    //    public MMDMotionComponent motionComponent = new MMDMotionComponent();
    //    public MMDMorphStateComponent morphStateComponent { get => rendererComponent.morphStateComponent; }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    public void PropChange(PropertyChangedEventArgs e)
    //    {
    //        PropertyChanged?.Invoke(this, e);
    //    }

    //    public override string ToString()
    //    {
    //        return Name;
    //    }
    //}
}
namespace Coocoo3D.FileFormat
{
    public static partial class PMXFormatExtension
    {
        //public static void Reload2(this MMD3DEntity entity, ProcessingList processingList, ModelPack modelPack, List<Texture2D> textures, string ModelPath)
        //{
        //    var modelResource = modelPack.pmx;
        //    LoadDesc(entity, modelResource, ModelPath);
        //    ReloadModel(entity, processingList, modelPack, textures);
        //}

        //public static void LoadDesc(this MMD3DEntity entity, PMXFormat pmx, string ModelPath)
        //{
        //    entity.Name = string.Format("{0} {1}", pmx.Name, pmx.NameEN);
        //    entity.Description = string.Format("{0}\n{1}", pmx.Description, pmx.DescriptionEN);
        //    entity.ModelPath = ModelPath;
        //}

        //public static void ReloadModel(this MMD3DEntity entity, ProcessingList processingList, ModelPack modelPack, List<Texture2D> textures)
        //{
        //    var modelResource = modelPack.pmx;
        //    entity.rendererComponent.ReloadModel(modelPack, textures);
        //    processingList.AddObject(new MeshAppendUploadPack(entity.rendererComponent.meshAppend, entity.rendererComponent.meshPosData));
        //}

        public static void Reload2(this GameObject gameObject, ProcessingList processingList, ModelPack modelPack, List<Texture2D> textures, string ModelPath)
        {
            var modelResource = modelPack.pmx;
            gameObject.Name = string.Format("{0} {1}", modelResource.Name, modelResource.NameEN);
            gameObject.Description = string.Format("{0}\n{1}", modelResource.Description, modelResource.DescriptionEN);
            //entity.ModelPath = ModelPath;

            ReloadModel(gameObject, processingList, modelPack, textures);
        }

        public static void ReloadModel(this GameObject gameObject, ProcessingList processingList, ModelPack modelPack, List<Texture2D> textures)
        {
            var modelResource = modelPack.pmx;
            var rendererComponent = new MMDRendererComponent();
            var morphStateComponent = rendererComponent.morphStateComponent;
            gameObject.AddComponent(rendererComponent);
            gameObject.AddComponent(new MMDMotionComponent());
            morphStateComponent.Reload(modelResource);

            rendererComponent.ReloadModel(modelPack, textures);
            processingList.AddObject(new MeshAppendUploadPack(rendererComponent.meshAppend, rendererComponent.meshPosData));

        }
    }
}
