Shader "Custom/PortalSurfaceFX"
{
    Properties
    {
        _NoiseTex("Wave Noise Texture", 2D) = "gray" {}
        _WaveStrength("Wave Strength", Range(0,0.2)) = 0.05
        _WaveFrequency("Wave Frequency", Range(0.5,5)) = 1.5
        _WaveSpeed("Wave Speed", Range(0.1,4)) = 1
        _TintColor("Tint Color", Color) = (0.2,0.6,1,0.5)
        _Emission("Emission", Range(0,5)) = 1
        _StencilID("Stencil ID", Range(0,255)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent+15"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "PortalDistortion"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            Stencil
            {
                Ref [_StencilID]
                Comp Equal
                Pass Keep
                Fail Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            sampler2D _NoiseTex;

            float4 _TintColor;
            float _WaveStrength;
            float _WaveFrequency;
            float _WaveSpeed;
            float _Emission;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos = worldPos;
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // Ondas radiales saliendo del centro
                float2 center = float2(0.5, 0.5);
                float2 dir = IN.uv - center;
                float dist = length(dir);

                // Animar las ondas
                float wave = sin(dist * _WaveFrequency * 20 - _Time.y * _WaveSpeed * 10) * _WaveStrength;

                // Añadir ruido
                float noise = tex2D(_NoiseTex, IN.uv * 3 + _Time.y * 0.2).r;
                wave += (noise - 0.5) * _WaveStrength * 0.5;

                // Distorsionar UVs
                float2 distortedUV = IN.uv + normalize(dir) * wave;

                // Muestrear escena detrás
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV);

                // Color final con tinte energético
                half4 final = sceneColor + _TintColor * _Emission * wave * 3;
                final.a = saturate(abs(wave) * 10 + 0.1);
                return final;
            }
            ENDHLSL
        }
    }
}
