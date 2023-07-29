using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class LoadModelUsecase
    {
        private readonly LoadedModel _loadedModel;

        public LoadModelUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public void OpenModel(string mdlxFile)
        {
            Close();

            _loadedModel.MdlxEntries = File.OpenRead(mdlxFile).Using(Bar.Read);
            _loadedModel.MdlxBytes = File.ReadAllBytes(mdlxFile);

            var barEntries = _loadedModel.MdlxEntries!;

            var model = Mdlx.Read(
                barEntries.First(it => it.Type == Bar.EntryType.Model).Stream
            );

            var textures = ModelTexture.Read(
                barEntries.First(it => it.Type == Bar.EntryType.ModelTexture).Stream
            )
                .Images;

            var modelMotionAttachable = MeshLoader.FromKH2(model);
            var tpose = modelMotionAttachable.MeshDescriptors;

            _loadedModel.MdlxRenderableList.Add(
                new MdlxRenderSource(
                    model,
                    textures,
                    matrices =>
                    {
                        if (matrices != null)
                        {
                            modelMotionAttachable.ApplyMotion(matrices);
                            return modelMotionAttachable.MeshDescriptors;
                        }
                        else
                        {
                            return tpose;
                        }
                    }
                )
            );
        }

        public void Close()
        {
            _loadedModel.MdlxRenderableList.Clear();
        }
    }
}
