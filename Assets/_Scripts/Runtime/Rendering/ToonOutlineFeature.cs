using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CatsVsPillows.Rendering
{
    /// <summary>
    /// URP renderer feature that draws cartoon ink outlines as a full-screen pass, using the camera depth and
    /// normals textures. Works on every mesh (including the skinned cat) without per-object setup.
    /// </summary>
    public sealed class ToonOutlineFeature : ScriptableRendererFeature
    {
        [Serializable]
        public sealed class Settings
        {
            [Tooltip("Assign Hidden/CatsVsPillows/ToonOutline so the shader is included in builds.")]
            public Shader shader;
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Color color = new Color(0.24f, 0.13f, 0.12f, 1f);
            [Tooltip("Line thickness in pixels at 1080p; scales with resolution.")]
            [Range(0.5f, 4f)] public float thickness = 1.4f;
            [Tooltip("Relative depth jump (fraction of distance) that counts as an edge.")]
            [Range(0.001f, 0.5f)] public float depthThreshold = 0.06f;
            [Range(0.05f, 2f)] public float normalThreshold = 0.6f;
            [Range(0f, 20f)] public float grazingDepthBoost = 6f;
            [Min(1f)] public float maxDistance = 60f;
        }

        [SerializeField] private Settings settings = new Settings();

        private Material _material;
        private OutlinePass _pass;

        public override void Create()
        {
            if (settings.shader == null)
                settings.shader = Shader.Find("Hidden/CatsVsPillows/ToonOutline");

            if (settings.shader != null)
                _material = CoreUtils.CreateEngineMaterial(settings.shader);

            _pass = new OutlinePass { renderPassEvent = settings.renderPassEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            CameraType cameraType = renderingData.cameraData.cameraType;
            if (_material == null || cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
                return;

            _pass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(_pass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (_material != null)
                _pass.Setup(renderer.cameraColorTargetHandle, _material, settings);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
            CoreUtils.Destroy(_material);
        }

        private sealed class OutlinePass : ScriptableRenderPass
        {
            private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
            private static readonly int ThicknessId = Shader.PropertyToID("_OutlineThickness");
            private static readonly int DepthThresholdId = Shader.PropertyToID("_DepthThreshold");
            private static readonly int NormalThresholdId = Shader.PropertyToID("_NormalThreshold");
            private static readonly int GrazingBoostId = Shader.PropertyToID("_GrazingDepthBoost");
            private static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");

            private readonly ProfilingSampler _profilingSampler = new ProfilingSampler("Toon Outline");
            private RTHandle _source;
            private RTHandle _temp;
            private Material _material;

            public void Setup(RTHandle source, Material material, Settings settings)
            {
                _source = source;
                _material = material;
                material.SetColor(ColorId, settings.color);
                material.SetFloat(ThicknessId, settings.thickness);
                material.SetFloat(DepthThresholdId, settings.depthThreshold);
                material.SetFloat(NormalThresholdId, settings.normalThreshold);
                material.SetFloat(GrazingBoostId, settings.grazingDepthBoost);
                material.SetFloat(MaxDistanceId, settings.maxDistance);
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                RenderingUtils.ReAllocateIfNeeded(ref _temp, descriptor, name: "_ToonOutlineTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, _profilingSampler))
                {
                    Blitter.BlitCameraTexture(cmd, _source, _temp, _material, 0);
                    Blitter.BlitCameraTexture(cmd, _temp, _source);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Dispose() => _temp?.Release();
        }
    }
}
