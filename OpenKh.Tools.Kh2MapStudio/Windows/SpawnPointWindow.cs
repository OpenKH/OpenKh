using ImGuiNET;
using OpenKh.Kh2.Ard;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using OpenKh.Tools.Kh2MapStudio.Models;
using System;
using System.Linq;
using System.Numerics;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class SpawnPointWindow
    {
        private static string ObjectFilter = "";
        private static ISpawnPointController _ctrl;

        public static bool Run(ISpawnPointController ctrl) => ForHeader("Spawn point editor", () =>
        {
            _ctrl = ctrl;
            if (ImGui.BeginCombo("Spawn point", ctrl.SelectSpawnPoint))
            {
                foreach (var spawnPoint in ctrl.SpawnPoints)
                {
                    if (ImGui.Selectable(spawnPoint.Name, spawnPoint.Name == ctrl.SelectSpawnPoint))
                    {
                        ctrl.SelectSpawnPoint = spawnPoint.Name;
                    }
                }

                ImGui.EndCombo();
            }

            if (ctrl.CurrentSpawnPoint != null)
                Run(ctrl.CurrentSpawnPoint);
        });

        private static void Run(SpawnPointModel model)
        {
            for (int i = 0; i < model.SpawnPoints.Count; i++)
            {
                var spawnGroup = model.SpawnPoints[i];
                if (ImGui.CollapsingHeader($"Spawn group #{i}"))
                {
                    Run(spawnGroup, i);
                }
            }
        }

        private static void Run(SpawnPoint point, int index)
        {
            ForEdit($"Unk00#{index}", () => point.Unk00, x => point.Unk00 = x);
            ForEdit($"Unk02#{index}", () => point.Unk02, x => point.Unk02 = x);
            ForEdit($"Unk20#{index}", () => point.Unk20, x => point.Unk20 = x);
            ForEdit($"Unk24#{index}", () => point.Unk24, x => point.Unk24 = x);

            ImGui.Separator();
            ImGui.Text($"Map teleport#{index}");
            ForEdit($"Place#{index}", () => point.Teleport.Place, x => point.Teleport.Place = x);
            ForEdit($"Door#{index}", () => point.Teleport.Door, x => point.Teleport.Door = x);
            ForEdit($"World#{index}", () => point.Teleport.World, x => point.Teleport.World = x);
            ForEdit($"Unknown#{index}", () => point.Teleport.Unknown, x => point.Teleport.Unknown = x);
            ImGui.Separator();

            for (var i = 0; i < point.Entities.Count; i++)
                ForTreeNode($"Entity #{index}-{i}", () => Run(point.Entities[i], i));

            for (var i = 0; i < point.EventActivators.Count; i++)
                ForTreeNode($"Event activator #{index}-{i}", () => Run(point.EventActivators[i], i));

            for (var i = 0; i < point.WalkPath.Count; i++)
                ForTreeNode($"Walking path #{index}-{i}", () => Run(point.WalkPath[i], i));

            for (var i = 0; i < point.Unknown0aTable.Count; i++)
                ForTreeNode($"Unknown 0A #{index}-{i}", () => Run(point.Unknown0aTable[i], i));

            for (var i = 0; i < point.Unknown0cTable.Count; i++)
                ForTreeNode($"Unknown 0C #{index}-{i}", () => Run(point.Unknown0cTable[i], i));
        }

        private static void Run(SpawnPoint.Entity entity, int index)
        {
            var objs = _ctrl.CurrentSpawnPoint.ObjEntryCtrl;
            if (ImGui.BeginCombo($"Object#{index}", objs.GetName(entity.ObjectId)))
            {
                var filter = ObjectFilter;
                if (ImGui.InputText($"Filter#{index}", ref filter, 16))
                    ObjectFilter = filter;

                foreach (var obj in objs.ObjectEntries.Where(x => filter.Length == 0 || x.ModelName.Contains(filter)))
                {
                    if (ImGui.Selectable(obj.ModelName, obj.ObjectId == entity.ObjectId))
                        entity.ObjectId = obj.ObjectId;
                }

                ImGui.EndCombo();
            }

            ForEdit3($"Position#{index}", () =>
                new Vector3(entity.PositionX, entity.PositionY, entity.PositionZ),
                x =>
                {
                    entity.PositionX = x.X;
                    entity.PositionY = x.Y;
                    entity.PositionZ = x.Z;
                });

            ForEdit3($"Rotation#{index}", () =>
                new Vector3(
                    (float)(entity.RotationX * 180f / Math.PI),
                    (float)(entity.RotationY * 180f / Math.PI),
                    (float)(entity.RotationZ * 180f / Math.PI)),
                x =>
                {
                    entity.RotationX = (float)(x.X / 180f * Math.PI);
                    entity.RotationY = (float)(x.Y / 180f * Math.PI);
                    entity.RotationZ = (float)(x.Z / 180f * Math.PI);
                });

            ForEdit($"Use entrance#{index}", () => entity.UseEntrance, x => entity.UseEntrance = x);
            ForEdit($"Entrance ID#{index}", () => entity.Entrance, x => entity.Entrance = x);
            ForEdit($"Unk1e#{index}", () => entity.Unk1e, x => entity.Unk1e = x);
            ForEdit($"Unk20#{index}", () => entity.Unk20, x => entity.Unk20 = x);
            ForEdit($"Ai Parameter#{index}", () => entity.AiParameter, x => entity.AiParameter = x);
            ForEdit($"TalkMessage#{index}", () => entity.TalkMessage, x => entity.TalkMessage = x);
            ForEdit($"ReactionCommand#{index}", () => entity.ReactionCommand, x => entity.ReactionCommand = x);
            ForEdit($"Unk30#{index}", () => entity.Unk30, x => entity.Unk30 = x);
        }

        private static void Run(SpawnPoint.EventActivator item, int index)
        {
            ForEdit($"Unk00#{index}", () => item.Unk00, x => item.Unk00 = x);
            ForEdit3($"Position#{index}", () =>
                new Vector3(item.PositionX, item.PositionY, item.PositionZ),
                x =>
                {
                    item.PositionX = x.X;
                    item.PositionY = x.Y;
                    item.PositionZ = x.Z;
                });
            ForEdit3($"Scale#{index}", () =>
                new Vector3(item.ScaleX, item.ScaleY, item.ScaleZ),
                x =>
                {
                    item.ScaleX = x.X;
                    item.ScaleY = x.Y;
                    item.ScaleZ = x.Z;
                }, 1f);

            ForEdit3($"Rotation#{index}", () =>
                new Vector3(
                    (float)(item.RotationX * 180f / Math.PI),
                    (float)(item.RotationY * 180f / Math.PI),
                    (float)(item.RotationZ * 180f / Math.PI)),
                x =>
                {
                    item.RotationX = (float)(x.X / 180f * Math.PI);
                    item.RotationY = (float)(x.Y / 180f * Math.PI);
                    item.RotationZ = (float)(x.Z / 180f * Math.PI);
                });

            ForEdit($"Unk28#{index}", () => item.Unk28, x => item.Unk28 = x);
            ForEdit($"Unk2c#{index}", () => item.Unk2c, x => item.Unk2c = x);
        }

        private static void Run(SpawnPoint.WalkPathDesc item, int index)
        {
            ForEdit($"Unk00#{index}", () => item.Unk00, x => item.Unk00 = x);
            ForEdit($"Unk04#{index}", () => item.Unk04, x => item.Unk04 = x);
            ForEdit($"Unk06#{index}", () => item.Unk06, x => item.Unk06 = x);
            for (var i = 0; i < item.Positions.Count; i++)
            {
                var pos = item.Positions[i];
                ForEdit3($"Walk #{index}-{i}", () =>
                    new Vector3(pos.X, pos.Y, pos.Z),
                    x =>
                    {
                        pos.X = x.X;
                        pos.Y = x.Y;
                        pos.Z = x.Z;
                    });
            }
        }

        private static void Run(SpawnPoint.Unknown0a item, int index)
        {
            ForEdit($"Unk00#{index}", () => item.Unk00, x => item.Unk00 = x);
            ForEdit($"Unk01#{index}", () => item.Unk01, x => item.Unk01 = x);
            ForEdit($"Unk02#{index}", () => item.Unk02, x => item.Unk02 = x);
            ForEdit($"Unk03#{index}", () => item.Unk03, x => item.Unk03 = x);
            ForEdit($"Unk04#{index}", () => item.Unk04, x => item.Unk04 = x);
            ForEdit($"Unk05#{index}", () => item.Unk05, x => item.Unk05 = x);
            ForEdit($"Unk06#{index}", () => item.Unk06, x => item.Unk06 = x);
            ForEdit($"Unk08#{index}", () => item.Unk08, x => item.Unk08 = x);
            ForEdit($"Unk0c#{index}", () => item.Unk0c, x => item.Unk0c = x);
        }

        private static void Run(SpawnPoint.Unknown0c item, int index)
        {
            ForEdit($"Unk00#{index}", () => item.Unk00, x => item.Unk00 = x);
            ForEdit($"Unk04#{index}", () => item.Unk04, x => item.Unk04 = x);
        }
    }
}
