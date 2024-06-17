using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using Simple3DViewport.Controls;
using Simple3DViewport.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    public class Viewport_ViewModel : NotifyPropertyChangedBase
    {
        public static FpsMode MODE_30_FPS = new FpsMode(33, 1);
        public static FpsMode MODE_15_FPS = new FpsMode(67, 2);
        public static FpsMode MODE_10_FPS = new FpsMode(100, 3);
        public static FpsMode MODE_5_FPS = new FpsMode(200, 6);
        public FpsMode currentFpsMode = MODE_10_FPS;

        public SimpleModel ThisModel { get; set; }
        public SimpleModel ThisCollisions { get; set; }

        // Animation
        public bool _animationRunning { get; set; }
        public bool AnimationRunning
        {
            get { return _animationRunning; }
            set
            {
                _animationRunning = value;
                OnPropertyChanged("AnimationRunning");
            }
        }
        public bool _renderCollisions { get; set; }
        public bool RenderCollisions
        {
            get { return _renderCollisions; }
            set
            {
                _renderCollisions = value;
                OnPropertyChanged("RenderCollisions");
            }
        }
        public bool _renderHitCollisions { get; set; }
        public bool RenderHitCollisions
        {
            get { return _renderHitCollisions; }
            set
            {
                _renderHitCollisions = value;
                OnPropertyChanged("RenderHitCollisions");
            }
        }
        public int? currentAnim { get; set; }
        private int? _currentFrame { get; set; }
        public int? CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                OnPropertyChanged("CurrentFrame");
            }
        }

        public Matrix4x4[] currentPose { get; set; }
        public Matrix4x4[] extraPose { get; set; }
        private AnbIndir currentAnb { get; set; }
        private IAnimMatricesProvider AnimMatricesProvider { get; set; }

        public Simple3DViewport_Control ViewportControl { get; set; }
        public bool enable_frameCommands
        {
            get
            {
                return MsetService.Instance.LoadedMotion != null;
            }
        }
        public bool enable_reload
        {
            get
            {
                return MdlxService.Instance.ModelFile != null;
            }
        }

        public Viewport_ViewModel()
        {
            AnimationRunning = false;
            RenderCollisions = true;
            RenderHitCollisions = false;
            CurrentFrame = 0;
            startAnimationTicker();

            subscribe_ObjectSelected();
            subscribe_MotionSelected();
        }

        public void clearViewport()
        {
            ViewportControl.VPModels.Clear();
            ThisModel = null;
            ThisCollisions = null;
            _animationRunning = false;
            currentAnim = null;
            CurrentFrame = null;
            currentPose = null;
            extraPose = null;
            currentAnb = null;
            AnimMatricesProvider = null;
        }

        public void loadModel(bool doCameraRestart)
        {
            AnimationRunning = false;
            foreach (ModelSkeletal.SkeletalGroup group in MdlxService.Instance.ModelFile.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, GetBoneMatrices(MdlxService.Instance.ModelFile.Bones));
            }

            if (MdlxService.Instance.ModelFile != null)
            {
                ThisModel = ViewportHelper.getModel(MdlxService.Instance.ModelFile, MdlxService.Instance.TextureFile);
                ViewportControl.VPModels.Clear();
                if (ThisModel != null)
                    ViewportControl.VPModels.Add(ThisModel);

                // Attachable
                if (AttachmentService.Instance.Attach_ModelFile != null)
                {
                    ViewportControl.VPModels.Add(ViewportHelper.getModel(AttachmentService.Instance.Attach_ModelFile, AttachmentService.Instance.Attach_TextureFile));
                }

                // Hit Collisions
                if (RenderHitCollisions && MdlxService.Instance.CollisionFile != null)
                {
                    SimpleModel model = new SimpleModel(getHitCollisions(null));
                    ViewportControl.VPModels.Add(model);
                }

                ViewportControl.render();
                if (doCameraRestart) ViewportControl.restartCamera();
            }
        }

        private void loadAnb()
        {
            if (!ObjectEditorUtils.isFilePathValid(App_Context.Instance.MdlxPath, "mdlx") || !ObjectEditorUtils.isFilePathValid(MsetService.Instance.MsetPath, "mset") || MsetService.Instance.LoadedMotion == null)
                return;

            Bar anbBarFile = Bar.Read(new MemoryStream(MsetService.Instance.MsetBinarc.Subfiles[MsetService.Instance.MsetBinarc.Entries[MsetService.Instance.LoadedMotionId].Link]));

            currentAnb = new AnbIndir(anbBarFile);
        }

        private void loadAnimProvider()
        {
            //using var mdlxStream = File.Open(Mdlx_Service.Instance.MdlxPath, FileMode.Open);

            // This is a test, model should be saved in the model module
            MdlxService.Instance.SaveModel();
            Stream mdlxStream = new MemoryStream();
            Bar.Write(mdlxStream, MdlxService.Instance.MdlxBar);
            mdlxStream.Position = 0;

            if (!mdlxStream.CanRead || !mdlxStream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            AnimMatricesProvider = currentAnb.GetAnimProvider(mdlxStream);
        }

        public void getMatricesForFrame(int frame)
        {
            loadAnimProvider();

            Matrix4x4[] matrices = AnimMatricesProvider.ProvideMatrices(frame);

            foreach (ModelSkeletal.SkeletalGroup group in MdlxService.Instance.ModelFile.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, matrices);
            }
        }

        public void loadFrame()
        {
            if (currentAnb == null)
                return;

            loadAnimProvider();

            ThisCollisions = new SimpleModel(new List<SimpleMesh>(), "COLLISIONS_1", new List<string> { "COLLISION", "COLLISION_GROUP" });

            if (!ObjectEditorUtils.isFilePathValid(MdlxService.Instance.MdlxPath, "mdlx") || !ObjectEditorUtils.isFilePathValid(MsetService.Instance.MsetPath, "mset") || MsetService.Instance.LoadedMotion == null)
                return;

            if (MsetService.Instance.LoadedMotion?.MotionFile?.KeyTimes == null || MsetService.Instance.LoadedMotion.MotionFile.KeyTimes.IsEmpty())
            {
                return;
            }

            int realFrameStart = (int)MsetService.Instance.LoadedMotion.MotionFile.KeyTimes[0];
            if (realFrameStart < 0) realFrameStart = 0;
            int realFrameEnd = (int)MsetService.Instance.LoadedMotion.MotionFile.KeyTimes[MsetService.Instance.LoadedMotion.MotionFile.KeyTimes.Count - 1];

            if (CurrentFrame < realFrameStart)
                CurrentFrame = realFrameEnd;

            if (CurrentFrame > realFrameEnd)
                CurrentFrame = realFrameStart;

            Matrix4x4[] matrices = AnimMatricesProvider.ProvideMatrices(CurrentFrame.Value);

            foreach (ModelSkeletal.SkeletalGroup group in MdlxService.Instance.ModelFile.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, matrices);
            }

            // Render the animated model
            ThisModel = ViewportHelper.getModel(MdlxService.Instance.ModelFile, MdlxService.Instance.TextureFile);
            ViewportControl.VPModels.Clear();
            if (ThisModel != null)
                ViewportControl.VPModels.Add(ThisModel);

            if (RenderCollisions)
            {
                loadCollisions(matrices);
                if (ThisCollisions != null)
                    ViewportControl.VPModels.Add(ThisCollisions);
            }

            if (AttachmentService.Instance.Attach_ModelFile != null)
            {
                foreach (SimpleModel model in getAttachmentModel(matrices))
                {
                    ViewportControl.VPModels.Add(model);
                }
            }

            if (RenderHitCollisions && MdlxService.Instance.CollisionFile != null)
            {
                SimpleModel model = new SimpleModel(getHitCollisions(matrices));
                ViewportControl.VPModels.Add(model);
            }

            ViewportControl.render();
        }

        // Ideally you'd parent bone 0 to the bone to be attached, however I can't get it to work, so this is the best approximation I could do. Not good enough for hitboxes, just for show
        public List<SimpleModel> getAttachmentModel(Matrix4x4[] modelMatrices)
        {
            App_Context test = App_Context.Instance;

            Stream temp_mdlxStream = new MemoryStream();
            Bar.Write(temp_mdlxStream, AttachmentService.Instance.Attach_MdlxBar);

            if (!temp_mdlxStream.CanRead || !temp_mdlxStream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");


            Bar attach_AnbBarFile = new Bar();
            try // Some animations don't use the weapon
            {
                attach_AnbBarFile = Bar.Read(new MemoryStream(MsetService.Instance.MsetBinarc.Subfiles[MsetService.Instance.MsetBinarc.Entries[MsetService.Instance.LoadedMotionId].Link]));
            }
            catch (Exception ex)
            {
                return new List<SimpleModel>();
            }

            AnbIndir attach_Anb = new AnbIndir(attach_AnbBarFile);

            IAnimMatricesProvider attach_AnimMatricesProvider = attach_Anb.GetAnimProvider(temp_mdlxStream);

            Matrix4x4[] matrices = attach_AnimMatricesProvider.ProvideMatrices(CurrentFrame.Value);

            foreach (ModelSkeletal.SkeletalGroup group in AttachmentService.Instance.Attach_ModelFile.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, matrices);
            }

            SimpleModel model = ViewportHelper.getModel(AttachmentService.Instance.Attach_ModelFile, AttachmentService.Instance.Attach_TextureFile);

            List<SimpleModel> modelList = new List<SimpleModel>();
            Simple3DViewport.Utils.Simple3DUtils.applyTransform(model.Meshes[0].Geometry, modelMatrices[AttachmentService.Instance.Attach_BoneId]);
            modelList.Add(model);

            return modelList;
        }

        public void nextFrame()
        {
            CurrentFrame += 1;
            loadCollisions(null);
            loadFrame();
        }
        public void previousFrame()
        {
            CurrentFrame -= 1;
            loadCollisions(null);
            loadFrame();
        }

        public void loadCollisions(Matrix4x4[] matrices)
        {
            if (MdlxService.Instance.CollisionFile != null)
            {
                List<short> collisionGroupsToLoad = new List<short>();

                if (MsetService.Instance.LoadedMotion.MotionTriggerFile?.RangeTriggerList == null)
                {
                    return;
                }

                foreach (MotionTrigger.RangeTrigger trigger in MsetService.Instance.LoadedMotion.MotionTriggerFile.RangeTriggerList)
                {
                    if ((trigger.StartFrame <= CurrentFrame || trigger.StartFrame == -1) && (trigger.EndFrame >= CurrentFrame || trigger.EndFrame == -1))
                    {
                        // Attack 1 hitbox
                        if (trigger.Trigger == 10)
                        {
                            // Param 1 is atkp entry
                            short hitbox1 = (short)(trigger.Param2);
                            if (!collisionGroupsToLoad.Contains(hitbox1))
                                collisionGroupsToLoad.Add(hitbox1);
                        }
                        // Attack 2 hitboxes
                        if (trigger.Trigger == 33)
                        {
                            // Param 1 is atkp entry
                            short hitbox1 = (short)(trigger.Param2);
                            short hitbox2 = (short)(trigger.Param3);
                            if (!collisionGroupsToLoad.Contains(hitbox1))
                                collisionGroupsToLoad.Add(hitbox1);
                            if (!collisionGroupsToLoad.Contains(hitbox2))
                                collisionGroupsToLoad.Add(hitbox2);
                        }
                        // Reaction collision
                        if (trigger.Trigger == 4)
                        {
                            short hitbox1 = (short)(trigger.Param1);
                            if (!collisionGroupsToLoad.Contains(hitbox1))
                                collisionGroupsToLoad.Add(hitbox1);
                        }
                    }
                }

                List<Collision_Wrapper> attackCollisions = new List<Collision_Wrapper>();
                for (int i = 0; i < MdlxService.Instance.CollisionFile.EntryList.Count; i++)
                {
                    ObjectCollision collision = MdlxService.Instance.CollisionFile.EntryList[i];
                    if (collisionGroupsToLoad.Contains(collision.Group))
                    {
                        attackCollisions.Add(new Collision_Wrapper("COLLISION_" + i, collision));
                    }
                }
                loadAttackCollisions(attackCollisions, matrices);
            }
        }

        public void subscribe_ObjectSelected()
        {
            App_Context.Instance.Event_ObjectSelected += new App_Context.EventHandler(sub_ObjectSelected);
        }
        private void sub_ObjectSelected(App_Context m, EventArgs e)
        {
            clearViewport();
            loadModel(true);
        }
        public void subscribe_MotionSelected()
        {
            App_Context.Instance.Event_MotionSelected += new App_Context.EventHandler(sub_MotionSelected);
        }
        private void sub_MotionSelected(App_Context m, EventArgs e)
        {
            CurrentFrame = 0;
            loadAnb();
            loadAnimProvider();
            loadFrame();
            //AnimationRunning = true;
        }

        // Animation ticker
        async Task startAnimationTicker()
        {
            TimeSpan timeInMs = currentFpsMode.MsBetweenTicks;

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
            if (!AnimationRunning)
            {
                //CurrentFrame = 0;
                return;
            }

            if (!ObjectEditorUtils.isFilePathValid(App_Context.Instance.MdlxPath, "mdlx") || !ObjectEditorUtils.isFilePathValid(MsetService.Instance.MsetPath, "mset") || MsetService.Instance.LoadedMotion == null)
                return;

            int timeTicked = currentFpsMode.FrameStep;

            CurrentFrame += timeTicked;
            loadFrame();
        }

        public List<SimpleMesh> getHitCollisions(Matrix4x4[] matrices)
        {
            List<SimpleMesh> list_hitMeshes = new List<SimpleMesh>();

            if (MdlxService.Instance.CollisionFile == null)
                return list_hitMeshes;

            Matrix4x4[] boneMatrices = new Matrix4x4[0];

            if (matrices != null)
                boneMatrices = matrices;
            else if (MdlxService.Instance.ModelFile != null)
                boneMatrices = ModelCommon.GetBoneMatrices(MdlxService.Instance.ModelFile.Bones);

            for (int i = 0; i < MdlxService.Instance.CollisionFile.EntryList.Count; i++)
            {
                ObjectCollision collision = MdlxService.Instance.CollisionFile.EntryList[i];

                if (collision.Type == (byte)ObjectCollision.TypeEnum.HIT)
                {

                    Vector3 basePosition = Vector3.Zero;
                    if (collision.Bone != 16384 && boneMatrices.Length != 0)
                    {
                        basePosition = Vector3.Transform(new Vector3(collision.PositionX, collision.PositionY, collision.PositionZ), boneMatrices[collision.Bone]);
                    }

                    list_hitMeshes.Add(getCollisionMesh(
                        "COLLISION_HIT_" + i,
                        new List<string> { "COLLISION", "COLLISION_HIT_SINGLE" },
                        collision.Shape,
                        new Vector3D(basePosition.X, basePosition.Y, basePosition.Z),
                        Color.FromArgb(100, 0, 0, 255),
                    collision.Radius,
                        collision.Height
                        ));
                }
            }

            return list_hitMeshes;
        }

        public void loadAttackCollisions(List<Collision_Wrapper> attackCollisions, Matrix4x4[] matrices)
        {
            if (MdlxService.Instance.CollisionFile != null)
            {
                Matrix4x4[] boneMatrices = new Matrix4x4[0];

                if (matrices != null)
                    boneMatrices = matrices;
                else if (MdlxService.Instance.ModelFile != null)
                    boneMatrices = ModelCommon.GetBoneMatrices(MdlxService.Instance.ModelFile.Bones);

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


                    simpleMeshes.Add(getCollisionMesh(
                        "COLLISION_" + i,
                        new List<string> { "COLLISION", "COLLISION_SINGLE" },
                        collision.Shape,
                        new Vector3D(basePosition.X, basePosition.Y, basePosition.Z),
                        color,
                        collision.Radius,
                        collision.Height
                        ));
                }

                ThisCollisions = new SimpleModel(simpleMeshes, "COLLISIONS_1", new List<string> { "COLLISION", "COLLISION_GROUP" });
            }
        }

        public static SimpleMesh getCollisionMesh(string id, List<string> labels, byte shape, Vector3D position, Color color, short width, short height)
        {
            SimpleMesh thisMesh = null;

            if (shape == (byte)ObjectCollision.ShapeEnum.ELLIPSOID)
            {
                thisMesh = new SimpleMesh(Simple3DViewport.Utils.GeometryShapes.getEllipsoid(width, height, 10, position, color), id, labels);
            }
            else if (shape == (byte)ObjectCollision.ShapeEnum.COLUMN)
            {
                thisMesh = new SimpleMesh(Simple3DViewport.Utils.GeometryShapes.getCylinder(width, height, 10, position, color), id, labels);
            }
            else if (shape == (byte)ObjectCollision.ShapeEnum.CUBE)
            {
                thisMesh = new SimpleMesh(Simple3DViewport.Utils.GeometryShapes.getCuboid(width, height, width, position, color), id, labels);
            }
            else if (shape == (byte)ObjectCollision.ShapeEnum.SPHERE)
            {
                thisMesh = new SimpleMesh(Simple3DViewport.Utils.GeometryShapes.getSphere(width, 10, position, color), id, labels);
            }

            return thisMesh;
        }

        public void reload()
        {
            AnimationRunning = false;
            CurrentFrame = 0;
            clearViewport();
            loadModel(false);
        }

        public class FpsMode
        {
            public TimeSpan MsBetweenTicks { get; set; }
            public int FrameStep { get; set; }

            public FpsMode(int msBetweenTicks, int frameStep) { MsBetweenTicks = TimeSpan.FromMilliseconds(msBetweenTicks); FrameStep = frameStep; }
        }
    }
}
