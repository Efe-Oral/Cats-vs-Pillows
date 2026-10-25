// Full-screen ink outline: detects depth and normal discontinuities and darkens them.
// Driven by ToonOutlineFeature (URP renderer feature).
Shader "Hidden/CatsVsPillows/ToonOutline"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "ToonOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            half4 _OutlineColor;
            float _OutlineThickness;     // pixels at 1080p
            float _DepthThreshold;       // relative depth jump that counts as an edge
            float _NormalThreshold;      // 0..2, how different neighbour normals must be
            float _GrazingDepthBoost;    // raises the depth threshold on surfaces seen at a grazing angle
            float _MaxDistance;          // outlines fade out beyond this eye depth

            float SampleEyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float2 texel = _OutlineThickness * (_ScreenParams.y / 1080.0) / _ScreenParams.xy;
                float2 uvA = uv + float2(-texel.x, -texel.y);
                float2 uvB = uv + float2( texel.x,  texel.y);
                float2 uvC = uv + float2( texel.x, -texel.y);
                float2 uvD = uv + float2(-texel.x,  texel.y);

                float depthCenter = SampleEyeDepth(uv);
                float depthEdge = abs(SampleEyeDepth(uvA) - SampleEyeDepth(uvB)) + abs(SampleEyeDepth(uvC) - SampleEyeDepth(uvD));

                float3 normalCenter = SampleSceneNormals(uv);
                float3 nA = SampleSceneNormals(uvA);
                float3 nB = SampleSceneNormals(uvB);
                float3 nC = SampleSceneNormals(uvC);
                float3 nD = SampleSceneNormals(uvD);
                float normalEdge = length(nA - nB) + length(nC - nD);

                // Surfaces seen edge-on change depth quickly without being real edges; be stricter there.
                float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
                float facing = saturate(abs(dot(normalCenter, -cameraForward)));
                float depthThreshold = _DepthThreshold * depthCenter * (1.0 + _GrazingDepthBoost * (1.0 - facing));

                float edge = max(step(depthThreshold, depthEdge), step(_NormalThreshold, normalEdge));
                edge *= 1.0 - smoothstep(_MaxDistance * 0.8, _MaxDistance, depthCenter);

                return half4(lerp(color.rgb, _OutlineColor.rgb, edge * _OutlineColor.a), color.a);
            }
            ENDHLSL
        }
    }
}
