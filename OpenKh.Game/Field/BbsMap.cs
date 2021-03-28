using Microsoft.Xna.Framework.Graphics;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenKh.Game.Field
{
    public class BbsMap : IMap, IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private List<PmpEntity> _pmpEntities = new List<PmpEntity>();
        private List<MeshGroup> _pmpModels = new List<MeshGroup>();

        public BbsMap(GraphicsDevice graphics, string filePath)
        {
            _graphics = graphics;

            var pmp = File.OpenRead(filePath).Using(Pmp.Read);
            var group = new List<MeshGroup>();

            int pmoIndex = 0;
            for (int i = 0; i < pmp.objectInfo.Count; i++)
            {
                Pmp.ObjectInfo currentInfo = pmp.objectInfo[i];

                if (currentInfo.PMO_Offset != 0)
                {
                    var pmpEntity = new PmpEntity(pmoIndex,
                        new Vector3(currentInfo.PositionX, currentInfo.PositionY, currentInfo.PositionZ),
                        new Vector3(currentInfo.RotationX, currentInfo.RotationY, currentInfo.RotationZ),
                        new Vector3(currentInfo.ScaleX, currentInfo.ScaleY, currentInfo.ScaleZ));
                    pmpEntity.DifferentMatrix = pmp.hasDifferentMatrix[pmoIndex];

                    var pParser = new PmoParser(pmp.PmoList[pmoIndex], 100.0f);
                    var textures = new List<Tim2KingdomTexture>();

                    var meshGroup = new MeshGroup();
                    meshGroup.MeshDescriptors = pParser.MeshDescriptors;
                    meshGroup.Textures = new IKingdomTexture[pmp.PmoList[pmoIndex].header.TextureCount];

                    for (int j = 0; j < pmp.PmoList[pmoIndex].header.TextureCount; j++)
                    {
                        textures.Add(new Tim2KingdomTexture(pmp.PmoList[pmoIndex].texturesData[j], graphics));
                        meshGroup.Textures[j] = textures[j];
                    }

                    _pmpEntities.Add(pmpEntity);
                    _pmpModels.Add(meshGroup);
                    pmoIndex++;
                }
            }
        }

        public void Dispose()
        {
            foreach (var model in _pmpModels)
            {
                foreach (var texture in model.Textures)
                    texture.Dispose();
            }
        }

        public void Render(Camera camera, KingdomShader shader, EffectPass pass, bool passRenderOpaque)
        {
            if (passRenderOpaque == true)
                // we know that all the meshes will not be opaque anyway
                return;

            var worldView = camera.World;
            var specialWorldView = worldView;
            specialWorldView.M14 = 0;
            specialWorldView.M24 = 0;
            specialWorldView.M34 = 0;
            specialWorldView.M41 = 0;
            specialWorldView.M42 = 0;
            specialWorldView.M43 = 0;
            specialWorldView.M44 = 1;

            var originalDepthStencilState = _graphics.DepthStencilState;
            var originalResterizerState = _graphics.RasterizerState;
            foreach (var entity in _pmpEntities)
            {
                if (entity.DifferentMatrix)
                {
                    shader.SetWorldView(ref specialWorldView);
                    _graphics.DepthStencilState = DepthStencilState.DepthRead;
                }
                else
                {
                    shader.SetWorldView(ref worldView);
                    _graphics.DepthStencilState = DepthStencilState.Default;
                }

                int AxisNumberChanged = 0;
                AxisNumberChanged += Convert.ToInt32(entity.Scaling.X < 0);
                AxisNumberChanged += Convert.ToInt32(entity.Scaling.Y < 0);
                AxisNumberChanged += Convert.ToInt32(entity.Scaling.Z < 0);

                if (AxisNumberChanged == 1 || AxisNumberChanged == 3)
                    _graphics.RasterizerState = RasterizerState.CullCounterClockwise;
                else
                    _graphics.RasterizerState = RasterizerState.CullClockwise;


                shader.SetModelView(entity.GetMatrix());
                shader.UseAlphaMask = true;
                pass.Apply();

                _graphics.RenderMeshNew(shader, pass, _pmpModels[entity.Index], passRenderOpaque);
            }

            _graphics.DepthStencilState = originalDepthStencilState;
            _graphics.RasterizerState = originalResterizerState;
        }
    }
}
