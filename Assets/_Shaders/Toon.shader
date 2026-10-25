// Stylized cel-shaded lit shader for URP (Forward).
// Light is quantized into a lit and a tinted shadow band, plus a rim highlight and an optional toon specular.
// Includes ShadowCaster, DepthOnly and DepthNormals passes so it casts shadows and feeds the outline feature.
Shader "CatsVsPillows/Toon"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Range(0, 2)) = 1
        _Brightness ("Brightness", Range(0, 2)) = 1
        _HueShift ("Hue Shift", Range(-0.5, 0.5)) = 0

        [Header(World Space UV)]
        [Toggle] _UseWorldUV ("Use World Space UV (tiles with object size)", Float) = 0
        _WorldUVScale ("World UV Scale", Float) = 0.25

        [Header(Cel Shading)]
        _ShadowColor ("Shadow Tint", Color) = (0.62, 0.52, 0.72, 1)
        _RampThreshold ("Light/Shadow Threshold", Range(0, 1)) = 0.5
        _RampSmoothness ("Band Softness", Range(0.001, 0.5)) = 0.04
        _ReceivedShadowStrength ("Received Shadow Strength", Range(0, 1)) = 0.85
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 0.5

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (1, 0.95, 0.85, 1)
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.35
        _RimThreshold ("Rim Width", Range(0, 1)) = 0.72

        [Header(Specular)]
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularStrength ("Specular Strength", Range(0, 1)) = 0
        _SpecularSize ("Specular Size", Range(0.8, 1)) = 0.96
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Saturation;
            half _Brightness;
            half _HueShift;
            half _UseWorldUV;
            float _WorldUVScale;
            half4 _ShadowColor;
            half _RampThreshold;
            half _RampSmoothness;
            half _ReceivedShadowStrength;
            half _AmbientStrength;
            half4 _RimColor;
            half _RimStrength;
            half _RimThreshold;
            half4 _SpecularColor;
            half _SpecularStrength;
            half _SpecularSize;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            // Picks the world plane facing the surface, so tiling textures keep their scale on stretched blocks.
            float2 WorldUV(float3 positionWS, float3 normalWS)
            {
                float3 n = abs(normalWS);
                float2 uv = n.y > n.x && n.y > n.z ? positionWS.xz : (n.x > n.z ? positionWS.zy : positionWS.xy);
                return uv * _WorldUVScale * _BaseMap_ST.xy + _BaseMap_ST.zw;
            }

            half3 AdjustSaturation(half3 color, half saturation)
            {
                half luminance = dot(color, half3(0.299, 0.587, 0.114));
                return lerp(luminance.xxx, color, saturation);
            }

            half CelBand(half value)
            {
                return smoothstep(_RampThreshold - _RampSmoothness, _RampThreshold + _RampSmoothness, value);
            }

            half3 ShadeLight(Light light, half3 albedo, float3 normalWS, float3 viewDirWS, half shadowSoftness)
            {
                half halfLambert = dot(normalWS, light.direction) * 0.5h + 0.5h;
                half shadow = lerp(1.0h, light.shadowAttenuation, _ReceivedShadowStrength);
                half lit = CelBand(halfLambert) * smoothstep(0.5h - shadowSoftness, 0.5h + shadowSoftness, shadow) * light.distanceAttenuation;

                half3 diffuse = albedo * lerp(_ShadowColor.rgb, 1.0h, lit);

                float3 halfDir = SafeNormalize(light.direction + viewDirWS);
                half spec = smoothstep(_SpecularSize - 0.005h, _SpecularSize + 0.005h, saturate(dot(normalWS, halfDir)));
                half3 specular = _SpecularColor.rgb * spec * _SpecularStrength * lit;

                return (diffuse + specular) * light.color;
            }

            // Lamps and other point lights: a stylized two-step pool of warm light (bright core + soft outer ring)
            // that only ever adds light, instead of a long physically based falloff.
            half3 ShadeAdditionalLight(Light light, half3 albedo, float3 normalWS)
            {
                half halfLambert = dot(normalWS, light.direction) * 0.5h + 0.5h;
                half attenuation = light.distanceAttenuation;
                half pool = smoothstep(0.02h, 0.05h, attenuation) * 0.45h + smoothstep(0.18h, 0.24h, attenuation) * 0.55h;
                return albedo * light.color * pool * lerp(0.35h, 1.0h, CelBand(halfLambert));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float2 uv = _UseWorldUV > 0.5h ? WorldUV(input.positionWS, normalWS) : input.uv;

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseColor.rgb;
                if (_HueShift != 0.0h)
                {
                    half3 hsv = RgbToHsv(albedo);
                    hsv.x = frac(hsv.x + _HueShift);
                    albedo = HsvToRgb(hsv);
                }
                albedo = AdjustSaturation(albedo, _Saturation) * _Brightness;

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 color = ShadeLight(mainLight, albedo, normalWS, viewDirWS, 0.1h);

            #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightCount; ++i)
                {
                    Light light = GetAdditionalLight(i, input.positionWS, half4(1, 1, 1, 1));
                    color += ShadeAdditionalLight(light, albedo, normalWS);
                }
            #endif

                color += albedo * SampleSH(normalWS) * _AmbientStrength;

                // Rim light on the lit side gives the "toy" edge highlight.
                half fresnel = 1.0h - saturate(dot(normalWS, viewDirWS));
                half rim = smoothstep(_RimThreshold - 0.02h, _RimThreshold + 0.02h, fresnel);
                rim *= saturate(dot(normalWS, mainLight.direction) * 0.5h + 0.5h);
                color += _RimColor.rgb * rim * _RimStrength;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

            #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
            #if defined(_GBUFFER_NORMALS_OCT)
                float3 normalWS = normalize(input.normalWS);
                float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                return half4(PackFloat2To888(remappedOctNormalWS), 0.0);
            #else
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            #endif
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
