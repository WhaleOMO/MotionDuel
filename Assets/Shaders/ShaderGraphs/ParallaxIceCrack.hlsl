#pragma once

void ParallaxIceCrack_float(float NumLayers, float CrackDepth, 
                            UnityTexture2D CrackMap, 
                            float2 UV, float3 ViewDirTangent,
                            out float3 CrackColor)
{
    float parallax = 0;
    [loop]
    for(int layer = 1; layer < NumLayers; layer++)
    {
        float ratio = (float) layer / NumLayers;
        float offset = lerp(0, CrackDepth, ratio);
        float opacity = lerp(1, 0, ratio);
        float2 uv = UV + normalize(ViewDirTangent) * offset;
        parallax += SAMPLE_TEXTURE2D(CrackMap, CrackMap.samplerstate, uv).r * pow(opacity, 0.5);
    }
    parallax = parallax * (1.0 / NumLayers);
    CrackColor = parallax.rrr;
}
