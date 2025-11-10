Shader "Custom/ToonLitDissolver"
{
    Properties
    {
        // === Base Properties ===
        [MainTexture]_BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.2
        _Metallic("Metallic", Range(0,1)) = 0.0
        [Normal]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0,2)) = 1

        // === Toon Lighting ===
        _RampThreshold("Toon Threshold", Range(0,1)) = 0.5
        _RampSmoothness("Toon Smoothness", Range(0.001,1)) = 0.1
        _ShadowColor("Shadow Color", Color) = (0.3,0.3,0.3,1)

        // === Dissolve FX ===
        [Header(Dissolve FX)]
        _NoiseMap("Noise Texture", 2D) = "gray" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0
        _EdgeColor("Edge Color", Color) = (0.2,0.8,1,1)
        _EdgeWidth("Edge Width", Range(0,0.2)) = 0.05
        _Emission("Edge Emission", Range(0,10)) = 2

        [Toggle]_ZWrite("Write Depth", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Cull [_Cull]
        ZWrite [_ZWrite]

        Pass
        {
            Name "ToonLitDissolve"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Luces/sombras URP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            // Instancing + XR single-pass
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 tangentOS  : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // === Properties ===
            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);  SAMPLER(sampler_BumpMap);
            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);

            float4 _BaseColor;
            float  _Smoothness, _Metallic;
            float  _BumpScale;
            float  _RampThreshold, _RampSmoothness;
            float4 _ShadowColor;

            float  _DissolveAmount, _EdgeWidth, _Emission;
            float4 _EdgeColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);      // ✅ correcto para XR
                OUT.normalWS    = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.viewDirWS   = normalize(GetWorldSpaceViewDir(worldPos));
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN); // ✅ asegura ojo correcto

                // === Base textures ===
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo  = baseTex.rgb * _BaseColor.rgb;

                // === Toon lighting ===
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                float toonStep = smoothstep(_RampThreshold - _RampSmoothness,
                                            _RampThreshold + _RampSmoothness,
                                            NdotL);
                half3 lightColor = lerp(_ShadowColor.rgb, mainLight.color.rgb, toonStep);
                half3 color = albedo * lightColor;

                // === Dissolve effect ===
                half noise        = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * 2).r;
                half dissolveEdge = noise - _DissolveAmount;
                if (dissolveEdge < 0) discard; // oculta zonas disueltas

                // Borde energético
                half edgeMask = smoothstep(0.0, _EdgeWidth, abs(dissolveEdge));
                half edgeGlow = saturate(1 - edgeMask) * _Emission;
                color = lerp(color, _EdgeColor.rgb, edgeGlow);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
