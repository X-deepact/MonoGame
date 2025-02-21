#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Time;
float Distortion;

sampler2D InputTexture : register(s0);

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Basic distortion effect
    float2 uv = texCoord;
    
    // Vertical wave distortion
    uv.x += sin(uv.y * 20.0 + Time * 5.0) * 0.02 * Distortion;
    
    // Simple scanlines
    float scanline = sin(uv.y * 200.0) * 0.1;
    
    // Sample the texture
    float4 color = tex2D(InputTexture, uv);
    
    // Apply scanline effect
    color.rgb -= scanline;
    
    return color;
}

technique TVDistortionTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 MainPS();
    }
} 