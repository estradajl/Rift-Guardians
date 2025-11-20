Shader "Custom/SectionFillerCombined"
{
    Properties
    {
        // --- BASE COLORS ---
        _FillColor("Fill Color", Color) = (0,1,1,0.3)
        _EdgeColor("Edge Glow", Color) = (0.2,1,1,1)
        _EdgePower("Edge Power", Range(0.1,10)) = 3

        // --- FX CONTROL ---
        _FillerMode("Filler Mode (0=Scan 1=Pulse 2=Shell)", Float) = 0
        _FXSpeed("FX Speed", Float) = 1.0
        _FXDensity("FX Density", Float) = 2.0
        _FXStrength("FX Strength", Float) = 1.0

        // --- CLIPPING ---
        _ClipMode("Clip Mode (0=Sphere 1=Box 2=Hemisphere)", Float) = 0

        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
        _Thickness("Section Thickness", Float) = 0.04

        // --- SOLID SHELL ---
        _ShellColor("Shell Color", Color) = (0,0.6,1,1)
        _ShellOpacity("Shell Opacity", Range(0,1)) = 0.25
        _ShellEdgePower("Shell Edge Power", Range(0.1,10)) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SectionFillerCombinedFX"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _FillColor;
            float4 _EdgeColor;
            float _EdgePower;

            float _FillerMode;
            float _FXSpeed;
            float _FXDensity;
            float _FXStrength;

            float _ClipMode;
            float3 _ClipCenter;
            float3 _ClipSize;
            float _Thickness;

            float4 _ShellColor;
            float _ShellOpacity;
            float _ShellEdgePower;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS    : NORMAL;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float3 posWS  : TEXCOORD0;
            };

            float GetClipDist(float3 wp)
            {
                float3 local = wp - _ClipCenter;

                // Sphere
                if (_ClipMode < 0.5)
                {
                    return length(local) - _ClipSize.x;
                }

                // Box
                if (_ClipMode < 1.5)
                {
                    float3 q = abs(local) - _ClipSize;
                    return max(max(q.x, q.y), q.z);
                }

                // Hemisphere
                if (local.y < 0) return 9999;
                return length(local) - _ClipSize.x;
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.posHCS = TransformWorldToHClip(OUT.posWS);
                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float clipDist = GetClipDist(IN.posWS);

                // Only draw within the slice thickness
                if (clipDist < 0) discard;
                if (clipDist > _Thickness) discard;

                float t = 1 - saturate(clipDist / _Thickness);

                float3 col = _FillColor.rgb;
                float alpha = _FillColor.a * t;

                // Edge glow
                col += _EdgeColor.rgb * pow(t, _EdgePower);

                float radialDist = length(IN.posWS - _ClipCenter);

                // Mode 0 — Radial Scan (rings moving outwards)
                if (_FillerMode < 0.5)
                {
                    float scan = sin(radialDist * _FXDensity + _Time.y * _FXSpeed);
                    scan = saturate(scan * 0.5 + 0.5);
                    scan *= t;
                    col += scan * _FXStrength;
                }
                // Mode 1 — Global Pulse (breathing effect)
                else if (_FillerMode < 1.5)
                {
                    float pulse = sin(radialDist * _FXDensity - _Time.y * _FXSpeed);
                    pulse = saturate(pulse * 0.5 + 0.5);
                    pulse *= t;
                    col += pulse * _FXStrength;
                }
                // Mode 2 — Solid Shell
                else
                {
                    col = _ShellColor.rgb;
                    alpha = t * _ShellOpacity;
                    col += _EdgeColor.rgb * pow(t, _ShellEdgePower) * 0.35;
                }

                return float4(col, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
