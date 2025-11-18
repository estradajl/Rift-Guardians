Shader "Custom/ClippingVolumeMask"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry-1"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "StencilMask"

            ZWrite Off
            ColorMask 0
            ZTest Always
            Cull Off

            Stencil
            {
                Ref 1
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -----------------------------------------------------
            // CORRECT INCLUDE (URP 14–17)
            // This gives us: TransformObjectToHClip()
            // -----------------------------------------------------
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Safe transform for all URP versions
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // No color writes, stencil only
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
