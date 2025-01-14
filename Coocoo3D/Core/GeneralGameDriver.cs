﻿using Coocoo3D.RenderPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coocoo3D.Core
{
    public class GeneralGameDriver : GameDriver
    {
        public override bool Next(RenderPipelineContext rpContext)
        {
            ref GameDriverContext context = ref rpContext.gameDriverContext;
            if (!(context.NeedRender>0 || context.Playing))
            {
                return false;
            }
            if (DateTime.Now - context.LatestRenderTime <TimeSpan.FromSeconds( context.FrameInterval))
            {
                context.NeedRender -=1;
                return false;
            }
            context.EnableDisplay = true;
            context.NeedRender -=1;
            context.RequireResize = context.RequireResizeOuter;
            context.RequireResizeOuter = false;

            DateTime now = DateTime.Now;
            context.DeltaTime = Math.Clamp((now - context.LatestRenderTime).TotalSeconds * context.PlaySpeed, -0.17f, 0.17f);
            context.LatestRenderTime = now;
            if (context.Playing)
                context.PlayTime += context.DeltaTime;
            return true;
        }
    }
}
