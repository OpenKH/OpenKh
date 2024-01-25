using OpenKh.ColladaUtils;
using OpenKh.Tests.Tools.Helpers;
using OpenKh.Tools.KhModels.Usecases;
using OpenKh.Tools.KhModels.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace OpenKh.Tests.Tools.Tdd
{
    public class SaveDaeModelUsecaseTest
    {
        private readonly SaveDaeModelUsecase _saveDaeModelUsecase = new();
        private readonly ConvertToDaeModelUsecase _convertToDaeModelUsecase = new();

        [Theory]
        [InlineData(@"%KH2FM_EXTRACTION_DIR%\obj\H_EX500.mdlx", "H_EX500")]
        [InlineData(@"%KH2FM_EXTRACTION_DIR%\obj\P_EX100.mdlx", "P_EX100")]
        [InlineData(@"%KH2FM_EXTRACTION_DIR%\map\jp\tt00.map", "tt00")]
        [InlineData(@"%KH2FM_EXTRACTION_DIR%\map\jp\tt01.map", "tt01")]
        public void ExportTest(string mdlxInput, string daeOutputPrefix)
        {
            mdlxInput = Environment.ExpandEnvironmentVariables(mdlxInput);
            if (!File.Exists(mdlxInput))
            {
                return;
            }

            var saveToDir = Path.Combine(Environment.CurrentDirectory, daeOutputPrefix);
            Directory.CreateDirectory(saveToDir);

            RunOnSta.Run(
                () =>
                {
                    var vp = new HelixToolkit.Wpf.HelixViewport3D();
                    var window = new Window
                    {
                        Content = vp,
                        Left = 0,
                        Top = 0,
                        Width = 300,
                        Height = 300,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                    };
                    window.Show();
                    var vm = new MainWindowVM(vp);
                    vm.LoadFilepath(mdlxInput);

                    foreach (var sourceModel in vm.VpService.Models)
                    {
                        using var daeStream = File.Create(Path.Combine(saveToDir, $"{daeOutputPrefix}_{sourceModel.Name}.dae"));

                        _saveDaeModelUsecase.Save(
                            model: _convertToDaeModelUsecase.Convert(
                                sourceModel: sourceModel,
                                savePngToDir: saveToDir,
                                filePrefix: $"{daeOutputPrefix}_{sourceModel.Name}"
                            ),
                            stream: daeStream
                        );
                    }

                    window.Close();
                }
            );
        }
    }
}
