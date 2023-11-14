#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix ModelView;
matrix WorldView;
matrix ProjectionView;
float2 TextureRegionU;
float2 TextureRegionV;
int TextureWrapModeU;
int TextureWrapModeV;
Texture2D Texture0;
bool UseAlphaMask;

sampler2D TextureSampler = sampler_state
{
	Texture = <Texture0>;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureUv : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureUv : TEXCOORD0;
};

float RegionClamp(float value, float valueMin, float valueMax)
{
	return min(max(value, valueMin), valueMax);
}

float RegionRepeat(float value, float min, float max)
{
	float mod = (value - min) % (max - min);
	if (mod < 0)
		mod += max - min;
	return min + mod;
}

float ApplyTextureWrap(float value, int mode, float min, float max) {
	if (mode == 1) {
		return RegionClamp(value, min, max);
	}
	else if (mode == 2) {
		return RegionRepeat(value, min, max);
	}

	return value;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(mul(input.Position, ModelView), WorldView), ProjectionView);
	output.TextureUv = input.TextureUv;
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 uv = float2(
		ApplyTextureWrap(input.TextureUv.x, TextureWrapModeU, TextureRegionU.x, TextureRegionU.y),
		ApplyTextureWrap(input.TextureUv.y, TextureWrapModeV, TextureRegionV.x, TextureRegionV.y)
		);
	float4 color = tex2D(TextureSampler, uv) * input.Color;
	if (UseAlphaMask && color.a <= 0.125) {
		clip(-1);
	}
	return color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};