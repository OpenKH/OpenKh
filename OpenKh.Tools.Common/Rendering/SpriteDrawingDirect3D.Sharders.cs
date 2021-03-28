// MIT License
// 
// Copyright(c) 2017 Luciano (Xeeynamo) Ciccariello
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Part of this software belongs to XeEngine toolset and United Lines Studio
// and it is currently used to create commercial games by Luciano Ciccariello.
// Please do not redistribuite this code under your own name, stole it or use
// it artfully, but instead support it and its author. Thank you.

// Took from Xe.Drawing.Direct3D and transported to OpenKH under the same developer


namespace OpenKh.Tools.Common.Rendering
{
    public partial class SpriteDrawingDirect3D
    {
        private const string VS =
@"cbuffer MatrixBuffer : register(b0)
{
	matrix Matrix;
	matrix dummy;
};

struct VertexIn
{
	float4 pos		: POSITION;
	float2 tex		: TEXTURE;
	float4 color	: COLOR;
};
struct VertexOut
{
	float4 pos		: SV_POSITION;
	float2 tex		: TEXTURE;
	float4 color	: COLOR;
};

VertexOut main(const VertexIn vIn)
{
	VertexOut vOut;
	//vOut.pos = mul(Matrix, vIn.pos);
	vOut.pos = vIn.pos;
	vOut.tex = vIn.tex;
	vOut.color = vIn.color;
	return vOut;
}";
        private const string PS =
@"struct PixelIn
{
	float4 pos		: SV_POSITION;
	float2 tex		: TEXTURE;
	float4 color	: COLOR;
};

Texture2D tImage0;
SamplerState sampleImage0;
Texture2D tClut0;
SamplerState sampleClut0;

float4 main(PixelIn pIn) : SV_TARGET
{
    float4 texColor = tImage0.Sample(sampleImage0, pIn.tex.xy);
	float4 blendColor = pIn.color;
    return texColor * blendColor;
	/*float4 color = pIn.color;
	if (pIn.tex.z < 0.50)
	{
		// Use palette W texture
		float colorIndex = tImage0.Sample(sampleImage0, pIn.tex.xy).r;
		color *= tClut0.Sample(sampleClut0, float2(colorIndex, pIn.tex.z * 2.0));
	}
	else if (pIn.tex.z < 1.0)
	{
		// Only texture
		color *= tImage0.Sample(sampleImage0, pIn.tex.xy);
	}
	else
	{
		// Do not use texture
	}
	return color;*/
}";
    }
}
