Shader "Custom/LitClipping"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)

        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0,2)) = 1.0

        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)

        _ClipMode("Clip Mode", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "LitClipped"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 tangentWS    : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
            };

            sampler2D _BaseMap;
            float4 _BaseColor;

            sampler2D _BumpMap;
            float _BumpScale;

            sampler2D _EmissionMap;
            float4 _EmissionColor;

            float _Metallic;
            float _Smoothness;

            float _ClipMode;
            float3 _ClipCenter;
            float3 _ClipSize;

            // -------------------------------------------------------------
            // CLIPPING FUNCTION
            // -------------------------------------------------------------
            bool ClipVolume(float3 posWS)
            {
                float3 local = posWS - _ClipCenter;

                if (_ClipMode == 0)          // Sphere
                {
                    return dot(local, local) > (_ClipSize.x * _ClipSize.x);
                }
                else if (_ClipMode == 1)     // Box
                {
                    float3 d = abs(local) - _ClipSize;
                    return any(d > 0);
                }
                else if (_ClipMode == 2)     // Hemisphere
                {
                    if (local.y < 0) return true;
                    return dot(local, local) > (_ClipSize.x * _ClipSize.x);
                }

                return false;
            }

            // -------------------------------------------------------------
            // VERTEX
            // -------------------------------------------------------------
            Varyings Vert (Attributes IN)
            {
                Varyings OUT;

                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);

                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 tWS = normalize(TransformObjectToWorldDir(IN.tangentOS.xyz));
                float3 bWS = cross(OUT.normalWS, tWS) * IN.tangentOS.w;
                OUT.tangentWS = tWS;
                OUT.bitangentWS = bWS;

                OUT.uv = IN.uv;

                return OUT;
            }

            // -------------------------------------------------------------
            // FRAGMENT (PBR)
            // -------------------------------------------------------------
            half4 Frag(Varyings IN) : SV_Target
            {
                if (ClipVolume(IN.positionWS))
                    discard;

                float3 albedo = tex2D(_BaseMap, IN.uv).rgb * _BaseColor.rgb;

                float3 normalTS = UnpackNormalScale(tex2D(_BumpMap, IN.uv), _BumpScale);
                float3 normalWS =
                    normalTS.x * IN.tangentWS +
                    normalTS.y * IN.bitangentWS +
                    normalTS.z * IN.normalWS;
                normalWS = normalize(normalWS);

                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);

                // Lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                float3 diffuse = albedo * NdotL * mainLight.color;

                // Simple PBR specular
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));
                float spec = pow(NdotH, lerp(8, 128, _Smoothness)) * _Metallic;

                float3 emission = tex2D(_EmissionMap, IN.uv).rgb * _EmissionColor.rgb;

                float3 color = diffuse + spec + emission;

                return float4(color, 1);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
