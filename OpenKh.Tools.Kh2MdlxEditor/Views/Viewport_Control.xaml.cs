using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Viewport_Control : UserControl
    {
        static System.Windows.Point L_previousPosition = new System.Windows.Point();
        static System.Windows.Point L_currentPosition = new System.Windows.Point();
        static System.Windows.Point R_previousPosition = new System.Windows.Point();
        static System.Windows.Point R_currentPosition = new System.Windows.Point();

        public Viewport3D Viewport { get; set; }
        public PerspectiveCamera VPCamera { get; set; }
        public List<GeometryModel3D> VPMeshes { get; set; }
        public Point3D AnchorPoint { get; set; }
        public Point3D AnchorPointTemp { get; set; }
        public Vector3D AnchorPointHorVec { get; set; }
        public Vector3D AnchorPointVerVec { get; set; }
        public bool AnchorPointLocked { get; set; }

        // CONSTRUCTOR
        public Viewport_Control()
        {
            InitializeComponent();
        }
        public Viewport_Control(List<GeometryModel3D> vpMeshes, PerspectiveCamera? vpCamera = null)
        {
            InitializeComponent();
            Viewport = new Viewport3D();
            Rect3D boundingBox = getBoundingBox(vpMeshes);

            if (vpCamera != null)
                VPCamera = vpCamera;
            else
            {
                VPCamera = Viewport3DUtils.getCameraByBoundingBox(boundingBox);
            }

            Viewport.Camera = VPCamera;
            AnchorPoint = new Point3D();
            AnchorPointTemp = new Point3D();
            AnchorPointLocked = false;

            Model3DGroup myModel3DGroup = new Model3DGroup();

            myModel3DGroup.Children.Add(new AmbientLight(System.Windows.Media.Brushes.White.Color));

            VPMeshes = vpMeshes;

            foreach (GeometryModel3D mesh in VPMeshes)
            {
                myModel3DGroup.Children.Add(mesh);
            }

            ModelVisual3D myModelVisual3D = new ModelVisual3D();
            myModelVisual3D.Content = myModel3DGroup;

            Viewport.Children.Add(myModelVisual3D);

            viewportFrame.Content = Viewport;
        }

        // ACTIONS
        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = 0.3;
            Point3D position = VPCamera.Position;
            Vector3D lookVector = Viewport3DUtils.getVectorToTarget(VPCamera.Position, AnchorPoint);
            double length = lookVector.Length;
            lookVector.Normalize();

            if (e.Delta > 0)
                lookVector *= length * scale;

            else if (e.Delta < 0)
                lookVector *= length * (-scale);

            VPCamera.Position = new Point3D(position.X + lookVector.X, position.Y + lookVector.Y, position.Z + lookVector.Z);
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                R_currentPosition = e.GetPosition(viewportFrame);

                if (R_previousPosition != R_currentPosition)
                    moveCamera(R_previousPosition, R_currentPosition);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                L_currentPosition = e.GetPosition(viewportFrame);

                if (L_previousPosition != L_currentPosition)
                    rotateCamera((L_currentPosition.X - L_previousPosition.X), (L_currentPosition.Y - L_previousPosition.Y), 0);
            }

            R_previousPosition = e.GetPosition(viewportFrame);
            L_previousPosition = e.GetPosition(viewportFrame);
        }

        private void Viewport_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            AnchorPointLocked = true;
            AnchorPointTemp = new Point3D(AnchorPoint.X, AnchorPoint.Y, AnchorPoint.Z);

            Point3D position = VPCamera.Position;
            AnchorPointHorVec = getHorizontalPerpendicularVector(new Vector3D(position.X - AnchorPoint.X, position.Y - AnchorPoint.Y, position.Z - AnchorPoint.Z));
            AnchorPointVerVec = getVerticalPerpendicularVector(new Vector3D(position.X - AnchorPoint.X, position.Y - AnchorPoint.Y, position.Z - AnchorPoint.Z), AnchorPointHorVec);
        }
        private void Viewport_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            AnchorPointLocked = false;
            AnchorPoint = new Point3D(AnchorPointTemp.X, AnchorPointTemp.Y, AnchorPointTemp.Z);
        }

        // FUNCTIONS
        public void moveCamera(Point pre, Point cur)
        {
            Point3D position = VPCamera.Position;
            Vector3D positionVector = new Vector3D(VPCamera.Position.X, VPCamera.Position.Y, VPCamera.Position.Z);
            double speed = positionVector.Length / 600;

            Vector3D moveHorVec = AnchorPointHorVec * (cur.X - pre.X) * speed;
            Vector3D moveVerVec = AnchorPointVerVec * (cur.Y - pre.Y) * speed;


            VPCamera.Position = new Point3D(position.X - moveHorVec.X, position.Y + moveVerVec.Y, position.Z - moveHorVec.Z);
            AnchorPointTemp = new Point3D(AnchorPointTemp.X - moveHorVec.X, AnchorPointTemp.Y + moveVerVec.Y, AnchorPointTemp.Z - moveHorVec.Z);
        }
        public void rotateCamera(Point pre, Point cur)
        {
            Point3D position = VPCamera.Position;
            double speed = 1;

            VPCamera.Position = new Point3D(position.X - (cur.X - pre.X) * speed, position.Y + (cur.Y - pre.Y) * speed, position.Z);
        }

        private void rotateCamera(double rX, double rY, double rZ)
        {
            Vector3D vector = new Vector3D(VPCamera.Position.X - AnchorPoint.X, VPCamera.Position.Y - AnchorPoint.Y, VPCamera.Position.Z - AnchorPoint.Z);

            double length = vector.Length;
            double theta = Math.Acos(vector.Y / length); // Vertical angle
            double phi = Math.Atan2(-vector.Z, vector.X); // Horizontal angle

            theta -= rY * 0.01;
            phi -= rX * 0.01;
            length *= 1.0 - 0.1 * rZ;

            theta = Math.Clamp(theta, 0.0001, Math.PI - 0.0001);

            vector.X = length * Math.Sin(theta) * Math.Cos(phi);
            vector.Z = -length * Math.Sin(theta) * Math.Sin(phi);
            vector.Y = length * Math.Cos(theta);

            VPCamera.Position = new Point3D(AnchorPoint.X + vector.X, AnchorPoint.Y + vector.Y, AnchorPoint.Z + vector.Z);
            VPCamera.LookDirection = Viewport3DUtils.getVectorToTarget(VPCamera.Position, AnchorPoint);
        }

        // Returns the horizontal perpendicular vector of length 1
        private Vector3D getHorizontalPerpendicularVector(Vector3D vector)
        {
            // Counterclockwise
            Vector3D perpendicularVector = new Vector3D(vector.Z, 0, -vector.X);
            if (perpendicularVector.X != 0 || perpendicularVector.Y != 0 || perpendicularVector.Z != 0)
                perpendicularVector.Normalize();

            return perpendicularVector;
        }
        // Returns the horizontal perpendicular vector of length 1
        private Vector3D getVerticalPerpendicularVector(Vector3D cameraVector, Vector3D horizontalVector)
        {
            // Upwards
            Vector3D perpendicularVector = Vector3D.CrossProduct(cameraVector, horizontalVector);
            if(perpendicularVector.X != 0 || perpendicularVector.Y != 0 || perpendicularVector.Z != 0)
                perpendicularVector.Normalize();

            return perpendicularVector;
        }

        private Rect3D getBoundingBox(List<GeometryModel3D> vpMeshes)
        {
            Rect3D boundingBox = new Rect3D();
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0;
            float minZ = 0;
            float maxZ = 0;
            foreach (GeometryModel3D mesh in vpMeshes)
            {
                float localMinX = (float)(mesh.Geometry.Bounds.Location.X - mesh.Geometry.Bounds.SizeX);
                float localMaxX = (float)(mesh.Geometry.Bounds.Location.X + mesh.Geometry.Bounds.SizeX);
                float localMinY = (float)(mesh.Geometry.Bounds.Location.Y - mesh.Geometry.Bounds.SizeY);
                float localMaxY = (float)(mesh.Geometry.Bounds.Location.Y + mesh.Geometry.Bounds.SizeY);
                float localMinZ = (float)(mesh.Geometry.Bounds.Location.Z - mesh.Geometry.Bounds.SizeZ);
                float localMaxZ = (float)(mesh.Geometry.Bounds.Location.Z + mesh.Geometry.Bounds.SizeZ);

                if (localMinX < minX)
                    minX = localMinX;
                if (localMaxX > maxX)
                    maxX = localMaxX;
                if (localMinY < minY)
                    minY = localMinY;
                if (localMaxY > maxY)
                    maxY = localMaxY;
                if (localMinZ < minZ)
                    minZ = localMinZ;
                if (localMaxZ > maxZ)
                    maxZ = localMaxZ;
            }

            boundingBox.SizeX = Math.Abs(maxX - minX);
            boundingBox.SizeY = Math.Abs(maxY - minY);
            boundingBox.SizeZ = Math.Abs(maxZ - minZ);

            double X = minX + (boundingBox.SizeX / 2);
            double Y = minY + (boundingBox.SizeY / 2);
            double Z = minZ + (boundingBox.SizeZ / 2);

            boundingBox.Location = new Point3D(X, Y, Z);

            return boundingBox;
        }
    }
}
