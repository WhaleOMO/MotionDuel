Shader "Custom/Sky"
{
    Properties
    {
        [Header(BaseGradient)]
        [HDR]_DayMidColor("Day Mid", color) = (0.86, 0.89, 1, 1)
        [HDR]_DayZenithColor("Day Zenith", color) = (0.27, 0.43, 1, 1)
        [HDR]_NightMidColor("Night Mid", color) = (0.5, 0.62, 0.64, 1)
        [HDR]_NightZenithColor("Night Zenith", color) = (0.118, 0.2, 0.51, 1)
        [Header(Horizon)]
        [HDR]_DayHorizonColor("Day Horizon", color) = (0.86, 0.89, 1, 1)
        [HDR]_NightHorizonColor("Night Horizon", color) = (0.5, 0.62, 0.64, 1)
        [Header(Mie Scattering)]
        [HDR]_MieScatteringColor("Mie Scatter Color", color) = (1.0, 1.0, 0.0, 1.0)
        [Header(Sun)]
        _SunSize("Sun Size", float) = 1.0
        [HDR]_SunColor("Sun Color", color) = (1, 1, 1, 1)
        [Header(NightSky)]
        _StarMap("Star Map", 2D) = "black"{}
        _StarNoise("Star Noise", 2D) = "white"{}
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float _SunSize;
            float3 _DayMidColor, _DayZenithColor, _NightMidColor, _NightZenithColor, _MieScatteringColor;
            float3 _DayHorizonColor, _NightHorizonColor;
            float3 _SunColor;

            Texture2D _StarMap; SAMPLER(sampler_StarMap); float4 _StarMap_ST;
            Texture2D _StarNoise; SAMPLER(sampler_StarNoise); float4 _StarNoise_ST;
            
            struct Attributes
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
            };

            #define DensityScaleHeight float(1200)  // Mie's scaleHeight
            #define PlanetRadius float(6357000)     // Earth's PlanetRadius
            #define PlanetCenter float3(0, -PlanetRadius, 0)
            
            float RaySphereIntersection(float3 rayStart, float3 rayDir, float3 planetCenter, float radius)
            {
                const float L = length(planetCenter - rayStart);
                const float tca = dot(planetCenter - rayStart, rayDir);
                const float d  = sqrt(L * L - tca * tca);
                const float thc = sqrt(radius * radius - d * d);

                if (d > radius) return -1;    // no intersection

                // find min distance (first intersect distance)
                const float t1 = tca - thc;
                const float t2 = tca + thc;
                const float t = (t1 < 0) ? t2 : t1;
                return t;
            }

            float MiePhase(float g, float cosVL)
            {
                float g2 = g * g;
                return (1.0 / (4.0 * PI)) * (1.0 - g2) / pow( abs(1.0 + g2 - 2.0 * g * cosVL), 1.5);
            }

            float MiePhaseCS(float g, float cosVL)
            {
                float g2 = g * g;
                return (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g2)) / (2.0 * (2.0 + g2))) * ((1 + cosVL * cosVL) / (pow((1 + g2 - 2 * g * cosVL), 1.5)));
            }

            void GetLocalDensity(float3 pos, out float localDensity)
            {
                float height = distance(pos, PlanetCenter) - PlanetRadius;
                localDensity = exp(-(height/DensityScaleHeight));
            }
            
            // Only Mie Scattering
            // Simplified, assuming light intensity arriving at P is the same as light original intensity
            // Thus Ip = Is * Transmittance(A,P) * ScatterMie(lightDir, rayDir) * ArtistSpecifiedColor  
            float3 IntegrateInscattering(float3 rayStart, float3 rayDir, float rayLength, float3 lightDir, int sampleCount)
            {
                #define Extinction float(4.4 * 1e-6)    // Mie's Absorption
                
                float3 step = normalize(rayDir) * (rayLength / (float)sampleCount);
                float stepSize = length(step);

                float mieScatter = 0;
                float localDensity = 0;
                float extinctionTowardsRay = 0;        // accumulated density

                float3 p = rayStart + 0.5 * step;      // start at middle point
                
                for (int i = 0; i < sampleCount; i+=1)
                {
                    GetLocalDensity(p, localDensity);
                    extinctionTowardsRay += localDensity * stepSize;
                    mieScatter += exp(-extinctionTowardsRay * Extinction) * localDensity;
                    p += step;
                }
                
                return _MieScatteringColor * mieScatter * MiePhaseCS(0.9, dot(rayDir, lightDir));
            }
            
            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half3 color = 0;
                Light mainLight = GetMainLight();
                float sunNightStep = smoothstep(-0.3, 0.25, mainLight.direction.y);
                // Base Gradient
                float3 gradientDay = lerp(_DayMidColor, _DayZenithColor, saturate(abs(i.uv.y)));
                float3 gradientNight = lerp(_NightMidColor, _NightZenithColor, saturate(i.uv.y));
                float3 skyGradient = lerp(gradientNight, gradientDay, sunNightStep);
                // Horizon
                float horizonWidth = 0.3;
                float horizonMask = smoothstep(-horizonWidth, 0, i.uv.y) * smoothstep(-horizonWidth, 0, -i.uv.y);
                float3 horizonGradient = lerp(_NightHorizonColor, _DayHorizonColor, sunNightStep) * saturate(horizonMask);
                // Mie Inscattering
                float3 rayStart = _WorldSpaceCameraPos.xyz;
                float3 rayDir = normalize(i.uv.xyz);
                float disToAtmosphere = RaySphereIntersection(rayStart, rayDir, PlanetCenter, PlanetRadius + 100000);
                float disToPlanet = RaySphereIntersection(rayStart, rayDir, PlanetCenter, PlanetRadius);
                // float rayDistance = (disToPlanet > 0) ? min(disToPlanet * 100000, disToAtmosphere) : disToAtmosphere;
                float3 inscattering = IntegrateInscattering(rayStart, rayDir, disToAtmosphere, mainLight.direction, 16);
                inscattering *= smoothstep(0, 0.2, 0.25 - abs(mainLight.direction.y));
                // Sun Disk
                float sunDistance = distance(i.uv.xyz, mainLight.direction);
                float sunArea = 1.0 - (sunDistance / _SunSize);
                sunArea = smoothstep(0.6, 1, sunArea);
                // Star
                float starMask = lerp((1 - smoothstep(-0.7, -0.2, -i.uv.y)), 0, sunNightStep);
                float starNoise = SAMPLE_TEXTURE2D(_StarNoise, sampler_StarNoise, i.uv.xz * _StarNoise_ST.x + _Time.x * 0.2).r;
                float stars = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, i.uv.xz * _StarMap_ST.xy + _StarMap_ST.zw - _Time.x * 0.15) * starMask;
                float3 starsColor = smoothstep(0.2, 0.3, stars) * smoothstep(0.5, 0.8, starNoise);
                #pragma conece{ HeHeightMap, UnityTexture2D BaseColor()[}, oyut float3 CrackColorreturn 0;Textur2e2Dvoid ;craCracjmkColor = 0;// Construct TBN matrxiix to tranform viewDir to tangent spaceParallaxvfloat3 WorldTangent, float3 wWorldBitangent, float3 wWorldNormal, matfloat3x3  TBN = float3x3(WorldTangent, WorldBitangent, WOrlorldNormal);float3 viewDirTsSS = mul(TBN, float3 WVieViewDir, TSangent,float parallax = 0;for(int layer = 0; layer < numNUMumLayers; layer+ ++__++){}[loop]float ratio = (float) layer / NumLayers;float offset  == L;Elerp(0, ssCrackdDepth, ratio());float fade = lerp(1, opacity = lerp(1, 0, ratio);parallax += ParallaxColorfloat2, UV, float2 uv = UV + normalize(viViewDirTangent) * offset; * popacity.parallax /= ppaarallax * (1.0 . / NUumLayers);return floaCrackColor = paralalax.rrr;Unityuv * 1 * pow(opacity, 24) 0.5)
                color = horizonGradient + skyGradient + clamp(inscattering, 0, 100) +
                        starsColor + sunArea * _SunColor;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
