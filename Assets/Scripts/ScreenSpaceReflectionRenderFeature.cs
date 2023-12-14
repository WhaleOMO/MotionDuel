using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceReflectionRenderFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        // Basic settings
        public RenderPassEvent injectionPoint;
        public Shader ssrShader;
        // SSR specific settings
        public int maxSteps;
        public float maxDistance;
        public float thickness;
        public float reflectionStride;
        public float reflectionJitter;
    }
    
    // Render Pass
    class ScreenSpaceReflectionRenderPass : ScriptableRenderPass
    {
        private Settings _settings;
        private Material _ssrMaterial;

        private RenderTextureDescriptor _descriptor;
        private RTHandle _ssrColor;     
        
        private ProfilingSampler _profilingSampler;
        private static readonly int SsrTextureShaderID = Shader.PropertyToID("_SSRTexture");

        private float _timer = 0;
        
        public ScreenSpaceReflectionRenderPass(Settings settings)
        {
            this._settings = settings;
            this.renderPassEvent = settings.injectionPoint;
            this._ssrMaterial = CoreUtils.CreateEngineMaterial(settings.ssrShader);
            _profilingSampler = new ProfilingSampler("SSR Render Pass");
            _timer = 0;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _descriptor = renderingData.cameraData.cameraTargetDescriptor;
            _descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _ssrColor, _descriptor, name: "_SSRTexture");
            //ConfigureTarget(_ssrColor, null);
            //ConfigureClear(ClearFlag.Color, Color.black);
        }

        public void Dispose()
        {
            _ssrColor?.Release();
            CoreUtils.Destroy(_ssrMaterial);
        }

        void UpdateSSRParams()
        {
            _ssrMaterial.SetFloat(Shader.PropertyToID("_MaxSteps"), _settings.maxSteps);
            _ssrMaterial.SetFloat(Shader.PropertyToID("_MaxDistance"), _settings.maxDistance);
            _ssrMaterial.SetFloat(Shader.PropertyToID("_Thickness"), _settings.thickness);
            _ssrMaterial.SetFloat(Shader.PropertyToID("_ReflectionStride"), _settings.reflectionStride);
            _ssrMaterial.SetFloat(Shader.PropertyToID("_ReflectionJitter"), _settings.reflectionJitter);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            _timer += Time.deltaTime;
            
            if (_timer < TimeSpan.FromMilliseconds(20).TotalSeconds)
            {
                return;
            }

            _timer = 0;
            CommandBuffer cmd = CommandBufferPool.Get("SSR_Pass");
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
                RenderingUtils.ReAllocateIfNeeded(ref _ssrColor, _descriptor, name: "_SSRTexture");
                UpdateSSRParams();
                // Blit camera texture with ssr material
                Blitter.BlitCameraTexture(cmd, cameraTargetHandle, _ssrColor, _ssrMaterial, 0);
            }

            // Execute command buffer
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            // Set global texture
            Shader.SetGlobalTexture(SsrTextureShaderID, _ssrColor);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // _ssrColor?.Release();
        }
    }

    public Settings settings;

    private ScreenSpaceReflectionRenderPass _ssrPass;
    
    public override void Create()
    {
        _ssrPass = new ScreenSpaceReflectionRenderPass(settings);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        _ssrPass.ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.ssrShader == null)
        {
            Debug.LogWarning("Missing SSR Shader. Will not execute.");
            return;
        }
        
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            _ssrPass.ConfigureInput(ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(_ssrPass);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        _ssrPass.Dispose();
    }
}
