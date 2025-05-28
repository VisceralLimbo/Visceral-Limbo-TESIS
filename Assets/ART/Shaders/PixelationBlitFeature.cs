using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationBlitFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial;
        RenderTargetIdentifier source;
        RenderTargetHandle tempTexture;

        public CustomRenderPass(Material mat)
        {
            blitMaterial = mat;
            tempTexture.Init("_TempPixelTex");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("PixelationEffect");

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tempTexture.id, descriptor);

            Blit(cmd, source, tempTexture.Identifier(), blitMaterial);
            Blit(cmd, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    public Material pixelationMaterial;
    CustomRenderPass pass;

    public override void Create()
    {
        pass = new CustomRenderPass(pixelationMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pixelationMaterial != null && renderingData.cameraData.camera.name == "RootCamera")
        {
            renderer.EnqueuePass(pass);
        }
    }
}

