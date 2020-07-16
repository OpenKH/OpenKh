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

using OpenKh.Engine.Renders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenKh.Tools.Common.Rendering
{
    using d3d = SharpDX.Direct3D11;
    using dxgi = SharpDX.DXGI;

    public partial class SpriteDrawingDirect3D
    {
        private static readonly byte[] WhiteBitmap = Enumerable.Range(0, 2 * 2 * sizeof(int)).Select(x => byte.MaxValue).ToArray();

        private readonly CDevice _device;
        private readonly ISpriteTexture _defaultTexture;

        public SpriteDrawingDirect3D()
        {
            _device = new CDevice();
            _defaultTexture = CreateSpriteTexture(2, 2, WhiteBitmap);
        }

        public d3d.Device1 Device => _device.Device;
        public d3d.DeviceContext1 Context => _device.Context;

        private class CDevice : IDisposable
        {
            private static readonly d3d.InputElement[] InputElements = new d3d.InputElement[]
            {
                new d3d.InputElement("POSITION", 0, dxgi.Format.R32G32_Float, sizeof(float) * 0, 0, d3d.InputClassification.PerVertexData, 0),
                new d3d.InputElement("TEXTURE", 0, dxgi.Format.R32G32_Float, sizeof(float) * 2, 0, d3d.InputClassification.PerVertexData, 0 ),
                new d3d.InputElement("COLOR", 0, dxgi.Format.R32G32B32A32_Float, sizeof(float) * 4, 0, d3d.InputClassification.PerVertexData, 0 ),
            };

            private List<IDisposable> _disposables = new List<IDisposable>();
			
            private d3d.Device d3dDevice;
            private dxgi.Device dxgiDevice;
            private dxgi.Device1 dxgiDevice1;
            private dxgi.Adapter dxgiAdapter;
            private dxgi.Factory dxgiFactory;

            private ShaderSignature inputSignature;
            private d3d.VertexShader vertexShader;
            private d3d.PixelShader pixelShader;
            private d3d.InputLayout inputLayout;

            public d3d.Device1 Device { get; }
            public d3d.DeviceContext1 Context => Device.ImmediateContext1;

            internal CDevice()
            {
                var flags = d3d.DeviceCreationFlags.BgraSupport;

                d3dDevice = new d3d.Device(DriverType.Hardware, flags);
                Device = d3dDevice.QueryInterface<d3d.Device1>();
                dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
				dxgiDevice1 = dxgiDevice.QueryInterface<dxgi.Device1>();
                dxgiAdapter = dxgiDevice.Adapter.QueryInterface<dxgi.Adapter>();
                dxgiFactory = dxgiAdapter.GetParent<dxgi.Factory>();

                CreateResources();
            }

			private void CreateResources()
            {
                //CreateDepthStencilState();
                CreateRasterizer();
                CreateBlend();
                CreateSamplers();
                //CreateWindowSizeDependentResources(new Size(1024, 1024));
                InitializeShaders();
            }

			private void CreateWindowSizeDependentResources(Size size)
            {
                var desc = new dxgi.SwapChainDescription
                {
                    Usage = dxgi.Usage.RenderTargetOutput | dxgi.Usage.BackBuffer,
                    BufferCount = 2,
                    IsWindowed = true,
                    SwapEffect = dxgi.SwapEffect.Discard,
                    Flags = dxgi.SwapChainFlags.None
                };

                desc.ModeDescription.Width = size.Width;
                desc.ModeDescription.Height = size.Height;
                desc.ModeDescription.Format = dxgi.Format.R8G8B8A8_UNorm;
                desc.ModeDescription.Scaling = dxgi.DisplayModeScaling.Stretched;
                desc.ModeDescription.RefreshRate.Numerator = 0;
                desc.ModeDescription.RefreshRate.Denominator = 0;
                desc.ModeDescription.ScanlineOrdering = dxgi.DisplayModeScanlineOrder.Unspecified;
                desc.SampleDescription.Count = 1;
                desc.SampleDescription.Quality = 0;

               //_swapChain = new dxgi.SwapChain(dxgiFactory, d3dDevice, desc);
            }

			private void CreateDepthStencilState()
            {
                var desc = new d3d.DepthStencilStateDescription
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = d3d.DepthWriteMask.All,
                    DepthComparison = d3d.Comparison.GreaterEqual,
                    IsStencilEnabled = false,
                    StencilReadMask = 0,
                    StencilWriteMask = 0
                };
                desc.FrontFace.FailOperation = d3d.StencilOperation.Keep;
                desc.FrontFace.DepthFailOperation = d3d.StencilOperation.Keep;
                desc.FrontFace.PassOperation = d3d.StencilOperation.Keep;
                desc.FrontFace.Comparison = d3d.Comparison.Never;
                desc.BackFace.FailOperation = d3d.StencilOperation.Keep;
                desc.BackFace.DepthFailOperation = d3d.StencilOperation.Keep;
                desc.BackFace.PassOperation = d3d.StencilOperation.Keep;
                desc.BackFace.Comparison = d3d.Comparison.Never;

                var depthStencilState = new d3d.DepthStencilState(Device, desc);
                _disposables.Add(depthStencilState);
                Context.OutputMerger.DepthStencilState = depthStencilState;
            }

			private void CreateRasterizer()
            {
                var desc = new d3d.RasterizerStateDescription()
                {
					FillMode = d3d.FillMode.Solid,
					CullMode = d3d.CullMode.None,
					IsFrontCounterClockwise = false,
					IsDepthClipEnabled = true,
					IsScissorEnabled = false
                };

                var rasterizerState = new d3d.RasterizerState(Device, desc);
                _disposables.Add(rasterizerState);
                Context.Rasterizer.State = rasterizerState;
            }

			private void CreateBlend()
            {
                var desc = new d3d.BlendStateDescription()
                {
                    AlphaToCoverageEnable = false,
                    IndependentBlendEnable = false
                };
                SetRenderTargetlendProperties(ref desc.RenderTarget[0]);

                var blendState = new d3d.BlendState(Device, desc);
                _disposables.Add(blendState);
                Context.OutputMerger.BlendState = blendState;
            }

            private void CreateSamplers()
            {
                var desc = new d3d.SamplerStateDescription()
                {
                    Filter = d3d.Filter.MinMagMipPoint,
                    AddressU = d3d.TextureAddressMode.Clamp,
                    AddressV = d3d.TextureAddressMode.Clamp,
                    AddressW = d3d.TextureAddressMode.Clamp,
                    MipLodBias = 0.0f,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = d3d.Comparison.Never,
                    BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                    MinimumLod = -3.402823466e+38F,
                    MaximumLod = +3.402823466e+38F
                };

                var sampler = new d3d.SamplerState(Device, desc);
                Context.PixelShader.SetSampler(0, sampler);
                _disposables.Add(sampler);
            }

			private void SetRenderTargetlendProperties(ref d3d.RenderTargetBlendDescription desc)
            {
                desc.IsBlendEnabled = true;
                desc.SourceBlend = d3d.BlendOption.SourceAlpha;
                desc.DestinationBlend = d3d.BlendOption.InverseSourceAlpha;
                desc.BlendOperation = d3d.BlendOperation.Add;
                desc.SourceAlphaBlend = d3d.BlendOption.SourceAlpha;
                desc.DestinationAlphaBlend = d3d.BlendOption.DestinationAlpha;
                desc.AlphaBlendOperation = d3d.BlendOperation.Add;
                desc.RenderTargetWriteMask = d3d.ColorWriteMaskFlags.All;
            }


            private void InitializeShaders()
            {
                using (var vertexShaderByteCode = ShaderBytecode.Compile(VS, "main", "vs_4_0", ShaderFlags.Debug))
                {
                    vertexShader = new d3d.VertexShader(Device, vertexShaderByteCode);
                    inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                    Context.VertexShader.Set(vertexShader);
                    _disposables.Add(vertexShader);
                }
                using (var pixelShaderByteCode = ShaderBytecode.Compile(PS, "main", "ps_4_0", ShaderFlags.Debug))
                {
                    pixelShader = new d3d.PixelShader(Device, pixelShaderByteCode);
                    Context.PixelShader.Set(pixelShader);
                    _disposables.Add(pixelShader);
                }

                Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                inputLayout = new d3d.InputLayout(d3dDevice, inputSignature, InputElements);
                Context.InputAssembler.InputLayout = inputLayout;
            }

            private string GetBytecodeShader(string shaderSource)
            {
                using (var byteCode = ShaderBytecode.Compile(VS, "main", "vs_4_0", ShaderFlags.OptimizationLevel3))
                {
                    return string.Join(",", byteCode.Bytecode.Data.Select(x => $"{x.ToString("X02")}"));
                }
            }
            public void Dispose()
            {
                foreach (var disposable in _disposables)
                    disposable.Dispose();
                dxgiFactory.Dispose();
                dxgiAdapter.Dispose();
                dxgiDevice1.Dispose();
                dxgiDevice.Dispose();
                Device.Dispose();
                d3dDevice.Dispose();
            }
        }
    }
}
