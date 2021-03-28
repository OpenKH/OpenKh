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
            ForEdit($"Type##{index}", () => point.Type, x => point.Type = x);
            ForEdit($"Flag##{index}", () => point.Flag, x => point.Flag = x);
            ForEdit($"Id##{index}", () => point.Id, x => point.Id = x);
            ForEdit($"Unk20##{index}", () => point.Unk20, x => point.Unk20 = x);
            ForEdit($"Unk24##{index}", () => point.Unk24, x => point.Unk24 = x);

            ImGui.Separator();
            ImGui.Text($"Map teleport##{index}");
            ForEdit($"Place##{index}", () => point.Teleport.Place, x => point.Teleport.Place = x);
            ForEdit($"Door##{index}", () => point.Teleport.Door, x => point.Teleport.Door = x);
            ForEdit($"World##{index}", () => point.Teleport.World, x => point.Teleport.World = x);
            ForEdit($"Unknown##{index}", () => point.Teleport.Unknown, x => point.Teleport.Unknown = x);
            ImGui.Separator();

            for (var i = 0; i < point.Entities.Count; i++)
                ForTreeNode($"Entity #{index}-{i}", () => Run(point.Entities[i], i));

            for (var i = 0; i < point.EventActivators.Count; i++)
                ForTreeNode($"Event activator #{index}-{i}", () => Run(point.EventActivators[i], i));

            for (var i = 0; i < point.WalkPath.Count; i++)
                ForTreeNode($"Walking path #{index}-{i}", () => Run(point.WalkPath[i], i));

            for (var i = 0; i < point.ReturnParameters.Count; i++)
                ForTreeNode($"Parameter #{index}-{i}", () => Run(point.ReturnParameters[i], i));

            for (var i = 0; i < point.Signals.Count; i++)
                ForTreeNode($"Signal #{index}-{i}", () => Run(point.Signals[i], i));
        }

        private static void Run(SpawnPoint.Entity entity, int index)
        {
            var objs = _ctrl.CurrentSpawnPoint.ObjEntryCtrl;
            if (ImGui.BeginCombo($"Object##{index}", objs.GetName(entity.ObjectId)))
            {
                var filter = ObjectFilter;
                if (ImGui.InputText($"Filter##{index}", ref filter, 16))
                    ObjectFilter = filter;

                foreach (var obj in objs.ObjectEntries.Where(x => filter.Length == 0 || x.ModelName.Contains(filter)))
                {
                    if (ImGui.Selectable(obj.ModelName, obj.ObjectId == entity.ObjectId))
                        entity.ObjectId = (int)obj.ObjectId;
                }

                ImGui.EndCombo();
            }

            ForEdit3($"Position##{index}", () =>
                new Vector3(entity.PositionX, entity.PositionY, entity.PositionZ),
                x =>
                {
                    entity.PositionX = x.X;
                    entity.PositionY = x.Y;
                    entity.PositionZ = x.Z;
                });

            ForEdit3($"Rotation##{index}", () =>
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

            ForEdit($"Spawn type##{index}", () => entity.SpawnType, x => entity.SpawnType = x);
            ForEdit($"Spawn arg##{index}", () => entity.SpawnArgument, x => entity.SpawnArgument = x);
            ForEdit($"Serial##{index}", () => entity.Serial, x => entity.Serial = x);
            ForEdit($"Argument 1##{index}", () => entity.Argument1, x => entity.Argument1 = x);
            ForEdit($"Argument 2##{index}", () => entity.Argument2, x => entity.Argument2 = x);
            ForEdit($"Reaction command##{index}", () => entity.ReactionCommand, x => entity.ReactionCommand = x);
            ForEdit($"Spawn delay##{index}", () => entity.SpawnDelay, x => entity.SpawnDelay = x);
            ForEdit($"Command##{index}", () => entity.Command, x => entity.Command = x);
            ForEdit($"Spawn range##{index}", () => entity.SpawnRange, x => entity.SpawnRange = x);
            ForEdit($"Level##{index}", () => entity.Level, x => entity.Level = x);
            ForEdit($"Medal##{index}", () => entity.Medal, x => entity.Medal = x);
        }

        private static void Run(SpawnPoint.EventActivator item, int index)
        {
            ForEdit($"Shape##{index}", () => item.Shape, x => item.Shape = x);
            ForEdit($"Option##{index}", () => item.Option, x => item.Option = x);
            ForEdit($"Flags##{index}", () => item.Flags, x => item.Flags = x);
            ForEdit($"Type##{index}", () => item.Type, x => item.Type = x);
            ForEdit($"BG group ON##{index}", () => item.OnBgGroup, x => item.OnBgGroup = x);
            ForEdit($"BG group OFF##{index}", () => item.OffBgGroup, x => item.OffBgGroup = x);
            ForEdit3($"Position##{index}", () =>
                new Vector3(item.PositionX, item.PositionY, item.PositionZ),
                x =>
                {
                    item.PositionX = x.X;
                    item.PositionY = x.Y;
                    item.PositionZ = x.Z;
                });
            ForEdit3($"Scale##{index}", () =>
                new Vector3(item.ScaleX, item.ScaleY, item.ScaleZ),
                x =>
                {
                    item.ScaleX = x.X;
                    item.ScaleY = x.Y;
                    item.ScaleZ = x.Z;
                }, 1f);

            ForEdit3($"Rotation##{index}", () =>
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
        }

        private static void Run(SpawnPoint.WalkPathDesc item, int index)
        {
            ForEdit($"Serial##{index}", () => item.Serial, x => item.Serial = x);
            ForEdit($"Flag##{index}", () => item.Flag, x => item.Flag = x);
            ForEdit($"Id##{index}", () => item.Id, x => item.Id = x);
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

        private static void Run(SpawnPoint.ReturnParameter item, int index)
        {
            ForEdit($"Unk00##{index}", () => item.Id, x => item.Id = x);
            ForEdit($"Unk01##{index}", () => item.Type, x => item.Type = x);
            ForEdit($"Unk02##{index}", () => item.Rate, x => item.Rate = x);
            ForEdit($"Unk03##{index}", () => item.EntryType, x => item.EntryType = x);
        }

        private static void Run(SpawnPoint.Signal item, int index)
        {
            ForEdit($"Signal ID##{index}", () => item.SignalId, x => item.SignalId = x);
            ForEdit($"Argument##{index}", () => item.Argument, x => item.Argument = x);
            ForEdit($"Action##{index}", () => item.Action, x => item.Action = x);
        }
    }
}
