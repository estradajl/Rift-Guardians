Shader "Custom/WorldClippingURP"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)

        [Toggle] _UseClip("Use Clipping", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            // Instancing & stereo (single-pass instanced friendly)
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _UseClip;
            CBUFFER_END

            // --- Global clipping uniforms (seteados desde ClippingVolume.cs) ---
            float4   _ClipCenter;      // xyz = center (world space)
            float4   _ClipBoxSize;     // xyz = box half-sizes or full sizes (ver código)
            float    _ClipRadius;      // sphere / cylinder radius
            float    _ClipHeight;      // cylinder height (full)
            int      _ClipType;        // 0=Box, 1=Sphere, 2=Hemisphere, 3=Cylinder
            float4x4 _ClipRotation;    // rotation matrix (worldFromLocal)

            // -------------------------------------------------------------------
            //      Helpers
            // -------------------------------------------------------------------

            // Convierte worldPos a espacio local del volumen
            float3 ToClipLocal(float3 worldPos)
            {
                // offset desde el centro
                float3 offset = worldPos - _ClipCenter.xyz;

                // R = matriz de rotación (local -> world)
                float3x3 R = (float3x3)_ClipRotation;
                // Inversa de R (como es rotación ortonormal, inv = transpose)
                float3x3 Rinv = transpose(R);

                // local = R^T * (world - center)
                float3 localPos = mul(Rinv, offset);
                return localPos;
            }

            bool IsInsideVolume(float3 worldPos)
            {
                float3 localPos = ToClipLocal(worldPos);

                // Por conveniencia, usamos half extents en box
                float3 halfSize = _ClipBoxSize.xyz * 0.5;

                if (_ClipType == 0)
                {
                    // BOX
                    if (abs(localPos.x) > halfSize.x) return false;
                    if (abs(localPos.y) > halfSize.y) return false;
                    if (abs(localPos.z) > halfSize.z) return false;
                    return true;
                }
                else if (_ClipType == 1)
                {
                    // SPHERE
                    float distSq = dot(localPos, localPos);
                    return distSq <= (_ClipRadius * _ClipRadius);
                }
                else if (_ClipType == 2)
                {
                    // HEMISPHERE (parte "superior" del eje Y local)
                    float distSq = dot(localPos, localPos);
                    bool insideSphere = distSq <= (_ClipRadius * _ClipRadius);
                    bool abovePlane  = localPos.y >= 0.0;
                    return insideSphere && abovePlane;
                }
                else if (_ClipType == 3)
                {
                    // CYLINDER (eje Y local)
                    float2 radial = localPos.xz;
                    float radialSq = dot(radial, radial);
                    float halfH = _ClipHeight * 0.5;

                    if (radialSq > (_ClipRadius * _ClipRadius)) return false;
                    if (abs(localPos.y) > halfH) return false;
                    return true;
                }

                // Si el tipo no es válido, no recortamos
                return true;
            }

            // -------------------------------------------------------------------
            //      Vertex
            // -------------------------------------------------------------------
            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos   = posWS;
                OUT.positionCS = TransformWorldToHClip(posWS);

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            // -------------------------------------------------------------------
            //      Fragment
            // -------------------------------------------------------------------
            half4 Frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // Clipping volumétrico
                if (_UseClip > 0.5)
                {
                    if (!IsInsideVolume(IN.worldPos))
                        discard;
                }

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float4 finalColor = texColor * _BaseColor;

                return finalColor;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
