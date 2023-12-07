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
        [Header(NightSky)]
        _StarMap("Star Map", 2D) = "black"{}
        _StarNoise("Star Noise", 2D) = "white"{}
    }
    
    SubShader
    {
        Tags { "Queue" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _DayMidColor, _DayZenithColor, _NightMidColor, _NightZenithColor, _MieScatteringColor;
            float3 _DayHorizonColor, _NightHorizonColor;

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

            void GetLocalDensity(float3 pos, out float localDensity)
            {
                float height = distance(pos, PlanetCenter) - PlanetRadius;
                localDensity = 3.99 * 1e-6 * exp(-(height/DensityScaleHeight));
            }
            
            // Only Mie Scattering
            // Simplified, assuming light intensity arriving at P is the same as light original intensity
            // Thus Ip = Is * Transmittance(A,P) * ScatterMie(lightDir, rayDir) * ArtistSpecifiedColor  
            float3 IntegrateInscattering(float3 rayStart, float3 rayDir, float rayLength, float3 lightDir, int sampleCount)
            {
                #define Extinction float(4.4 * 1e-6)    // Mie's Absorption
                
                float3 step = rayDir * (rayLength / sampleCount);
                float3 stepSize = length(step);

                float scatterMie = 0;               // final scatter result
                float densityTowardsRay = 0;        // accumulated density

                float localDensity = 0;
                float prevLocalDensity = 0;
                float prevTransmittance = 0;

                // for p at ray start
                GetLocalDensity(rayStart, localDensity);
                densityTowardsRay += localDensity * stepSize;
                prevLocalDensity = localDensity;

                float Transmittance = exp(-(densityTowardsRay) * Extinction) * localDensity;
                prevTransmittance = Transmittance;

                for (int i = 1; i < sampleCount; i+=1)
                {
                    float3 P = rayStart + step * i;         // ray march

                    GetLocalDensity(P, localDensity);
                    densityTowardsRay += (prevLocalDensity + localDensity) * stepSize / 2;
                    Transmittance = exp(-(densityTowardsRay) * Extinction) * localDensity;
                    scatterMie += (prevTransmittance + Transmittance) * stepSize / 2;

                    prevTransmittance = Transmittance;
                    prevLocalDensity = localDensity;
                }

                scatterMie = scatterMie * MiePhase(0.9, dot(rayDir, lightDir));
                
                return float3(scatterMie * _MieScatteringColor);
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
                float3 rayStart = float3(0,1,0);
                float3 rayDir = normalize(i.uv.xyz);
                float disToAtmosphere = RaySphereIntersection(rayStart, rayDir, PlanetCenter, PlanetRadius + 100000);
                float disToPlanet = RaySphereIntersection(rayStart, rayDir, PlanetCenter, PlanetRadius);
                float rayDistance = disToPlanet > 0 ? min(disToPlanet, disToAtmosphere) : disToAtmosphere;
                float3 inscattering = IntegrateInscattering(rayStart, rayDir, rayDistance, mainLight.direction, 16);
                // Star
                float starMask = lerp((1 - smoothstep(-0.7, -0.2, -i.uv.y)), 0, sunNightStep);
                float starNoise = SAMPLE_TEXTURE2D(_StarNoise, sampler_StarNoise, i.uv.xz * _StarNoise_ST.x + _Time.x * 0.2).r;
                float stars = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, i.uv.xz * _StarMap_ST.xy + _StarMap_ST.zw - _Time.x * 0.15) * starMask;
                float3 starsColor = smoothstep(0.2, 0.3, stars) * smoothstep(0.5, 0.8, starNoise);

                //return half4(inscattering, 1.0);
                return half4(horizonGradient + skyGradient + inscattering + starsColor, 1.0);
            }
            ENDHLSL
        }
    }
}
