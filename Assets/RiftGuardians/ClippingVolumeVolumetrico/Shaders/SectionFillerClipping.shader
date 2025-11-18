Shader "Custom/SectionFillerClipping"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (0,1,1,0.3)
        _EdgeColor("Edge Glow", Color) = (0.2,1,1,1)
        _EdgePower("Edge Power", Range(0.1,10)) = 3
        _ScrollSpeed("Scroll Speed", Float) = 1.0

        _ClipMode("Clip Mode", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)

        _Thickness("Surface Thickness", Float) = 0.04
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent+100"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SectionFiller"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // -----------------------------------------
            // PROPERTIES
            // -----------------------------------------
            float4 _FillColor;
            float4 _EdgeColor;
            float _EdgePower;
            float _ScrollSpeed;

            float _ClipMode;
            float3 _ClipCenter;
            float3 _ClipSize;
            float _Thickness;

            // -----------------------------------------
            // STRUCTS
            // -----------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float3 posWS  : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            // -----------------------------------------
            // SAME CLIP LOGIC AS YOUR LIT CLIPPING
            // -----------------------------------------
            float GetClipDist(float3 worldPos)
            {
                float3 local = worldPos - _ClipCenter;

                if (_ClipMode == 0)   // sphere
                {
                    return length(local) - _ClipSize.x;
                }
                else if (_ClipMode == 1) // box
                {
                    float3 q = abs(local) - _ClipSize;
                    return max(max(q.x, q.y), q.z);
                }
                else if (_ClipMode == 2) // hemisphere
                {
                    if (local.y < 0)
                        return 1.0; // fully outside below plane
                    return length(local) - _ClipSize.x;
                }

                return 1.0;
            }

            // -----------------------------------------
            // VERTEX
            // -----------------------------------------
            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.posHCS = TransformWorldToHClip(OUT.posWS);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                return OUT;
            }

            // -----------------------------------------
            // FRAGMENT
            // -----------------------------------------
            float4 Frag(Varyings IN) : SV_Target
            {
                float clipDist = GetClipDist(IN.posWS);

                // We want ONLY the cut surface:
                // region INSIDE the clip volume but very close to its boundary
                if (clipDist < -_Thickness)   // too deep inside → discard
                    discard;

                if (clipDist > _Thickness)    // fully outside → discard
                    discard;

                float t = saturate(1 - abs(clipDist / _Thickness));

                // Base color
                float4 col = _FillColor;

                // Edge highlight
                col.rgb += _EdgeColor.rgb * pow(t, _EdgePower);

                // Subtle animated effect
                col.rgb += 0.05 * sin(IN.posWS.y * 5 + _Time.y * _ScrollSpeed);

                col.a *= t;

                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
