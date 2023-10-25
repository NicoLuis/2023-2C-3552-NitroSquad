﻿#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 WorldViewProjection;
float4x4 NormalWorldMatrix;

float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float Ka; // KAmbient
float Kd; // KDiffuse
float Ks; // KSpecular
float shininess;
float3 lightPosition;
float3 eyePosition; // Camera position
float2 Tiling;

static const float PI = 3.14159265359;

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
    MIPFILTER = LINEAR;
};

//Textura para Normals
texture NormalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Metallic
texture MetallicTexture;
sampler2D metallicSampler = sampler_state
{
    Texture = (MetallicTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Roughness
texture RoughnessTexture;
sampler2D roughnessSampler = sampler_state
{
    Texture = (RoughnessTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Ambient Occlusion
texture AoTexture;
sampler2D aoSampler = sampler_state
{
    Texture = (AoTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}


float DistributionGGX(float3 normal, float3 halfVector, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(normal, halfVector), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 normal, float3 view, float3 light, float roughness)
{
    float NdotV = max(dot(normal, view), 0.0);
    float NdotL = max(dot(normal, light), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = mul(input.Position, World);
    output.Normal = mul(input.Normal, NormalWorldMatrix);
    //output.Normal = mul(float4(normalize(input.Normal.xyz), 0.0), NormalWorldMatrix);
    output.TextureCoordinates = input.TextureCoordinates * Tiling;
	
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 L = normalize(lightPosition - input.WorldPosition.xyz); // light direction
    float3 V = normalize(eyePosition - input.WorldPosition.xyz); // view direction
    float3 H = normalize(L + V); // half vector
    float3 N = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, normalize(input.Normal.xyz)); // normal
    
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    
    float3 diffuseLight = Kd * diffuseColor * saturate(dot(N, L));
    float3 specularLight = Ks * specularColor * pow(dot(N, H), shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(ambientColor * Ka + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    return finalColor;
}


//Vertex Shader
VertexShaderOutput FullVS(VertexShaderInput input)
{
    VertexShaderOutput output;

	// Proyectamos la position
    output.Position = mul(input.Position, WorldViewProjection);

	// Propagamos las coordenadas de textura
    output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
    output.Normal = mul(input.Normal, NormalWorldMatrix);

	// Usamos la matriz de world para proyectar la posicion
    output.WorldPosition = mul(input.Position, World);

    return output;
}


//Pixel Shader
float4 FullPS(VertexShaderOutput input) : COLOR
{
    float3 albedo = pow(tex2D(textureSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
    float metallic = tex2D(metallicSampler, input.TextureCoordinates).r;
    float roughness = tex2D(roughnessSampler, input.TextureCoordinates).r;
    float ao = tex2D(aoSampler, input.TextureCoordinates).r;
    
    float3 V = normalize(eyePosition - input.WorldPosition.xyz);
    float3 N = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, (float3) input.Normal);
    float3 L = lightPosition - input.WorldPosition.xyz;
    float distance = length(L);
	L = normalize(L); // Normalize our light vector after using its length
    float3 H = normalize(V + L);
    float NdotL = max(dot(N, L), 0.0); // Scale light by NdotL
    
    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, albedo, metallic);
    
	// Reflectance equation
    float3 Lo = float3(0.0, 0.0, 0.0);
    
    //float attenuation = 1.0 / (distance);
    //float3 radiance = lightColors[index] * attenuation;

	// Cook-Torrance BRDF
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    float3 nominator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) + 0.001;
    float3 specular = nominator / denominator;

    float3 kS = F;
    float3 kD = (float3(1.0, 1.0, 1.0) - kS) * (1.0 - metallic);

    //TODO
    Lo += (kD * NdotL * albedo / PI + specular); // * radiance;

    float3 ambient = float3(0.3, 0.3, 0.3) * albedo * ao;

    float3 color = ambient + Lo;

	// HDR tonemapping
    color = color / (color + float3(1, 1, 1));
    float exponent = 1.0 / 2.2;
	// Gamma correct
    color = pow(color, float3(exponent, exponent, exponent));

    return float4(color, 1.0);
}

technique Full
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL FullVS();
        PixelShader = compile PS_SHADERMODEL FullPS();
    }
};


technique Default
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};