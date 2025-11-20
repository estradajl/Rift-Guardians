Shader "Custom/SectionFillerOptimized"
{
    Properties
    {
        //--------------------------------------------------
        // APPEARANCE — BASE + EDGE
        //--------------------------------------------------
        _FillColor("Fill Color", Color) = (0,1,1,0.3)
        _EdgeColor("Edge Glow", Color) = (0.2,1,1,1)
        _EdgePower("Edge Power", Range(0.1,10)) = 3

        //--------------------------------------------------
        // FX SETTINGS — SCAN / PULSE / SHELL
        //--------------------------------------------------
        _FillerMode("Filler Mode (0=Scan 1=Pulse 2=Shell)", Float) = 0
        _FXSpeed("FX Speed", Float) = 1.0
        _FXDensity("FX Density", Float) = 2.0
        _FXStrength("FX Strength", Float) = 1.0

        //--------------------------------------------------
        // CLIPPING PARAMETERS
        //--------------------------------------------------
        _ClipMode("Clip Mode (0=Sphere 1=Box 2=Hemisphere)", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
        _Thickness("Section Thickness", Float) = 0.04

        //--------------------------------------------------
        // HOLOGRAPHIC SHELL OVERRIDE
        //--------------------------------------------------
        _ShellColor("Shell Color", Color) = (0,0.6,1,1)
        _ShellOpacity("Shell Opacity", Range(0,1)) = 0.25
        _ShellEdgePower("Shell Edge Power", Range(0.1,10)) = 2
    }

    SubShader
    {
        Tags 
        { "RenderType"="Transparent"
          "Queue"="Transparent+100" 
        }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

        Pass
        {
            Name "SectionFillerCombinedFX"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //--------------------------------------------------
            // CONSTANT BUFFER FOR SRP BATCHER
            //--------------------------------------------------
            CBUFFER_START(UnityPerMaterial)
                float4 _FillColor;
                float4 _EdgeColor;
                float  _EdgePower;

                float  _FillerMode;
                float  _FXSpeed;
                float  _FXDensity;
                float  _FXStrength;

                float  _ClipMode;
                float3 _ClipCenter;
                float3 _ClipSize;
                float  _Thickness;

                float4 _ShellColor;
                float  _ShellOpacity;
                float  _ShellEdgePower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float3 posWS  : TEXCOORD0;
            };

            //--------------------------------------------------
            // CLIPPING FUNCTION
            //--------------------------------------------------
            float GetClipDist(float3 wp)
            {
                float3 local = wp - _ClipCenter;

                if (_ClipMode < 0.5)     // Sphere
                    return length(local) - _ClipSize.x;

                if (_ClipMode < 1.5)     // Box
                {
                    float3 q = abs(local) - _ClipSize;
                    return max(max(q.x, q.y), q.z);
                }

                if (local.y < 0) return 9999.0; // Hemisphere base
                return length(local) - _ClipSize.x;
            }

            //--------------------------------------------------
            // VERTEX
            //--------------------------------------------------
            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.posHCS = TransformWorldToHClip(OUT.posWS);

                return OUT;
            }

            //--------------------------------------------------
            // FRAGMENT
            //--------------------------------------------------
            float4 Frag(Varyings IN) : SV_Target
            {
                float clipDist = GetClipDist(IN.posWS);

                if (clipDist < 0) discard;
                if (clipDist > _Thickness) discard;

                half t = (half)(1.0 - saturate(clipDist / _Thickness));

                half3 col = (half3)_FillColor.rgb;
                half alpha = (half)(_FillColor.a * t);

                col += (half3)_EdgeColor.rgb * pow(t, _EdgePower);

                half radialDist = (half)length(IN.posWS - _ClipCenter);

                //----------------------------
                // MODE 0 — SCAN
                //----------------------------
                if (_FillerMode < 0.5)
                {
                    half scan = (half)sin(radialDist * _FXDensity + _Time.y * _FXSpeed);
                    scan = saturate(scan * 0.5 + 0.5);
                    scan *= t;
                    col += scan * _FXStrength;
                }
                //----------------------------
                // MODE 1 — PULSE
                //----------------------------
                else if (_FillerMode < 1.5)
                {
                    half pulse = (half)sin(radialDist * _FXDensity - _Time.y * _FXSpeed);
                    pulse = saturate(pulse * 0.5 + 0.5);
                    pulse *= t;
                    col += pulse * _FXStrength;
                }
                //----------------------------
                // MODE 2 — SHELL
                //----------------------------
                else
                {
                    col = (half3)_ShellColor.rgb;
                    alpha = (half)(t * _ShellOpacity);
                    col += (half3)_EdgeColor.rgb * pow(t, _ShellEdgePower) * 0.35;
                }

                return float4(col, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
