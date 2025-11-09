Shader "Custom/PortalStencilWriter"
{
    Properties
    {
        [IntRange]_StencilID("Stencil ID", Range(0,255)) = 1
        [Toggle]_ShowDebug("Show Debug Color", Float) = 0
        _DebugColor("Debug Color", Color) = (0,1,1,0.25)
        [Toggle]_ZWrite("Write Depth", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry+1"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "PortalStencilWriter"
            Cull [_CullMode]
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
                Fail Keep
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Debug color: left eye red, right eye blue (for quick stereo test)
                #if _ShowDebug
                    if (unity_StereoEyeIndex == 0)
                        return half4(_DebugColor.rg, _DebugColor.b, _DebugColor.a);
                    else
                        return half4(_DebugColor.bgr, _DebugColor.a);
                #else
                    return half4(0,0,0,0);
                #endif
            }
            ENDHLSL
        }
    }
}
