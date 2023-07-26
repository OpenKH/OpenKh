using OpenKh.Tools.Kh2ObjectEditor.Utils;
using Simple3DViewport.Objects;
using Simple3DViewport.Controls;
using System;
using System.IO;
using System.Text.Json;
using OpenKh.Kh2Anim.Mset;
using System.Numerics;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Kh2.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Documents;
using System.Collections.Generic;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class Viewport_ViewModel
    {
        public Main_ViewModel MainVM { get; set; }
        public SimpleModel ThisModel { get; set; }
        public SimpleModel ThisCollisions { get; set; }

        // Animation
        public bool animationRunning { get; set; }
        public int currentAnim { get; set; }
        public int currentFrame { get; set; }
        public Matrix4x4[] currentPose { get; set; }
        public Matrix4x4[] extraPose { get; set; }

        public Simple3DViewport_Control ViewportControl { get; set; }

        public Viewport_ViewModel()
        {
            MainVM = new Main_ViewModel();
            animationRunning = false;
            startAnimationTicker();
            Subscribe();
            Subscribe2();
        }

        public Viewport_ViewModel(Main_ViewModel mainVM, Simple3DViewport_Control viewportControl)
        {
            MainVM = mainVM;
            ViewportControl = viewportControl;
            animationRunning = false;
            startAnimationTicker();
            loadModel();
            Subscribe();
            Subscribe2();
        }

        public void loadModel()
        {
            if (MainVM.LoadedObject != null)
            {
                ThisModel = ViewportHelper.getModel(MainVM.LoadedObject.ModelFile, MainVM.LoadedObject.TextureFile);
                ViewportControl.VPModels.Clear();
                if(ThisModel != null) ViewportControl.VPModels.Add(ThisModel);
                ViewportControl.render();
                ViewportControl.restartCamera();
            }
        }

        public void loadFrame()
        {
            if (!MainVM.LoadedObject.isMdlxPathValid() || !MainVM.LoadedObject.isMsetPathValid() || MainVM.LoadedMotion == null)
                return;

            animationRunning = true;

            MainVM.LoadedMotion.Entry.Stream.Position = 0;
            AnimationBinary animBinary = new AnimationBinary(MainVM.LoadedMotion.Entry.Stream);

            if (currentFrame > animBinary.MotionFile.InterpolatedMotionHeader.FrameCount)
                currentFrame = 0;

            MainVM.LoadedMotion.Entry.Stream.Position = 0;
            Bar anbBarFile = Bar.Read(MainVM.LoadedMotion.Entry.Stream);

            AnbIndir andTest = new AnbIndir(anbBarFile); // This is the first anb from Sora's mset

            using var stream = File.Open(MainVM.LoadedObject.MdlxPath, FileMode.Open);

            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            IAnimMatricesProvider animProvider = andTest.GetAnimProvider(stream); // This is Sora's MDLX

            Matrix4x4[] matrices = animProvider.ProvideMatrices(currentFrame); // See matrices at time 0

            string jsonString = JsonSerializer.Serialize(matrices);
            foreach (Matrix4x4 matr in matrices)
            {
                //System.Diagnostics.Debug.WriteLine(matr);
            }

            foreach (ModelSkeletal.SkeletalGroup group in MainVM.LoadedObject.ModelFile.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, matrices);
            }

            //foreach(animBinary.MotionTriggerFile.RangeTriggerList)

            // Render the animated model
            loadCollisions(matrices);
            ThisModel = ViewportHelper.getModel(MainVM.LoadedObject.ModelFile, MainVM.LoadedObject.TextureFile);
            ViewportControl.VPModels.Clear();
            if (ThisModel != null)
                ViewportControl.VPModels.Add(ThisModel);
            if(ThisCollisions != null)
                ViewportControl.VPModels.Add(ThisCollisions);
            ViewportControl.render();

            // stream cleanup
            MainVM.LoadedMotion.Entry.Stream.Position = 0;
        }

        public void nextFrame()
        {
            int timeTicked = 3;
            currentFrame += timeTicked;
            loadCollisions(null);
            loadFrame();
        }

        public void loadCollisions(Matrix4x4[] matrices)
        {
            if(MainVM.LoadedObject.CollisionFile != null)
            {
                MainVM.LoadedMotion.Entry.Stream.Position = 0;
                AnimationBinary animBinary = new AnimationBinary(MainVM.LoadedMotion.Entry.Stream);
                MainVM.LoadedMotion.Entry.Stream.Position = 0;

                List<short> collisionsToLoad = new List<short>();

                if(animBinary.MotionTriggerFile?.RangeTriggerList == null)
                {
                    return;
                }

                foreach(MotionTrigger.RangeTrigger trigger in animBinary.MotionTriggerFile.RangeTriggerList)
                {
                    if (trigger.Trigger == 10 && currentFrame >= trigger.StartFrame && currentFrame <= trigger.EndFrame)
                    {
                        collisionsToLoad.Add((short)trigger.Param2);
                    }
                }

                List<Collision_Wrapper> attackCollisions = new List<Collision_Wrapper>();
                for (int i = 0; i < MainVM.LoadedObject.CollisionFile.EntryList.Count; i++)
                {
                    ObjectCollision collision = MainVM.LoadedObject.CollisionFile.EntryList[i];
                    if(collision.Type == (byte)ObjectCollision.TypeEnum.ATTACK)
                    {
                        attackCollisions.Add(new Collision_Wrapper("COLLISION_" + i, collision));
                    }
                }

                List<Collision_Wrapper> finalAttackCollisions = new List<Collision_Wrapper>();
                foreach(short ctl in collisionsToLoad)
                {
                    finalAttackCollisions.Add(attackCollisions[ctl]);
                }
                loadAttackCollisions(finalAttackCollisions, matrices);
            }
        }

        // Event load file
        public void Subscribe()
        {
            MainVM.Load += new Main_ViewModel.EventHandler(MyFunction);
        }
        private void MyFunction(Main_ViewModel m, EventArgs e)
        {
            loadModel();
        }
        public void Subscribe2()
        {
            MainVM.MotionSelect += new Main_ViewModel.EventHandler(MyFunction2);
        }
        private void MyFunction2(Main_ViewModel m, EventArgs e)
        {
            currentFrame = 0;
            animationRunning = true;
        }

        // Animation ticker
        async Task startAnimationTicker()
        {
            TimeSpan timeInMs = TimeSpan.FromMilliseconds(67);

            PeriodicTimer animationTimer = new PeriodicTimer(timeInMs);
            while (await animationTimer.WaitForNextTickAsync())
            {
                try
                {
                    animationTicker();
                }
                catch (Exception e) { }
            }
        }

        public void animationTicker()
        {
            if (!animationRunning)
            {
                currentFrame = 0;
                return;
            }

            if (!MainVM.LoadedObject.isMdlxPathValid() || !MainVM.LoadedObject.isMsetPathValid() || MainVM.LoadedMotion == null)
                return;

            int timeTicked = 2;

            currentFrame += timeTicked;
            loadFrame();
        }

        public void loadAttackCollisions(List<Collision_Wrapper> attackCollisions, Matrix4x4[] matrices)
        {
            if (MainVM.LoadedObject.CollisionFile != null)
            {
                Matrix4x4[] boneMatrices = new Matrix4x4[0];

                if (matrices != null)
                    boneMatrices = matrices;
                else if (MainVM.LoadedObject.ModelFile != null)
                    boneMatrices = ModelCommon.GetBoneMatrices(MainVM.LoadedObject.ModelFile.Bones);

                List<SimpleMesh> simpleMeshes = new List<SimpleMesh>();

                for (int i = 0; i < attackCollisions.Count; i++)
                {
                    ObjectCollision collision = attackCollisions[i].Collision;

                    Vector3 basePosition = Vector3.Zero;
                    if (collision.Bone != 16384 && boneMatrices.Length != 0)
                    {
                        basePosition = Vector3.Transform(new Vector3(collision.PositionX, collision.PositionY, collision.PositionZ), boneMatrices[collision.Bone]);
                    }


                    Color color = new Color();
                    if (collision.Type == (byte)ObjectCollision.TypeEnum.HIT)
                    {
                        color = Color.FromArgb(100, 0, 0, 255);
                    }
                    else if (collision.Type == (byte)ObjectCollision.TypeEnum.REACTION)
                    {
                        color = Color.FromArgb(100, 0, 255, 0);
                    }
                    else
                    {
                        color = Color.FromArgb(100, 255, 0, 0);
                    }

                    if (collision.Shape == (byte)ObjectCollision.ShapeEnum.ELLIPSOID)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getEllipsoid(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.COLUMN)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCylinder(collision.Radius, collision.Height, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.CUBE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getCuboid(collision.Radius, collision.Height, collision.Radius, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                    else if (collision.Shape == (byte)ObjectCollision.ShapeEnum.SPHERE)
                    {
                        simpleMeshes.Add(new SimpleMesh(
                            Simple3DViewport.Utils.GeometryShapes.getSphere(collision.Radius, 10, new Vector3D(basePosition.X, basePosition.Y, basePosition.Z), color),
                            "COLLISION_" + i,
                            new List<string> { "COLLISION", "COLLISION_SINGLE" }
                            ));
                    }
                }

                ThisCollisions = new SimpleModel(simpleMeshes, "COLLISIONS_1", new List<string> { "COLLISION", "COLLISION_GROUP" });
            }
        }
    }
}
