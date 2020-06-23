﻿using Coocoo3D.FileFormat;
using Coocoo3DGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coocoo3D.Core
{
    public class MainCaches
    {
        public Dictionary<string, Texture2D> textureCaches = new Dictionary<string, Texture2D>();
        public Dictionary<string, VertexShader> vsCaches = new Dictionary<string, VertexShader>();
        public Dictionary<string, GeometryShader> gsCaches = new Dictionary<string, GeometryShader>();
        public Dictionary<string, PixelShader> psCaches = new Dictionary<string, PixelShader>();
        public Dictionary<string, PixelShader> csCaches = new Dictionary<string, PixelShader>();
        public Dictionary<string, PObject> pObjectCaches = new Dictionary<string, PObject>();
        public Dictionary<string, PObject> pObjectShadowCaches = new Dictionary<string, PObject>();

        public Dictionary<string, PMXFormat> pmxCaches = new Dictionary<string, PMXFormat>();
    }
}