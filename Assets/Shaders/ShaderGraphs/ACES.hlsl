#pragma once

void ACES_float(float3 colorIn, out float3 color){	
    float3x3 m1 = float3x3(
        float3(0.59719, 0.07600, 0.02840),
        float3(0.35458, 0.90834, 0.13383),
        float3(0.04823, 0.01566, 0.83777)
    );
    float3x3 m2 = float3x3(
        float3(1.60475, -0.10208, -0.00327),
        float3(-0.53108,  1.10813, -0.07276),
        float3(-0.07367, -0.00605,  1.07602)
    );
    float3 v = mul(m1,colorIn);    
    float3 a = v * (v + 0.0245786) - 0.000090537;
    float3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
    color = pow(clamp(mul(m2, (a/b)), 0.0, 1.0), (1.0 / 2.2));	
}
