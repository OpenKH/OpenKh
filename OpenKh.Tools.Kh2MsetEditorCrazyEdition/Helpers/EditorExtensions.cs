using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers
{
    internal static class EditorExtensions
    {
        public static void UseKh2MsetEditorCrazyEdition(
            this IServiceCollection self,
            int InitialWindowWidth,
            int InitialWindowHeight,
            string GamePath,
            string ConfigDir
        )
        {
            self.AddSingleton<GetGamePathUsecase>(
                sp => () => GamePath
            );

            self.AddSingleton<App>();
            self.AddSingleton<MapRenderer>();
            self.AddSingleton<RenderModelUsecase>();
            self.AddSingleton<LoadedModel>();
            self.AddSingleton<KingdomShader>();
            self.AddSingleton<ManageKingdomTextureUsecase>();
            self.AddSingleton<LoadModelUsecase>();
            self.AddSingleton<PrintActionResultUsecase>();
            self.AddSingleton<MotionLoaderToolUsecase>();
            self.AddSingleton<LoadMotionUsecase>();
            self.AddSingleton<LoadMotionDataUsecase>();
            self.AddSingleton<LayoutOnMultiColumnsUsecase>();
            




            self.AddSingleton(
                sp => new Camera()
                {
                    CameraPosition = new Vector3(0, 100, 200),
                    CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
                }
            );



            self.AddSingleton(
                sp => sp.GetRequiredService<MonoGameImGuiBootstrap>().Content
            );

            self.AddSingleton(
                sp => sp.GetRequiredService<MonoGameImGuiBootstrap>().GraphicsDevice
            );

            self.AddSingleton(
                sp => sp.GetRequiredService<MonoGameImGuiBootstrap>().GraphicsDeviceManager
            );

            self.AddSingleton(
                sp => sp.GetRequiredService<MonoGameImGuiBootstrap>().Content
            );

            self.AddSingleton<IToolRunnableProvider, MdlxMsetLoaderToolUsecase>();
            self.AddSingleton<IToolRunnableProvider, MotionPlayerToolUsecase>();
            
            self.AddSingleton<GetWhiteTextureUsecase>(
                sp =>
                    () =>
                    {
                        var whiteTexture = new Texture2D(sp.GetRequiredService<GraphicsDevice>(), 2, 2);
                        whiteTexture.SetData(Enumerable.Range(0, 2 * 2 * sizeof(int)).Select(_ => (byte)0xff).ToArray());
                        return whiteTexture;
                    }
            );

            self.AddSingleton(
                sp =>
                {
                    var xmlFile = Path.Combine(ConfigDir, "Presets.xml");
                    if (File.Exists(xmlFile))
                    {
                        using var stream = File.OpenRead(xmlFile);
                        return (MdlxMsetPresets)new XmlSerializer(typeof(MdlxMsetPresets))
                            .Deserialize(stream)!;
                    }
                    else
                    {
                        return new MdlxMsetPresets { };
                    }
                }
            );

            self.AddSingleton(
                sp =>
                {
                    var lazyApp = new Lazy<App>(() => sp.GetRequiredService<App>());
                    var bootstrap = new MonoGameImGuiBootstrap(
                        InitialWindowWidth,
                        InitialWindowHeight,
                        bootstrap =>
                        {
                            var app = lazyApp.Value;
                        }
                    );
                    bootstrap.MainLoop = _ =>
                    {
                        if (lazyApp.Value.MainLoop())
                        {
                            bootstrap.Exit();
                        }
                    };

                    return bootstrap;
                }
            );
        }
    }
}
