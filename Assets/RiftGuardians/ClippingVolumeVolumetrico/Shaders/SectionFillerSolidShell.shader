Shader "Custom/SectionFillerSolidShell"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (0,0.8,1,0.3)
        _EdgeColor("Edge Glow", Color) = (0.4,1,1,1)
        _EdgePower("Edge Power", Range(0.1,10)) = 4

        _ClipMode("Clip Mode", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
        _Thickness("Thickness", Float) = 0.05
    }

    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
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
            float _EdgePower,_ClipMode,_Thickness;
            float3 _ClipCenter,_ClipSize;

            struct Attributes{float4 posOS:POSITION; float3 normalOS:NORMAL;};
            struct Varyings{float4 posHCS:SV_POSITION; float3 posWS:TEXCOORD0;};

            float GetClipDist(float3 wp)
            {
                float3 l = wp-_ClipCenter;
                if(_ClipMode==0) return length(l)-_ClipSize.x;
                if(_ClipMode==1){float3 q=abs(l)-_ClipSize; return max(max(q.x,q.y),q.z);}
                if(_ClipMode==2){if(l.y<0)return 1; return length(l)-_ClipSize.x;}
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
                if(d<-_Thickness||d>_Thickness) discard;

                float t = 1 - saturate(abs(d/_Thickness));

                float3 color =
                    _FillColor.rgb +
                    _EdgeColor.rgb * pow(t,_EdgePower);

                return float4(color, t * _FillColor.a);
            }

            ENDHLSL
        }
    }
}
