using ModelingToolkit.HelixModule;
using ModelingToolkit.Objects;
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
    public class ExportToBasicDaeUsecaseTest
    {
        private readonly SaveBasicDaeUsecase _saveBasicDaeUsecase = new();
        private readonly ConvertToBasicDaeUsecase _convertToBasicDaeUsecase = new();

        [Theory]
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
                        using var daeStream = File.Create($"{daeOutputPrefix}_{sourceModel.Name}.dae");

                        _saveBasicDaeUsecase.Save(
                            model: _convertToBasicDaeUsecase.Convert(
                                sourceModel: sourceModel,
                                filePrefix: $"{daeOutputPrefix}_{sourceModel.Name}"
                            ),
                            stream: daeStream
                        );
                    }
                }
            );
        }
    }
}
