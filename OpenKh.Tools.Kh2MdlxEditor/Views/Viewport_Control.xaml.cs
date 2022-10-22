using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public Point3D LookAtPoint { get; set; }

        public Viewport_Control()
        {
            InitializeComponent();
        }
        public Viewport_Control(List<GeometryModel3D> vpMeshes, PerspectiveCamera? vpCamera = null)
        {
            InitializeComponent();
            Viewport = new Viewport3D();

            if(vpCamera != null)
                VPCamera = vpCamera;
            else
                VPCamera = Viewport3DUtils.getDefaultCamera();

            Viewport.Camera = VPCamera;
            LookAtPoint = new Point3D();

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

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point3D position = VPCamera.Position;
            //Vector3D vector = new Vector3D(position.X, position.Y, position.Z);
            //double length = vector.Length;

            if (e.Delta > 0)
                VPCamera.Position = new Point3D(position.X, position.Y, position.Z *= 0.7);

            else if (e.Delta < 0)
                VPCamera.Position = new Point3D(position.X, position.Y, position.Z *= 1.3);
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
        public void moveCamera(Point pre, Point cur)
        {
            Point3D position = VPCamera.Position;
            double speed = position.Z / 600;

            VPCamera.Position = new Point3D(position.X - (cur.X - pre.X) * speed, position.Y + (cur.Y - pre.Y) * speed, position.Z);
            //VPCamera.LookDirection = Viewport3DUtils.getVectorToTarget(VPCamera.Position, LookAtPoint);
        }
        public void rotateCamera(Point pre, Point cur)
        {
            Point3D position = VPCamera.Position;
            double speed = 1;

            VPCamera.Position = new Point3D(position.X - (cur.X - pre.X) * speed, position.Y + (cur.Y - pre.Y) * speed, position.Z);
        }

        private void rotateCamera(double rX, double rY, double rZ)
        {
            Vector3D vector = new Vector3D(VPCamera.Position.X, VPCamera.Position.Y, VPCamera.Position.Z);

            double length = vector.Length;
            double theta = Math.Acos(vector.Y / length);
            double phi = Math.Atan2(-vector.Z, vector.X);

            theta -= rY * 0.01;
            phi -= rX * 0.01;
            length *= 1.0 - 0.1 * rZ;

            theta = Math.Clamp(theta, 0.0001, Math.PI - 0 - 0001);

            vector.X = length * Math.Sin(theta) * Math.Cos(phi);
            vector.Z = -length * Math.Sin(theta) * Math.Sin(phi);
            vector.Y = length * Math.Cos(theta);

            VPCamera.Position = new Point3D(vector.X, vector.Y, vector.Z);
            VPCamera.LookDirection = Viewport3DUtils.getVectorToTarget(VPCamera.Position);
        }
    }
}
