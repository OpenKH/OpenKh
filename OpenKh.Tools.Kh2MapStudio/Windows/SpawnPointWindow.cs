using ImGuiNET;
using OpenKh.Common; //New
using Xe.Tools.Wpf.Dialogs; //New
using OpenKh.Kh2.Ard;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using OpenKh.Tools.Kh2MapStudio.Models;
using System.IO; //New
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenKh.Kh2.Ard.SpawnPoint; //Newly added, for WalkPath Addition.
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class SpawnPointWindow
    {
        private static string ObjectFilter = "";
        private static ISpawnPointController _ctrl;
        private static string _newSpawnPointName = "N_00"; // Default name for the new spawn point

        public static bool Run(ISpawnPointController ctrl) => ForHeader("Spawn point editor", () =>
        {
            _ctrl = ctrl;

            // Check list of spawn points is null or empty
            if (ctrl.SpawnPoints != null && ctrl.SpawnPoints.Any())
            {
                ImGui.PushItemWidth(200); // Adjust width as needed
                if (ImGui.BeginCombo(" ", ctrl.SelectSpawnPoint))
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
                // Add a button to save the file on the same line as the dropdown.
                ImGui.SameLine();
                if (ImGui.Button("Save Spawnpoint as YML"))
                {
                    SaveSpawnPointAsYaml(ctrl);
                }
                ImGui.PushItemWidth(50);
                ImGui.InputText("", ref _newSpawnPointName, 4); // String of 4 characters long.
                ImGui.PopItemWidth();
                ImGui.SameLine(); // Place the button on the same line as the text input
                // Add a button to add a new spawn point entry

                //Rename/Add/Remove SpawnPoint butons.
                if (ImGui.Button($"Rename '{ctrl.SelectSpawnPoint}' to '{_newSpawnPointName}'"))
                {
                    RenameSpawnPoint(ctrl);
                }
                ImGui.SameLine();
                if (ImGui.Button($"Add new Spawnpoint '{_newSpawnPointName}'"))
                {
                    AddNewSpawnPointEntry(ctrl);
                }
                ImGui.SameLine();
                if (ImGui.Button($"Remove '{ctrl.SelectSpawnPoint}'"))
                {
                    RemoveSelectedSpawnPoint(ctrl);
                }
                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text("No spawn points available.");
            }
            if (ctrl.CurrentSpawnPoint != null)
                Run(ctrl.CurrentSpawnPoint);
        });
        private static void RenameSpawnPoint(ISpawnPointController ctrl)
        {
            var selectedSpawnPoint = ctrl.SpawnPoints.FirstOrDefault(sp => sp.Name == ctrl.SelectSpawnPoint);
            if (selectedSpawnPoint != null)
            {
                // Check if the new name already exists
                if (ctrl.SpawnPoints.Any(sp => sp.Name == _newSpawnPointName))
                {
                    //Insert warning here. Standard error msg seems to pop up and disappear immediately, omitted for now.
                    return;
                }

                // Update the name of the selected spawn point
                var oldName = selectedSpawnPoint.Name;
                selectedSpawnPoint.Name = _newSpawnPointName;
                ctrl.SelectSpawnPoint = _newSpawnPointName;

                // Rename the corresponding Bar.Entry as well
                var barEntries = (ctrl as MapRenderer)?.ArdBarEntries;
                var barEntry = barEntries?.FirstOrDefault(e => e.Name == oldName);
                if (barEntry != null)
                {
                    barEntry.Name = _newSpawnPointName;
                }
            }
        }

        //Add a new spawn point entry
        private static void AddNewSpawnPointEntry(ISpawnPointController ctrl)
        {
            // Use the specified name for the new spawn point
            var newSpawnPointName = string.IsNullOrEmpty(_newSpawnPointName) ? "N_00" : _newSpawnPointName;

            // Check if the name already exists in the ARD
            var barEntries = (ctrl as MapRenderer)?.ArdBarEntries;
            if (barEntries != null && barEntries.Any(e => e.Name == newSpawnPointName))
            {
                Console.WriteLine($"Spawn point with name {newSpawnPointName} already exists.");
                return;
            }

            // Create a new Bar.Entry for AreaDataSpawn
            var newEntry = new Bar.Entry
            {
                Name = newSpawnPointName,
                Type = Bar.EntryType.AreaDataSpawn,
                Stream = new MemoryStream()
            };

            // Add to the Bar
            if (barEntries != null)
            {
                barEntries.Add(newEntry);
            }

            // Create and add a new spawn point model
            var objEntryCtrl = (ctrl as MapRenderer)?.ObjEntryController; // Ensure the correct type cast
            if (objEntryCtrl != null)
            {
                var newSpawnPoint = new SpawnPointModel(objEntryCtrl, newEntry.Name, new List<SpawnPoint>());
                ctrl.SpawnPoints.Add(newSpawnPoint);
                ctrl.SelectSpawnPoint = newSpawnPoint.Name;
            }
        }

        //Remove the currently selected spawn point entry
        private static void RemoveSelectedSpawnPoint(ISpawnPointController ctrl)
        {
            if (ctrl.SelectSpawnPoint == null)
            {
                Console.WriteLine("No spawn point selected to remove.");
                return;
            }

            var barEntries = (ctrl as MapRenderer)?.ArdBarEntries;
            if (barEntries == null)
            {
                Console.WriteLine("Bar entries not found.");
                return;
            }

            // Find the entry with the selected name and remove it
            var entryToRemove = barEntries.FirstOrDefault(e => e.Name == ctrl.SelectSpawnPoint);
            if (entryToRemove != null)
            {
                barEntries.Remove(entryToRemove);
                ctrl.SpawnPoints.RemoveAll(sp => sp.Name == ctrl.SelectSpawnPoint);
                ctrl.SelectSpawnPoint = ctrl.SpawnPoints.FirstOrDefault()?.Name;
                Console.WriteLine($"Spawn point {entryToRemove.Name} removed.");
            }
            else
            {
                Console.WriteLine($"Spawn point with name {ctrl.SelectSpawnPoint} not found in bar entries.");
            }
        }



        //NEW: Easily export spawnpoint as a YML. QoL change to make patching via OpenKH easier, so you don't need to extract the spawnpoint, use OpenKh.Command.Spawnpoints, decompile, etc.
        private static readonly List<FileDialogFilter> YamlFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("YAML file", "yml")
            .AddAllFiles();

        private static void SaveSpawnPointAsYaml(ISpawnPointController ctrl)
        {
            // Serialize the currently loaded SpawnPoint to YAML
            var spawnPoint = ctrl.CurrentSpawnPoint.SpawnPoints;
            if (spawnPoint != null)
            {
                var defaultName = $"{ctrl.CurrentSpawnPoint.Name}.yml"; // Set the default name to the current spawn point's name
                Xe.Tools.Wpf.Dialogs.FileDialog.OnSave(savePath =>
                {
                    try
                    {
                        // Serialize and save the spawn point data to the selected file
                        File.WriteAllText(savePath, Helpers.YamlSerialize(spawnPoint));
                        Console.WriteLine($"Spawn point saved to {savePath}");
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions if needed
                        Console.WriteLine($"Error saving spawn point as YAML: {ex.Message}");
                    }
                }, YamlFilter, defaultName);
            }
        }

        private static void Run(SpawnPointModel model)
        {
            if (ImGui.SmallButton("Add a new Spawn Group"))
            {
                var newSpawnPoint = new Kh2.Ard.SpawnPoint
                {
                    Entities = new List<Kh2.Ard.SpawnPoint.Entity> { },
                    EventActivators = new List<Kh2.Ard.SpawnPoint.EventActivator> { },
                    WalkPath = new List<Kh2.Ard.SpawnPoint.WalkPathDesc> { },
                    ReturnParameters = new List<Kh2.Ard.SpawnPoint.ReturnParameter> { },
                    Signals = new List<Kh2.Ard.SpawnPoint.Signal> { },
                    Teleport = new Kh2.Ard.SpawnPoint.TeleportDesc { }
                };
                model.SpawnPoints.Add(newSpawnPoint);
            }

            ImGui.SameLine();
            //The above code copies the last spawngroup.
            if (model.SpawnPoints.Count > 0)
                if (ImGui.SmallButton("Remove last Spawn Group"))
                    model.SpawnPoints.RemoveAt(model.SpawnPoints.Count - 1);
            ImGui.Separator();
            //Tree View of Spawn Groups
            if (ImGui.CollapsingHeader("Spawn Groups"))
            {
                ImGui.Indent(20.0f);
                for (int i = 0; i < model.SpawnPoints.Count; i++)
                {
                    var spawnGroup = model.SpawnPoints[i];
                    if (ImGui.CollapsingHeader($"Spawn group #{i}"))
                    {
                        Run(spawnGroup, i);
                    }
                }
                ImGui.Unindent(20.0f);
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


            //Entities
            if (ImGui.SmallButton("Add a new Entity"))
                point.Entities.Add(new SpawnPoint.Entity {ObjectId = 1 }); //Add ObjectId of 1.
            ImGui.SameLine();
            if (point.Entities.Count > 0) //Check first to see if there are entities. 
                if (ImGui.SmallButton("Remove last Entity")) //Placed BEFORE, so that button doesnt repeat 3x.
                    point.Entities.RemoveAt(point.Entities.Count - 1);
            ImGui.Separator();
            for (var i = 0; i < point.Entities.Count; i++) //Entities: Tree View
                ForTreeNode($"Entity #{index}-{i}", () => Run(point.Entities[i], i));
            
            //EventActivators, then Add/Remove Buttons.
            if (ImGui.SmallButton("Add a new Event Activator"))
                point.EventActivators.Add(new SpawnPoint.EventActivator());
            ImGui.SameLine();
            if (point.EventActivators.Count > 0)
                if (ImGui.SmallButton("Remove last Activator"))
                    point.EventActivators.RemoveAt(point.EventActivators.Count - 1);
            ImGui.Separator();
            for (var i = 0; i < point.EventActivators.Count; i++) //Event Activators: Tree View
                ForTreeNode($"Event activator #{index}-{i}", () => Run(point.EventActivators[i], i));


            //WalkPath, then Add/Remove Buttons.
            if (ImGui.SmallButton("Add a new Walking Path"))
            {
                var newWalkPath = new SpawnPoint.WalkPathDesc();

                // Ensure Position has at least one value
                if (newWalkPath.Positions == null || newWalkPath.Positions.Count == 0)
                {
                    newWalkPath.Positions = new List<Position>();
                }
                point.WalkPath.Add(newWalkPath);
            }
            ImGui.SameLine();
            if (point.WalkPath.Count > 0)
                if (ImGui.SmallButton("Remove last Walking Path"))
                    point.WalkPath.RemoveAt(point.WalkPath.Count - 1);
            ImGui.Separator();
            for (var i = 0; i < point.WalkPath.Count; i++)//WalkPath: Tree View
                ForTreeNode($"Walking path #{index}-{i}", () => Run(point.WalkPath[i], i));


            //Return Parameters, then Add/Remove Buttons.
            //Remove/Add
            if (ImGui.SmallButton("Add a new Return Parameter"))
                point.ReturnParameters.Add(new SpawnPoint.ReturnParameter());
            ImGui.SameLine();
            if (point.ReturnParameters.Count > 0)
                if (ImGui.SmallButton("Remove last Return Parameter"))
                    point.ReturnParameters.RemoveAt(point.ReturnParameters.Count - 1);
            ImGui.Separator();
            for (var i = 0; i < point.ReturnParameters.Count; i++)//Return parameters: Tree View
                ForTreeNode($"Parameter #{index}-{i}", () => Run(point.ReturnParameters[i], i));


            //Signal, then Add/Remove Buttons.
            //Remove/Add
            if (ImGui.SmallButton("Add a new Signal"))
                point.Signals.Add(new SpawnPoint.Signal());
            ImGui.SameLine();
            if (point.Signals.Count > 0)
                if (ImGui.SmallButton("Remove last Signal"))
                    point.Signals.RemoveAt(point.Signals.Count - 1);
            ImGui.Separator();
            for (var i = 0; i < point.Signals.Count; i++)//Signals: Tree View
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
            //Add new WalkingPath Positions so that NPCs have multiple possible paths.
            if (ImGui.SmallButton($"Add a new position##{index}"))
            {
                item.Positions.Add(new Position());
            }
            ImGui.SameLine();
            if (item.Positions.Count > 0)
                if (ImGui.SmallButton($"Remove last position"))
                {
                    item.Positions.RemoveAt(item.Positions.Count - 1);
                }
            ImGui.Separator();
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
            ForEdit($"Id##{index}", () => item.Id, x => item.Id = x);
            ForEdit($"Type##{index}", () => item.Type, x => item.Type = x);
            ForEdit($"Rate##{index}", () => item.Rate, x => item.Rate = x);
            ForEdit($"Entry Type##{index}", () => item.EntryType, x => item.EntryType = x);
            ForEdit($"Argument04##{index}", () => item.Argument04, x => item.Argument04 = x);
            ForEdit($"Argument08##{index}", () => item.Argument08, x => item.Argument08 = x);
            ForEdit($"Argument0c##{index}", () => item.Argument0c, x => item.Argument0c = x);
        }

        private static void Run(SpawnPoint.Signal item, int index)
        {
            ForEdit($"Signal ID##{index}", () => item.SignalId, x => item.SignalId = x);
            ForEdit($"Argument##{index}", () => item.Argument, x => item.Argument = x);
            ForEdit($"Action##{index}", () => item.Action, x => item.Action = x);
        }
    }
}
