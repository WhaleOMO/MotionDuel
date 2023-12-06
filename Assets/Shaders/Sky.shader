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

            float3 _DayMidColor, _DayZenithColor, _NightMidColor, _NightZenithColor;
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
                // Star
                float starMask = lerp((1 - smoothstep(-0.7, -0.2, -i.uv.y)), 0, sunNightStep);
                float starNoise = SAMPLE_TEXTURE2D(_StarNoise, sampler_StarNoise, i.uv.xz * _StarNoise_ST.x + _Time.x * 0.2).r;
                float stars = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, i.uv.xz * _StarMap_ST.xy + _StarMap_ST.zw - _Time.x * 0.15) * starMask;
                float3 starsColor = smoothstep(0.2, 0.3, stars) * smoothstep(0.5, 0.8, starNoise);
                
                return half4(horizonGradient + skyGradient + starsColor, 1.0);
            }
            ENDHLSL
        }
    }
}
