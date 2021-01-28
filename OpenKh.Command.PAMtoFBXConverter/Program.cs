using OpenKh.Engine.Parsers;
using OpenKh.Imaging;

using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Bbs;
using System.Collections.Generic;
using System.Numerics;
using OpenKh.Common.Utils;
using Assimp.Unmanaged;
using Assimp;

namespace OpenKh.Command.PAMtoFBXConverter
{
    [Command("OpenKh.Command.PAMtoFBXConverter")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "PMO File", "The PMO to use as a base for the conversion.")]
        public string PMO_FileName { get; }

        [Required]
        [Argument(1, "PAM File", "The PAM animation set to use for the conversion.")]
        public string PAM_FileName { get; }

        private void OnExecute()
        {
            try
            {
                Convert(PMO_FileName, PAM_FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static void Convert(string PMO_Path, string PAM_Path)
        {
            Stream pmoStream = File.OpenRead(PMO_Path);
            Stream pamStream = File.OpenRead(PAM_Path);
            Pmo pmo = Pmo.Read(pmoStream);
            Pam pam = Pam.Read(pamStream);

            Assimp.Scene nScene = GetPMOScene(pmo);
            List<Assimp.Animation> FBXAnims = PAMtoFBXAnim(pam);

            pmoStream.Close();
            pamStream.Close();

            using var ctx = new AssimpContext();
            ctx.ExportFile(nScene, "Test.fbx", "fbx");
            
        }

        private static Assimp.Scene GetPMOScene(Pmo pmo)
        {
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("root");

            for(int i = 0; i < pmo.Meshes.Count; i++)
            {
                Assimp.Mesh mesh = new Assimp.Mesh($"Mesh{i}", Assimp.PrimitiveType.Triangle);
                Pmo.MeshChunks chunk = pmo.Meshes[i];
                List<int> indices = new List<int>();
                for (int j = 0; j < chunk.vertices.Count; j++)
                {
                    mesh.Vertices.Add(new Assimp.Vector3D(
                        chunk.vertices[j].X * pmo.header.ModelScale * 100.0f,
                        chunk.vertices[j].Y * pmo.header.ModelScale * 100.0f,
                        chunk.vertices[j].Z * pmo.header.ModelScale * 100.0f));

                    mesh.Faces.Add(new Face(new int[]
                    {
                        (j * 3),
                        (j * 3) + 1,
                        (j * 3) + 2
                    }));
                    
                }
                mesh.MaterialIndex = 0;

                var material = new Material();
                material.Clear();

                scene.Materials.Add(material);

                //mesh.SetIndices(indices.ToArray(), 3);
                scene.Meshes.Add(mesh);
            }

            scene.RootNode.MeshIndices.AddRange(Enumerable.Range(0, scene.MeshCount));

            return scene;
        }

        private static List<Assimp.Animation> PAMtoFBXAnim(Pam pam)
        {
            List<Assimp.Animation> animationList = new List<Assimp.Animation>();

            for(int i = 0; i < pam.header.AnimationCount; i++)
            {
                Assimp.Animation anim = new Assimp.Animation();
                anim.Name = pam.animList[i].AnimEntry.AnimationName;
                
                animationList.Add(anim);
            }

            return animationList;
        }
    }
}
