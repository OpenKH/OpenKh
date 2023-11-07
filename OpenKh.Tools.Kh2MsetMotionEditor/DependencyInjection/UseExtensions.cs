using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec;
using OpenKh.Tools.Kh2MsetMotionEditor.Models.Presets;
using OpenKh.Tools.Kh2MsetMotionEditor.Usecases;
using OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows;
using OpenKh.Tools.Kh2MsetMotionEditor.Usecases.InsideTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenKh.Tools.Kh2MsetMotionEditor.DependencyInjection
{
    internal static class UseExtensions
    {
        public static void UseKh2MsetMotionEditor(
            this IServiceCollection self,
            int InitialWindowWidth,
            int InitialWindowHeight,
            string GamePath,
            string ConfigDir
        )
        {
            Directory.CreateDirectory(ConfigDir);

            IEnumerable<MdlxMsetPreset> mdlxMsetPresets = new MdlxMsetPreset[0];
            BoneDictElement boneDict = new BoneDictElement();

            self.AddSingleton<GetGamePathUsecase>(
                sp => () => GamePath
            );

            self.AddSingleton<App>();
            self.AddSingleton<RenderModelUsecase>();
            self.AddSingleton<LoadedModel>();
            self.AddSingleton<KingdomShader>();
            self.AddSingleton<ManageKingdomTextureUsecase>();
            self.AddSingleton<LoadModelUsecase>();
            self.AddSingleton<PrintActionResultUsecase>();
            self.AddSingleton<LoadMotionUsecase>();
            self.AddSingleton<LoadMotionDataUsecase>();
            self.AddSingleton<LayoutOnMultiColumnsUsecase>();
            self.AddSingleton<GenerateXsdUsecase>();
            self.AddSingleton<LoadXmlUsecase>();
            self.AddSingleton<FilterBoneViewUsecase>();
            self.AddSingleton<IMExExcelUsecase>();
            self.AddSingleton<ErrorMessages>();
            self.AddSingleton<AskOpenFileNowUsecase>();
            self.AddSingleton<SearchForKh2AssetFileUsecase>();
            self.AddSingleton<MakeHandyEditorUsecase>();
            self.AddSingleton<BigOneSelectorPopupUsecase>();
            self.AddSingleton<EditCollectionNoErrorUsecase>();
            self.AddSingleton<FormatExpressionNodesUsecase>();
            self.AddSingleton<FormatListItemUsecase>();
            self.AddSingleton<ITextureBinder, TextureBinderProxy>();
            self.AddSingleton<ComputeSpriteIconUvUsecase>();
            self.AddSingleton<PrintDebugInfo>();
            self.AddSingleton<ConvertVectorSpaceUsecase>();
            self.AddSingleton<FCurvesManagerUsecase>();
            self.AddSingleton<LogCrashStatusUsecase>();
            self.AddSingleton<GlobalInfo>();
            self.AddSingleton<OpenWindowUsecase>();
            self.AddSingleton<EditMotionDataUsecase>();
            self.AddSingleton<NormalMessages>();

            // next




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

            // windows
            self.AddSingleton<IWindowRunnableProvider, BonesManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, IKHelperManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, InitialPoseManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, ErrorMessagesWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, ExpressionManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, ConstraintManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, PrintDebugInfoManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, JointManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, RootPositionWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, FCurvesForwardManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, FCurvesInverseManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, FCurveKeyManagerWindowUsecase>();
            self.AddSingleton<IWindowRunnableProvider, FCurvesFkIkGridManagerWindow>();
            self.AddSingleton<IWindowRunnableProvider, NormalMessagesWindowUsecase>();




            // tools
            self.AddSingleton<IToolRunnableProvider, MdlxMsetLoaderToolUsecase>();
            self.AddSingleton<IToolRunnableProvider, MotionPlayerToolUsecase>();

            self.AddSingleton(Settings.Default);

            self.AddSingleton<CreateWhiteTextureUsecase>(
                sp =>
                    () =>
                    {
                        var whiteTexture = new Texture2D(sp.GetRequiredService<GraphicsDevice>(), 2, 2);
                        whiteTexture.SetData(Enumerable.Range(0, 2 * 2 * sizeof(int)).Select(_ => (byte)0xff).ToArray());
                        return whiteTexture;
                    }
            );

            self.AddSingleton<CreateSpriteIconsTextureUsecase>(
                sp =>
                    () =>
                    {
                        return sp.GetRequiredService<ContentManager>().Load<Texture2D>("SpriteIcons");
                    }
            );

            self.AddSingleton<GetMdlxMsetPresets>(
                sp =>
                    () =>
                        mdlxMsetPresets
            );

            self.AddSingleton<GetBoneDictElementUsecase>(
                sp =>
                    () =>
                        boneDict
            );

            self.AddSingleton<ReloadKh2PresetsUsecase>(
                sp =>
                    () =>
                    {
                        try
                        {
                            mdlxMsetPresets = sp.GetRequiredService<LoadXmlUsecase>()
                                .LoadXmlOrCreateNewOne<MdlxMsetPresets>(
                                    Path.Combine(ConfigDir, "Presets.xml"),
                                    Path.Combine(ConfigDir, "Presets.xsd")
                                )
                                .GetPresets();
                        }
                        catch (Exception ex)
                        {
                            sp.GetRequiredService<ErrorMessages>().Add(
                                new Exception($"Presets.xml file contains error", ex)
                            );
                        }

                        try
                        {
                            boneDict = sp.GetRequiredService<LoadXmlUsecase>()
                                .LoadXmlOrCreateNewOne<BoneDictElement>(
                                    Path.Combine(ConfigDir, "BoneDict.xml"),
                                    Path.Combine(ConfigDir, "BoneDict.xsd")
                                );
                        }
                        catch (Exception ex)
                        {
                            sp.GetRequiredService<ErrorMessages>().Add(
                                new Exception($"BoneDict.xml file contains error", ex)
                            );
                        }

                        sp.GetRequiredService<LoadedModel>().Kh2PresetsAge.Bump();
                    }
            );

            self.AddSingleton(
                sp =>
                {
                    var lazyApp = new Lazy<App>(() => sp.GetRequiredService<App>());
                    var bootstrap = new MonoGameImGuiBootstrap(
                        InitialWindowWidth,
                        InitialWindowHeight,
                        bootstrap => { }
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
