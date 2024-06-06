using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using IntParameter = UnityEngine.Rendering.PostProcessing.IntParameter;


[Serializable]
[PostProcess(typeof(PostProcessPixelize), PostProcessEvent.BeforeTransparent, "Sora/Pixelized")]
public sealed class PixelizePostProcessing : PostProcessEffectSettings
{
    public IntParameter _PixelSample = new IntParameter { value = 1 };
}

public sealed class PostProcessPixelize : PostProcessEffectRenderer<PixelizePostProcessing>
{
    private RenderTargetIdentifier colorBuffer, pixelBuffer;
    
    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Hidden/Pixelize"));

        sheet.properties.SetFloat("_PixelSample", settings._PixelSample);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        
    }


}
