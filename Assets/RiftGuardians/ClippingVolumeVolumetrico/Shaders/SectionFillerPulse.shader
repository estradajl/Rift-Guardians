Shader "Custom/SectionFillerPulse"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (0,0.9,1,0.25)
        _EdgeColor("Edge Glow", Color) = (0,1,1,1)
        _EdgePower("Edge Power", Range(0.1,10)) = 3

        _PulseSpeed("Pulse Speed", Float) = 3.0

        _ClipMode("Clip Mode", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
        _Thickness("Surface Thickness", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _FillColor,_EdgeColor;
            float _EdgePower,_PulseSpeed;
            float _ClipMode; float3 _ClipCenter,_ClipSize;
            float _Thickness;

            struct Attributes { float4 posOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 posHCS:SV_POSITION; float3 posWS:TEXCOORD0; };

            float GetClipDist(float3 wp)
            {
                float3 local = wp - _ClipCenter;
                if(_ClipMode==0) return length(local)-_ClipSize.x;
                if(_ClipMode==1){ float3 q = abs(local)-_ClipSize; return max(max(q.x,q.y),q.z); }
                if(_ClipMode==2){ if(local.y<0) return 1; return length(local)-_ClipSize.x; }
                return 1;
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posWS = TransformObjectToWorld(IN.posOS.xyz);
                OUT.posHCS = TransformWorldToHClip(OUT.posWS);
                return OUT;
            }

            float4 Frag(Varyings IN):SV_Target
            {
                float d = GetClipDist(IN.posWS);
                if(d<-_Thickness || d>_Thickness) discard;

                float t = 1 - saturate(abs(d/_Thickness));

                float dist = length(IN.posWS - _ClipCenter);
                float pulse = sin(dist*6 + _Time.y * _PulseSpeed);
                pulse = saturate(pulse * 0.5 + 0.5);

                float3 color =
                    _FillColor.rgb +
                    _EdgeColor.rgb * pow(t,_EdgePower) +
                    pulse * 0.25;

                return float4(color, t * _FillColor.a);
            }

            ENDHLSL
        }
    }
}
