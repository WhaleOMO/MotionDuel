Shader "Custom/SSR"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "SSRPass"
            
            HLSLPROGRAM
            #pragma vertex VertNoScaleBias
            #pragma fragment SSRFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float _MaxSteps;
            float _MaxDistance;
            float _Thickness;
            float _ReflectionStride;
            float _ReflectionJitter;
            
            Varyings VertNoScaleBias(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif

                output.positionCS = pos;
                output.texcoord = uv; // * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return output;
            }
            
            float4 SSRFrag (Varyings input) : SV_Target 
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Get Scene Depth
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(input.texcoord);
                #else
                    // Adjust z to match NDC for OpenGL
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(input.texcoord));
                #endif

                // Get Scene Normal (World Space)
                float3 normalWS = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_LinearClamp, input.texcoord).xyz;
                // Transform to View Space
                float3 normalVS = TransformWorldToViewDir(normalWS, true);
                // Reconstruct world space positions
                // NDC --> Clip --> WorldSpace
                float3 posWS = ComputeWorldSpacePosition(input.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 posVS = TransformWorldToView(posWS);

                // reflect ray in view space
                float3 reflectDir = normalize(reflect(normalize(posVS), normalVS));
                float3 startView = posVS;
                float3 endView = posVS + (reflectDir * _MaxDistance); //_MaxDistance);

                // Start pos and end pos to screen space (to optimize ray marching process)
                float2 texSize = _ScreenParams.xy; //_BlitTextureSize;
                float4 startFrag = mul(UNITY_MATRIX_P, float4(startView,1.0));
                startFrag = startFrag / startFrag.w;
                startFrag.xy = startFrag.xy * 0.5 + 0.5;
                startFrag.y = 1.0 - startFrag.y;
                startFrag.xy = startFrag.xy * texSize;
                
                float4 endFrag = mul(UNITY_MATRIX_P, float4(endView,1.0));
                endFrag = endFrag / endFrag.w;
                endFrag = endFrag / endFrag.w;
                endFrag.xy = endFrag.xy * 0.5 + 0.5;
                endFrag.y = 1.0 - endFrag.y;
                endFrag.xy = endFrag.xy * texSize; 

                float deltaX = endFrag.x - startFrag.x;
                float deltaY = endFrag.y - startFrag.y;
                // Find longer delta
                float useX = abs(deltaX) >= abs(deltaY) ? 1.0 : 0.0;
                float delta = lerp(abs(deltaY), abs(deltaX), useX) * _ReflectionStride;
                float2 increment = float2(deltaX, deltaY) / max(delta, 0.001);

                float2 frag = startFrag.xy;
                frag += increment * _ReflectionJitter;

                float search0 = 0, search1 = 0;
                float hit0 = 0, hit1 = 0;
                
                // First Pass
                // Trying to hit a surface
                UNITY_LOOP
                for(int i = 0; i < int(delta); i++)
                {
                    frag += increment;
                    if (frag.x < 0 || frag.y < 0 ||
                        frag.x > texSize.x || frag.y > texSize.y) break;

                    float2 fragUV = frag / texSize;
                    float fragDepth = LinearEyeDepth(SampleSceneDepth(fragUV), _ZBufferParams);

                    search1 = lerp((frag.y - startFrag.y) / deltaY, (frag.x - startFrag.x) / deltaX, useX);
                    search1 = clamp(search1, 0.0, 1.0);
                    
                    // unity's view space depth is negative
                    // perspective-correct interpolation of depth
                    float viewDepth = _ProjectionParams.x * (startView.z * endView.z) / lerp(endView.z, startView.z, search1);
                    float deltaDepth = viewDepth - fragDepth;
                    
                    if (deltaDepth>0 && deltaDepth < _Thickness)
                    {
                        hit0 = 1;
                        break;
                    }
                    search0 = search1;
                }

                // get mid point between two position
                search1 = search0 + ((search1 - search0) / 2.0);

                if (hit0 == 0)
                {
                    float2 fragUV = frag / texSize;
                    return float4(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, fragUV).rgb,1); 
                }
                
                float steps = _MaxSteps * hit0;
                // Second Pass, Using bisection method to find surface hit point
                UNITY_LOOP
                for (int step = 0; step < steps; step++)
                {
                    frag = lerp(startFrag.xy, endFrag.xy, search1);
                    if(frag.x < 0.0 || frag.y < 0.0 ||
                        frag.x > texSize.x || frag.y > texSize.y) break;

                    float2 fragUV = frag / texSize;
                    float fragDepth = LinearEyeDepth(SampleSceneDepth(fragUV), _ZBufferParams);
                
                    float viewDepth = _ProjectionParams.x*(startView.z * endView.z) / lerp(endView.z, startView.z, search1);
                    float deltaDepth = viewDepth - fragDepth;
                
                    if (deltaDepth > 0 && deltaDepth <  _Thickness * 0.1)
                    {
                        hit1 = 1;
                        search1 = search0 + ((search1 - search0) / 2);
                    }
                    else
                    {
                        float temp = search1;
                        search1 = search1 + ((search1 - search0) / 2);
                        search0 = temp;
                    }
                }

                float3 reflColor = 0;
                if(hit1 ==1)
                {
                    float2 fragUV = frag / texSize;
                    reflColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, fragUV).rgb;
                }
                
                return float4(reflColor,1.0);
            }
                
            ENDHLSL
        }
    }
}
