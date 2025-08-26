using HelixToolkit.Wpf;
using ModelingToolkit.HelixModule;
using ModelingToolkit.Objects;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class ViewerService : NotifyPropertyChangedBase
    {
        public ViewportController VpController { get; set; }
        public MtModel LoadedModel { get; set; }
        public MdlxService ServiceMdlx { get; set; } = MdlxService.Instance;
        //-------------------------------------
        // Animation
        //-------------------------------------
        private int _currentFrame { get; set; }
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                _currentFrame = value;
                OnPropertyChanged("CurrentFrame");
            }
        }
        private int _motionMinFrame { get; set; }
        public int MotionMinFrame
        {
            get { return _motionMinFrame; }
            set
            {
                _motionMinFrame = value;
                OnPropertyChanged("MotionMinFrame");
            }
        }
        private int _motionMaxFrame { get; set; }
        public int MotionMaxFrame
        {
            get { return _motionMaxFrame; }
            set
            {
                _motionMaxFrame = value;
                OnPropertyChanged("MotionMaxFrame");
            }
        }
        private bool _animationRunning { get; set; }
        public bool AnimationRunning
        {
            get { return _animationRunning; }
            set
            {
                _animationRunning = value;
                OnPropertyChanged("AnimationRunning");
            }
        }
        //-------------------------------------
        // Reder Options
        //-------------------------------------
        public bool _autoCollisions { get; set; }
        public bool AutoCollisions
        {
            get { return _autoCollisions; }
            set
            {
                _autoCollisions = value;
                if (!value)
                {
                    foreach (MtShape shape in VpController.ShapeVisuals.Keys)
                    {
                        shape.IsVisible = false;
                    }
                }
                VpController.Render();
                OnPropertyChanged("AutoCollisions");
            }
        }
        private bool _autoCollisionsAttack { get; set; }
        public bool AutoCollisionsAttack
        {
            get { return _autoCollisionsAttack; }
            set
            {
                _autoCollisionsAttack = value;
                OnPropertyChanged("AutoCollisionsAttack");
            }
        }
        private bool _autoCollisionsOther { get; set; }
        public bool AutoCollisionsOther
        {
            get { return _autoCollisionsOther; }
            set
            {
                _autoCollisionsOther = value;
                OnPropertyChanged("AutoCollisionsOther");
            }
        }
        private bool _isBoundingBoxVisible { get; set; }
        public bool IsBoundingBoxVisible
        {
            get { return _isBoundingBoxVisible; }
            set
            {
                _isBoundingBoxVisible = value;
                MakeBoundingBoxVisible(value);
                OnPropertyChanged("IsBoundingBoxVisible");
            }
        }
        //-------------------------------------
        // Skeleton helper
        //-------------------------------------
        private IAnimMatricesProvider _animMatricesProvider { get; set; }
        private AnbIndir _loadedAnb { get; set; }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // SINGLETON
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public ViewerService()
        {
            AutoCollisionsAttack = true;
            AutoCollisionsOther = true;
            IsBoundingBoxVisible = false;
            StartAnimationTicker();
        }
        private static ViewerService _instance = null;
        public static ViewerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ViewerService();
                }
                return _instance;
            }
        }
        public static void Reset()
        {
            _instance = new ViewerService();
        }
        public void HookViewport(HelixViewport3D viewport)
        {
            Instance.VpController = new ViewportController(viewport);
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // FUNCTIONS
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Render()
        {
            VpController.ClearModels();
            VpController.ClearShapes();
            LoadModel();
            LoadCollisions();
            VpController.Render();
        }

        public void LoadModel()
        {
            if (MdlxService.Instance.MdlxBar == null) {
                return;
            }
            LoadedModel = MdlxProcessor.GetMtModel(MdlxService.Instance.MdlxBar);
            VpController.AddModel(LoadedModel);
            VpController.ResetCamera();
        }
        public void LoadCollisions()
        {
            if (MdlxService.Instance.CollisionFile?.EntryList == null) {
                return;
            }

            VpController.ClearShapes();
            for (int i = 0; i < MdlxService.Instance.CollisionFile.EntryList.Count; i++)
            {
                VpController.AddShape(GetCollisionShape(i));
            }
        }
        public void ShowCollisions(List<int> collisionIds)
        {
            List<MtShape> shapes = new List< MtShape >();
            foreach (int collisionId in collisionIds)
            {
                shapes.AddRange(VpController.FindShapesByName(GetCollisionName(collisionId)));
            }
            foreach(MtShape shape in VpController.ShapeVisuals.Keys)
            {
                shape.IsVisible = shapes.Contains(shape);
            }
            VpController.Render();
        }
        public void ShowCollision(int collisionId)
        {
            foreach (MtShape shape in VpController.FindShapesByName(GetCollisionName(collisionId)))
            {
                shape.IsVisible = true;
            }
            VpController.Render();
        }
        public void HideCollision(int collisionId)
        {
            foreach (MtShape shape in VpController.FindShapesByName(GetCollisionName(collisionId))) {
                shape.IsVisible = false;
            }
            VpController.Render();
        }

        public void LoadBoundingBox()
        {
            if (MsetService.Instance.LoadedMotion?.MotionFile?.InterpolatedMotionHeader?.BoundingBox == null)
            {
                return;
            }

            foreach(MtShape shape in VpController.ShapeVisuals.Keys)
            {
                if(shape.Name == "BB")
                {
                    VpController.RemoveShape(shape);
                }
            }

            float sizeX = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxX - MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinX);
            float sizeY = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxY - MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinY);
            float sizeZ = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxZ - MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMinZ);

            float centerX = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxX - (sizeX / 2));
            float centerY = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxY - (sizeY / 2));
            float centerZ = (float)(MsetService.Instance.LoadedMotion?.MotionFile.InterpolatedMotionHeader.BoundingBox.BoundingBoxMaxX - (sizeZ / 2));

            MtShape bbShape = MtShape.CreateBoundingBox(new Vector3(centerZ, centerX, centerY), sizeZ, sizeX, sizeY);
            bbShape.Name = "BB";
            bbShape.IsVisible = IsBoundingBoxVisible;
            bbShape.ShapeColor = Color.FromArgb(255, 255, 255, 0);
            VpController.AddShape(bbShape);
        }

        public void MakeBoundingBoxVisible(bool isVisible)
        {
            // Prevent initialization crash
            if(VpController == null) {
                return;
            }

            foreach (MtShape shape in VpController.FindShapesByName("BB"))
            {
                shape.IsVisible = isVisible;
            }
        }

        public void LoadMotion()
        {
            if(MsetService.Instance.LoadedMotion?.MotionFile == null) {
                throw new System.Exception("Viewport: There's no moveset loaded");
            }

            if(MsetService.Instance.LoadedMotion.MotionFile.InterpolatedMotionHeader.BoneCount >= 300)
            {
                SetFpsMode(MODE_10_FPS);
            }
            else
            {
                SetFpsMode(MODE_15_FPS);
            }

            LoadAnbIndir();
            LoadAnimProvider();

            float adjustFactor = 60.0f / (float)MsetService.Instance.LoadedMotion.MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond;

            MotionMinFrame = (int)((int)MsetService.Instance.LoadedMotion.MotionFile.InterpolatedMotionHeader.FrameData.FrameStart * adjustFactor);
            MotionMaxFrame = (int)((int)MsetService.Instance.LoadedMotion.MotionFile.InterpolatedMotionHeader.FrameData.FrameEnd * adjustFactor);
            CurrentFrame = MotionMinFrame;

            LoadPoseForFrame(MotionMinFrame);

            LoadBoundingBox();
        }

        public void FrameIncrease(int amount)
        {
            CurrentFrame += amount;
            if (CurrentFrame > MotionMaxFrame) {
                CurrentFrame = MotionMinFrame;
            }
            else if (CurrentFrame < MotionMinFrame) {
                CurrentFrame = MotionMaxFrame;
            }
        }
        public void LoadFrame()
        {
            LoadPoseForFrame(CurrentFrame);
        }
        private void LoadPoseForFrame(int frame)
        {
            LoadAnimProvider();
            Matrix4x4[] matrices = _animMatricesProvider.ProvideMatrices(frame);
            for (int i = 0; i < LoadedModel.Joints.Count; i++) {
                LoadedModel.Joints[i].AbsoluteTransformationMatrix = matrices[i];
            }
            CalcVerticesPositionFromPose();
            UpdateMeshVertices();
            RecalcCollisionPositions();
            CheckFrameShapeVisibility();

            VpController.Render();
        }

        private void UpdateMeshVertices()
        {
            List< GeometryModel3D > oldMeshes = new List< GeometryModel3D >();
            foreach (var child in ((Model3DGroup)VpController.ModelVisuals[LoadedModel].Content).Children)
            {
                if (child is GeometryModel3D geometryModel)
                {
                    oldMeshes.Add(geometryModel);
                }
            }

            for (int i = 0; i < LoadedModel.Meshes.Count; i++)
            {
                MtMesh mesh = LoadedModel.Meshes[i];
                GeometryModel3D oldMesh = oldMeshes[i];
                Point3DCollection points = ((MeshGeometry3D)oldMesh.Geometry).Positions;
                points.Clear();

                foreach (MtVertex vertex in mesh.Vertices)
                {
                    points.Add(new Point3D(vertex.AbsolutePosition.Value.X, vertex.AbsolutePosition.Value.Y, vertex.AbsolutePosition.Value.Z));
                }
            }
        }

        // Calculates the mesh based on the pose for single-weighted models
        private void CalcVerticesPositionFromPose()
        {
            foreach(MtMesh mesh in LoadedModel.Meshes)
            {
                foreach(MtVertex vertex in mesh.Vertices)
                {
                    vertex.AbsolutePosition = Vector3.Transform(vertex.Weights[0].RelativePosition.Value, LoadedModel.Joints[vertex.Weights[0].JointIndex.Value].AbsoluteTransformationMatrix.Value);
                }
            }
        }

        private void RecalcCollisionPositions()
        {
            List<int> visibleShapeIndices = new List<int>();
            foreach(MtShape shape in VpController.ShapeVisuals.Keys)
            {
                if (shape.IsVisible && shape.Name.StartsWith("COL_"))
                {
                    int underscoreIndex = shape.Name.IndexOf("COL_");
                    int.TryParse(shape.Name.Substring(underscoreIndex + 4), out int shapeIndex);
                    visibleShapeIndices.Add(shapeIndex);
                }
            }
            //for (int i = 0; i < MdlxService.Instance.CollisionFile.EntryList.Count; i++)
            foreach(int i in visibleShapeIndices)
            {
                ObjectCollision collisionEntry = MdlxService.Instance.CollisionFile.EntryList[i];
                Vector3 basePosition = Vector3.Zero;
                if (collisionEntry.Bone == 16384 && LoadedModel.Joints.Count > 0) // Root
                {
                    basePosition = Vector3.Transform(new Vector3(collisionEntry.PositionX, collisionEntry.PositionY, collisionEntry.PositionZ), LoadedModel.Joints[0].AbsoluteTransformationMatrix.Value);
                }
                else if (LoadedModel.Joints.Count != 0)
                {
                    basePosition = Vector3.Transform(new Vector3(collisionEntry.PositionX, collisionEntry.PositionY, collisionEntry.PositionZ), LoadedModel.Joints[collisionEntry.Bone].AbsoluteTransformationMatrix.Value);
                }
                basePosition = new Vector3(basePosition.Z, basePosition.X, basePosition.Y);

                foreach(MtShape shape in VpController.FindShapesByName(GetCollisionName(i)))
                {
                    ModelVisual3D shapeVisual = VpController.ShapeVisuals[shape];
                    if (shapeVisual is BoxVisual3D)
                    {
                        ((BoxVisual3D)shapeVisual).Center = new Point3D(basePosition.X, basePosition.Y, basePosition.Z);
                    }
                    else if (shapeVisual is EllipsoidVisual3D)
                    {
                        ((EllipsoidVisual3D)shapeVisual).Center = new Point3D(basePosition.X, basePosition.Y, basePosition.Z);
                    }
                }
            }
        }

        private void CheckFrameShapeVisibility()
        {
            if (!AutoCollisions || MsetService.Instance.LoadedMotion.MotionTriggerFile?.RangeTriggerList == null) {
                return;
            }

            HashSet<int> collisionGroups = new HashSet<int>();
            foreach (MotionTrigger.RangeTrigger trigger in MsetService.Instance.LoadedMotion.MotionTriggerFile.RangeTriggerList)
            {
                if ((trigger.StartFrame <= CurrentFrame || trigger.StartFrame == -1) && (trigger.EndFrame >= CurrentFrame || trigger.EndFrame == -1))
                {
                    // Reaction collision
                    if (trigger.Trigger == 4 && AutoCollisionsOther)
                    {
                        collisionGroups.Add(trigger.Param1);
                    }
                    // Attack 1 hitbox
                    if (trigger.Trigger == 10 && AutoCollisionsAttack)
                    {
                        // Param 1 is atkp entry
                        collisionGroups.Add(trigger.Param2);
                    }
                    // Attack 2 hitboxes
                    if (trigger.Trigger == 33 && AutoCollisionsAttack)
                    {
                        // Param 1 is atkp entry
                        collisionGroups.Add(trigger.Param2);
                    }
                    // Reaction collision - self
                    if ((trigger.Trigger == 20 ||
                        trigger.Trigger == 6) &&
                        AutoCollisionsOther)
                    {
                        // Param 1 is command id
                        collisionGroups.Add(trigger.Param2);
                    }
                }
            }

            List<string> collisionNames = new List<string>();
            for (int i = 0; i < MdlxService.Instance.CollisionFile.EntryList.Count; i++)
            {
                if (collisionGroups.Contains(MdlxService.Instance.CollisionFile.EntryList[i].Group))
                {
                    collisionNames.Add(GetCollisionName(i));
                }
            }

            foreach(MtShape shape in VpController.ShapeVisuals.Keys)
            {
                shape.IsVisible = collisionNames.Contains(shape.Name);
            }
        }

        public MtShape GetCollisionShape(int collisionId)
        {
            if (MdlxService.Instance.CollisionFile == null) {
                throw new System.Exception("Collision file not found");
            }
            
            ObjectCollision collisionEntry = MdlxService.Instance.CollisionFile.EntryList[collisionId];

            // Position
            Vector3 basePosition = Vector3.Zero;
            if (collisionEntry.Bone == 16384 && LoadedModel.Joints.Count > 0) // Root
            {
                basePosition = Vector3.Transform(new Vector3(collisionEntry.PositionX, collisionEntry.PositionY, collisionEntry.PositionZ), LoadedModel.Joints[0].AbsoluteTransformationMatrix.Value);
            }
            else if (LoadedModel.Joints.Count != 0)
            {
                basePosition = Vector3.Transform(new Vector3(collisionEntry.PositionX, collisionEntry.PositionY, collisionEntry.PositionZ), LoadedModel.Joints[collisionEntry.Bone].AbsoluteTransformationMatrix.Value);
            }
            basePosition = new Vector3(basePosition.Z, basePosition.X, basePosition.Y);

            // Color
            Color color = new Color();
            if (collisionEntry.Type == ObjectCollision.TypeEnum.HIT) {
                color = Color.FromArgb(100, 255, 255, 0);
            }
            else if (collisionEntry.Type == ObjectCollision.TypeEnum.ATTACK)
            {
                color = Color.FromArgb(100, 255, 0, 0);
            }
            else if (collisionEntry.Type == ObjectCollision.TypeEnum.REACTION) {
                color = Color.FromArgb(100, 0, 255, 0);
            }
            else if (collisionEntry.Type == ObjectCollision.TypeEnum.TARGET)
            {
                color = Color.FromArgb(100, 0, 0, 255);
            }
            else {
                color = Color.FromArgb(100, 255, 255, 255);
            }

            MtShape collisionShape = new MtShape();
            if(collisionEntry.Shape == ObjectCollision.ShapeEnum.ELLIPSOID) {
                collisionShape = MtShape.CreateEllipsoid(basePosition, collisionEntry.Height, collisionEntry.Radius, collisionEntry.Radius);
            }
            else if (collisionEntry.Shape == ObjectCollision.ShapeEnum.COLUMN) {
                collisionShape = MtShape.CreateColumn(basePosition, collisionEntry.Height, collisionEntry.Radius);
            }
            else if (collisionEntry.Shape == ObjectCollision.ShapeEnum.CUBE) {
                collisionShape = MtShape.CreateBox(basePosition, collisionEntry.Height, collisionEntry.Radius, collisionEntry.Radius);
                collisionShape.Type = MtShape.ShapeType.cube;
            }
            else if (collisionEntry.Shape == ObjectCollision.ShapeEnum.SPHERE) {
                collisionShape = MtShape.CreateEllipsoid(basePosition, collisionEntry.Height, collisionEntry.Radius, collisionEntry.Radius);
            }
            collisionShape.Name = GetCollisionName(collisionId);
            collisionShape.ShapeColor = color;

            collisionShape.IsVisible = false;

            return collisionShape;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // DATA LOADERS
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Load the AnbIndir for current motion (Per motion)
        private void LoadAnbIndir()
        {
            if (!ObjectEditorUtils.isFilePathValid(App_Context.Instance.MdlxPath, "mdlx") ||
                !ObjectEditorUtils.isFilePathValid(MsetService.Instance.MsetPath, "mset") ||
                MsetService.Instance.LoadedMotion == null)
            {
                return;
            }

            Bar anbBarFile = Bar.Read(new MemoryStream(MsetService.Instance.MsetBinarc.Subfiles[MsetService.Instance.MsetBinarc.Entries[MsetService.Instance.LoadedMotionId].Link]));

            _loadedAnb = new AnbIndir(anbBarFile);
        }
        // Load the AnimProvider for current motion (Per frame)
        private void LoadAnimProvider()
        {
            // This is a test, model should be saved in the model module
            MdlxService.Instance.SaveModel();
            Stream mdlxStream = new MemoryStream();
            Bar.Write(mdlxStream, MdlxService.Instance.MdlxBar);

            _animMatricesProvider = _loadedAnb.GetAnimProvider(mdlxStream);
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // FORMATTER
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetCollisionName(int collisionId)
        {
            return "COL_" + collisionId;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        // ANIMATION TICKER
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        public class FpsMode
        {
            public TimeSpan MsBetweenTicks { get; set; }
            public int FrameStep { get; set; }

            public FpsMode(int msBetweenTicks, int frameStep) { MsBetweenTicks = TimeSpan.FromMilliseconds(msBetweenTicks); FrameStep = frameStep; }
        }
        public static FpsMode MODE_30_FPS = new FpsMode(33, 1);
        public static FpsMode MODE_15_FPS = new FpsMode(67, 2);
        public static FpsMode MODE_10_FPS = new FpsMode(100, 3);
        public static FpsMode MODE_5_FPS = new FpsMode(200, 6);
        private FpsMode _currentFpsMode = MODE_15_FPS;
        private PeriodicTimer _timer;
        private CancellationTokenSource _cts;
        public void SetFpsMode(FpsMode mode)
        {
            _currentFpsMode = mode;
            _cts.Cancel();
            StartAnimationTicker();
        }
        async Task StartAnimationTicker()
        {
            TimeSpan timeInMs = _currentFpsMode.MsBetweenTicks;

            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(timeInMs);
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    AnimationTicker();
                }
                catch (Exception e) { }
            }
        }

        public void AnimationTicker()
        {
            if (!AnimationRunning ||
                !ObjectEditorUtils.isFilePathValid(App_Context.Instance.MdlxPath, "mdlx") ||
                !ObjectEditorUtils.isFilePathValid(MsetService.Instance.MsetPath, "mset") ||
                MsetService.Instance.LoadedMotion == null)
            {
                return;
            }

            ViewerService.Instance.FrameIncrease(_currentFpsMode.FrameStep);
            ViewerService.Instance.LoadFrame();
        }
    }
}
