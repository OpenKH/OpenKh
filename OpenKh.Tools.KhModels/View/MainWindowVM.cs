using HelixToolkit.Wpf;
using ModelingToolkit.AssimpModule;
using ModelingToolkit.HelixModule;
using ModelingToolkit.Objects;
using OpenKh.AssimpUtils;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Ddd;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Tools.KhModels.BBS;
using OpenKh.Tools.KhModels.DDD;
using OpenKh.Tools.KhModels.KH1;
using OpenKh.Tools.KhModels.KIH2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using static OpenKh.Ddd.PmoV4;

namespace OpenKh.Tools.KhModels.View
{
    public class MainWindowVM
    {
        public ViewportController VpController { get; set; }
        public string Filepath { get; set; }
        // Options
        public bool ShowMesh { get; set; }
        public bool ShowWireframe { get; set; }
        public bool ShowSkeleton { get; set; }
        public bool ShowJoints { get; set; }
        public bool ShowBoundingBox { get; set; }
        public bool ShowOrigin { get; set; }

        public MainWindowVM(HelixViewport3D viewport)
        {
            VpController = new ViewportController(viewport);
            SetDefaultOptions();
        }

        public void SetDefaultOptions()
        {
            ShowMesh = true;
            ShowWireframe = false;
            ShowSkeleton = true;
            ShowJoints = true;
            ShowBoundingBox = false;
            ShowOrigin = false;
            SetOptions();
        }

        public void SetOptions()
        {
            VpController.SetVisibilityMesh(ShowMesh);
            VpController.SetVisibilityWireframe(ShowWireframe);
            VpController.SetVisibilitySkeleton(ShowSkeleton);
            VpController.SetVisibilityJoint(ShowJoints);
            VpController.SetVisibilityBoundingBox(ShowBoundingBox);
            VpController.SetVisibilityOrigin(ShowOrigin);
        }

        public void LoadFilepath(string filepath)
        {
            Filepath = filepath;
            LoadFile();
        }

        public void LoadFile()
        {
            if(Filepath == null)
                throw new Exception("File not set");

            string fileName = Path.GetFileNameWithoutExtension(Filepath);

            Stopwatch sw = Stopwatch.StartNew(); // Profiling

            List<MtModel> mtModels = new List<MtModel>();

            // External model
            if (Filepath.ToLower().EndsWith(".fbx") || Filepath.ToLower().EndsWith(".dae"))
            {
                MtModel model = AssimpImporter.ImportScene(Filepath);
                model.Name = fileName;
                mtModels.Add(model);
            }
            // KH1
            else if (Filepath.ToLower().EndsWith(".mdls"))
            {
                Mdls mdlsModel = new Mdls(Filepath);
                MtModel model = MdlsProcessor.GetMtModel(mdlsModel);
                model.Name = fileName;
                model.CalculateFromRelativeData();
                mtModels.Add(model);
            }
            // KH1 - weapon
            else if (Filepath.ToLower().EndsWith(".wpn"))
            {
                Wpn wpnModel = new Wpn(Filepath);
                MtModel model = WpnProcessor.GetMtModel(wpnModel);
                model.Name = fileName;
                mtModels.Add(model);
            }
            // KH2
            else if (Filepath.ToLower().EndsWith(".mdlx"))
            {
                using (var stream = File.Open(Filepath, FileMode.Open))
                {
                    if (!Bar.IsValid(stream))
                        return;

                    Bar barFile = Bar.Read(stream);
                    MtModel model = MdlxProcessor.GetMtModel(barFile);
                    model.Name = fileName;
                    mtModels.Add(model);
                }
            }
            // KH BBS - DDD
            else if (Filepath.ToLower().EndsWith(".pmo"))
            {
                byte pmoVersion = 0;
                using (FileStream stream = new FileStream(Filepath, FileMode.Open))
                {
                    stream.Position = 6;
                    pmoVersion = stream.PeekByte();
                }

                // BBS
                if (pmoVersion == 3)
                {
                    FileStream stream = new FileStream(Filepath, FileMode.Open);
                    stream.Position = 0;
                    Pmo pmoModel = Pmo.Read(stream);
                    MtModel model = PmoProcessor.GetMtModel(pmoModel);
                    model.Name = fileName;
                    mtModels.Add(model);
                }
                // DDD
                else if (pmoVersion == 4)
                {
                    PmoV4_2 pmoModel = PmoV4_2.Read(Filepath);
                    MtModel model = PmoV4Processor.GetMtModel(pmoModel);
                    model.Name = fileName;
                    mtModels.Add(model);
                }
            }
            //KH 2 Map
            else if (Filepath.ToLower().EndsWith(".map"))
            {
                using (var stream = File.Open(Filepath, FileMode.Open))
                {
                    if (!Bar.IsValid(stream))
                        return;

                    Bar barFile = Bar.Read(stream);
                    mtModels = MapProcessor.GetMtModels(barFile);
                }
            }
            //KH BBS Map
            else if (Filepath.ToLower().EndsWith(".arc"))
            {
                FileStream stream = new FileStream(Filepath, FileMode.Open);
                stream.Position = 0;
                IEnumerable<Arc.Entry> arcFiles = Arc.Read(stream);
                Pmp pmpModel = new Pmp();
                foreach (Arc.Entry entry in arcFiles)
                {
                    if (entry.Name.EndsWith(".pmp"))
                    {
                        using (Stream pmpStream = new MemoryStream(entry.Data))
                        {
                            pmpModel = Pmp.Read(pmpStream);
                            for (int i = 0; i < pmpModel.PmoList.Count; i++)
                            {
                                Pmo pmoModel = pmpModel.PmoList[i];
                                if (pmoModel != null)
                                {
                                    MtModel model = PmoProcessor.GetMtModel(pmoModel);
                                    model.Name = "Model" + i.ToString("D4");
                                    mtModels.Add(model);
                                }
                            }
                        }
                    }
                }
            }
            // KH DDD Map
            else if (Filepath.ToLower().EndsWith(".pmp"))
            {
                Debug.WriteLine("START: " + sw.ElapsedMilliseconds);
                FileStream stream = new FileStream(Filepath, FileMode.Open);
                stream.Position = 0;
                PmpDdd pmpModel = PmpDdd.Read(stream);
                Debug.WriteLine("PMP READ: " + sw.ElapsedMilliseconds);
                for (int i = 0; i < pmpModel.PmoList.Count; i++)
                {
                    PmoV4_2 pmoModel = pmpModel.PmoList[i];
                    if (pmoModel != null)
                    {
                        MtModel model = PmoV4Processor.GetMtModel(pmoModel);
                        model.Name = "Model" + i.ToString("D4");
                        System.Numerics.Matrix4x4 modelTransformation = pmpModel.objectInfo[i].GetTransformationMatrix();
                        foreach(MtMesh mesh in model.Meshes)
                        {
                            foreach(MtVertex vertex in mesh.Vertices)
                            {
                                vertex.AbsolutePosition = Vector3.Transform(vertex.AbsolutePosition.Value, modelTransformation);
                            }
                            if (pmpModel.objectInfo[i].ShouldMirrorFaces())
                            {
                                foreach (MtFace face in mesh.Faces)
                                {
                                    face.Clockwise = !face.Clockwise;
                                }
                            }
                        }
                        mtModels.Add(model);
                    }
                }
                Debug.WriteLine("PMP CONVERTED: " + sw.ElapsedMilliseconds);
            }

            if (mtModels.Count > 0)
            {
                VpController.ClearModels();
                foreach(MtModel model in mtModels)
                {
                    VpController.AddModel(model, loadWireframe: false, loadBoundingBox: false);
                }
                VpController.Render();
            }
            Debug.WriteLine("PMP RENDERED: " + sw.ElapsedMilliseconds);
        }

        public void ExportModel(AssimpGeneric.FileFormat fileFormat = AssimpGeneric.FileFormat.fbx)
        {
            //MtModel model = VpService.Models[0];
            //Assimp.Scene scene = AssimpExporter.ExportScene(model);

            Assimp.Scene scene = AssimpExporter.ExportScene(VpController.ModelVisuals.Keys.ToList());

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export model";
            sfd.FileName = (VpController.ModelVisuals.Keys.Count == 1) ? VpController.ModelVisuals.Keys.ToArray()[0].Name + "." + AssimpGeneric.GetFormatFileExtension(fileFormat) : "Map." + AssimpGeneric.GetFormatFileExtension(fileFormat);
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                string dirPath = Path.GetDirectoryName(sfd.FileName);

                AssimpGeneric.ExportScene(scene, fileFormat, sfd.FileName);
                foreach(MtModel model in VpController.ModelVisuals.Keys.ToArray())
                {
                    foreach (MtMaterial material in model.Materials)
                    {
                        if (material.DiffuseTextureBitmap != null)
                        {
                            string textureName = (VpController.ModelVisuals.Keys.ToArray().Length == 1) ? Path.Combine(dirPath, material.Name + ".png") : Path.Combine(dirPath, model.Name + "." + material.Name + ".png");
                            material.ExportAsPng(textureName);
                        }
                    }
                }
            }
        }

        public void ExportMdls(AssimpGeneric.FileFormat fileFormat = AssimpGeneric.FileFormat.fbx)
        {
            if (Filepath == null)
                return;

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export model";
            sfd.FileName = Filepath + "." + AssimpGeneric.GetFormatFileExtension(fileFormat);
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                string dirPath = System.IO.Path.GetDirectoryName(sfd.FileName);
                if (!Directory.Exists(dirPath))
                    return;

                Mdls model = new Mdls(Filepath);
                Assimp.Scene scene = Kh1MdlsAssimp.getAssimpScene(model);

                AssimpGeneric.ExportScene(scene, fileFormat, sfd.FileName);

                // Textures
                for (int i = 0; i < model.Images.Count; i++)
                {
                    //model.Images[i].loadBitmap();

                    string textureName = "Texture" + i.ToString("D4");
                    string outPath = System.IO.Path.Combine(dirPath, textureName + ".png");
                    model.Images[i].bitmap.Save(outPath, ImageFormat.Png);
                }
            }
        }
    }
}
