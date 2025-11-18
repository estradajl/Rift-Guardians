using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ClippingVolumeFeature : ScriptableRendererFeature
{
    class StencilPass : ScriptableRenderPass
    {
        private FilteringSettings filterSettings;
        private ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");
        private string profilerTag;

        public StencilPass(string profilerTag, LayerMask layerMask)
        {
            this.profilerTag = profilerTag;
            filterSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSettings);
        }
    }

    public LayerMask clippingLayer = 0;
    StencilPass stencilPass;

    public override void Create()
    {
        stencilPass = new StencilPass("Clipping Volume Mask", clippingLayer);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(stencilPass);
    }
}
