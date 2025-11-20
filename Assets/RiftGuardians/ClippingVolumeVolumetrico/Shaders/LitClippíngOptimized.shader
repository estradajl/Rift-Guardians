Shader "Custom/LitClippingOptimized"
{
    Properties
    {
        //--------------------------------------------------
        // SURFACE / BASE MAP
        //--------------------------------------------------
        _BaseMap ("Albedo Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        //--------------------------------------------------
        // NORMALS
        //--------------------------------------------------
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0,2)) = 1.0

        //--------------------------------------------------
        // MATERIAL REFLECTANCE
        //--------------------------------------------------
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        //--------------------------------------------------
        // EMISSION
        //--------------------------------------------------
        _EmissionMap("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)

        //--------------------------------------------------
        // CLIPPING
        //--------------------------------------------------
        _ClipMode("Clip Mode", Float) = 0
        _ClipCenter("Clip Center", Vector) = (0,0,0,0)
        _ClipSize("Clip Size", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "LitClipped"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            sampler2D _BaseMap;
            sampler2D _BumpMap;
            sampler2D _EmissionMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissionColor;

                float _BumpScale;
                float _Metallic;
                float _Smoothness;

                float _ClipMode;
                float3 _ClipCenter;
                float3 _ClipSize;
            CBUFFER_END

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

            //--------------------------------------------------
            // CLIPPING SHAPES
            //--------------------------------------------------
            bool ClipVolume(float3 posWS)
            {
                float3 local = posWS - _ClipCenter;

                if (_ClipMode < 0.5)
                    return dot(local, local) > (_ClipSize.x * _ClipSize.x);

                if (_ClipMode < 1.5)
                {
                    float3 d = abs(local) - _ClipSize;
                    return any(d > 0);
                }

                if (local.y < 0) return true;
                return dot(local, local) > (_ClipSize.x * _ClipSize.x);
            }

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = positionWS;

                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS  = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;

                OUT.normalWS    = normalWS;
                OUT.tangentWS   = tangentWS;
                OUT.bitangentWS = bitangentWS;

                OUT.uv = IN.uv;
                OUT.positionHCS = TransformWorldToHClip(positionWS);

                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                if (ClipVolume(IN.positionWS))
                    discard;

                float4 albedoTex = tex2D(_BaseMap, IN.uv);
                float3 albedo = albedoTex.rgb * _BaseColor.rgb;

                float3 normalTS = UnpackNormal(tex2D(_BumpMap, IN.uv)) * _BumpScale;

                float3x3 TBN = float3x3(
                    normalize(IN.tangentWS),
                    normalize(IN.bitangentWS),
                    normalize(IN.normalWS)
                );

                float3 normalWS = normalize(mul(normalTS, TBN));

                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                float3 diffuse = albedo * NdotL * mainLight.color;

                float3 halfDir = normalize(mainLight.direction + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));

                float specPower = lerp(8.0, 128.0, _Smoothness);
                float spec = pow(NdotH, specPower) * _Metallic;

                float3 emission = tex2D(_EmissionMap, IN.uv).rgb * _EmissionColor.rgb;

                float3 finalColor = diffuse + spec + emission;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
