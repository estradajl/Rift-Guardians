Shader "Custom/UniversalDissolveFX"
{
    Properties
    {
        _BaseMap("Base Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _NoiseMap("Noise Texture", 2D) = "gray" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0
        _EdgeColor("Edge Color", Color) = (0.2,0.8,1,1)
        _EdgeWidth("Edge Width", Range(0,0.2)) = 0.05
        _Emission("Emission", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+20" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "DissolveFX"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            float4 _BaseColor;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _Emission;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half baseNoise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * 2).r;
                half dissolveEdge = baseNoise - _DissolveAmount;

                // base color
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // borde brillante
                half edge = smoothstep(0.0, _EdgeWidth, abs(dissolveEdge));
                half edgeGlow = saturate(1 - edge) * _Emission;

                // mezcla
                baseColor.rgb = lerp(baseColor.rgb, _EdgeColor.rgb, edgeGlow);
                baseColor.a = edgeGlow;

                // ocultar partes disueltas
                clip(dissolveEdge);

                return baseColor;
            }
            ENDHLSL
        }
    }
}
