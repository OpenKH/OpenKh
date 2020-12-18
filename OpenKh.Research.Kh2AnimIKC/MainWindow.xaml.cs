using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Research.Kh2AnimIKC.Models;
using OpenKh.Research.Kh2AnimIKC.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vector = System.Windows.Vector;

namespace OpenKh.Research.Kh2AnimIKC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BoneDef> boneDefs = new List<BoneDef>();
        private int numA = 5;
        private int numB = 5;
        private Matrix4x4[] matricesOut;

        public MainWindow()
        {
            InitializeComponent();

            var UseIt = true;

            {
                int x = 0;
                for (; x < numA; x++)
                {
                    boneDefs.Add(new BoneDef
                    {
                        Display = $"A{x}",
                        Parent = x - 1,
                        Idx = x,

                        Sw = (x == (numA - 4) || x == (numA - 4)) ? 20 : 0,
                        Unk3 = ((x == numA - 100) || (x == numA - 2)) ? 3 : 0,

                        Tx = (x != 0) ? 50 : 0
                    });
                }
                for (; x < numA + numB; x++)
                {
                    boneDefs.Add(new BoneDef
                    {
                        Display = $"B{x}",
                        Parent = (x == numA) ? 0 : x - 1,
                        Idx = x,

                        Ty = (x != numA) ? 50 : 0
                    });
                }
            }

            {
                var writer = new StringWriter();

                for (int x = 0; x < numB; x++)
                {
                    writer.WriteLine($"{numA + x,3} 00");
                }
                for (int x = 0; x < numA; x++)
                {
                    if (UseIt && x == numA - 4)
                    {
                        writer.WriteLine($"{x,3} 03");
                    }
                    else if (UseIt && x == numA - 3)
                    {
                        writer.WriteLine($"{x,3} 20");
                    }
                    else if (UseIt && x == numA - 2)
                    {
                        writer.WriteLine($"{x,3} 04");
                    }
                    else
                    {
                        writer.WriteLine($"{x,3} 00");
                    }
                }

                jointsText.Text = writer.ToString();
            }

            {
                var writer = new StringWriter();

                writer.WriteLine($"{numA + numB - 1,3} {numA - 1,3} 03 01 -1 0");

                ikcText.Text = writer.ToString();
            }

            {
                var view = new CollectionViewSource();
                view.Source = boneDefs;
                boneGrid.DataContext = view;
            }
        }

        private void build_Click(object sender, RoutedEventArgs e)
        {
            var model = Mdlx.CreateModelFromScratch();
            model.SubModels[0].Bones.AddRange(
                boneDefs
                    .Take(numA)
                    .Select(
                        it => new Mdlx.Bone
                        {
                            Index = it.Idx,
                            Parent = it.Parent,
                            Unk08 = it.Unk2,
                            Unk0c = it.Unk3,
                            ScaleX = it.Sx,
                            ScaleY = it.Sy,
                            ScaleZ = it.Sz,
                            ScaleW = it.Sw,
                            RotationX = it.Rx,
                            RotationY = it.Ry,
                            RotationZ = it.Rz,
                            RotationW = it.Rw,
                            TranslationX = it.Tx,
                            TranslationY = it.Ty,
                            TranslationZ = it.Tz,
                            TranslationW = it.Tw,
                        }
                    )
            );

            var mot = Motion.CreateInterpolatedFromScratch();
            var i = mot.Interpolated;
            i.Footer = new Motion.FooterTable
            {
                Unknown = new int[9],
            };
            i.BoneCount = (short)numA;
            i.FrameEnd = 1;
            i.FramePerSecond = 30;
            i.TotalFrameCount = 1;
            i.IKHelpers.AddRange(
                boneDefs
                    .Skip(numA)
                    .Select(
                        it => new Motion.IKHelperTable
                        {
                            Index = it.Idx,
                            ParentIndex = it.Parent,
                            Unk08 = it.Unk2,
                            Unk0c = it.Unk3,
                            ScaleX = it.Sx,
                            ScaleY = it.Sy,
                            ScaleZ = it.Sz,
                            ScaleW = it.Sw,
                            RotateX = it.Rx,
                            RotateY = it.Ry,
                            RotateZ = it.Rz,
                            RotateW = it.Rw,
                            TranslateX = it.Tx,
                            TranslateY = it.Ty,
                            TranslateZ = it.Tz,
                            TranslateW = it.Tw,
                        }
                    )
            );
            i.Joints.AddRange(
                jointsText.Text.Split('\n')
                    .Select(text => text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .Where(cells => cells.Length == 2)
                    .Select(
                        cells => new Motion.JointTable
                        {
                            JointIndex = short.Parse(cells[0]),
                            Flag = Convert.ToInt16(cells[1], 16),
                        }
                    )
            );
            i.IKChains.AddRange(
                ikcText.Text.Split('\n')
                    .Select(text => text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    .Where(cells => cells.Length == 6)
                    .Select(
                        cells => new Motion.IKChainTable
                        {
                            IKHelperIndex = short.Parse(cells[0]),
                            ModelBoneIndex = short.Parse(cells[1]),
                            Unk00 = Convert.ToByte(cells[2], 16),
                            Unk01 = Convert.ToByte(cells[3], 16),
                            Table8Index = short.Parse(cells[4]),
                            Unk08 = short.Parse(cells[5]),
                        }
                    )
            );

            var modelStream = new MemoryStream();
            model.Write(modelStream);
            modelStream.Position = 0;

            var mdlxStream = new MemoryStream();

            Bar.Write(
                mdlxStream,
                new Bar.Entry[]
                {
                    new Bar.Entry
                    {
                        Name = "A000",
                        Stream = modelStream,
                        Type = Bar.EntryType.Model,
                    },
                }
            );

            mdlxStream.Position = 0;

            File.WriteAllBytes("temp.mdlx", mdlxStream.ToArray());

            var anbStream = new MemoryStream();

            var motionStream = new MemoryStream();

            Motion.Write(motionStream, mot);

            var anbBarEntries = new Bar.Entry[]
            {
                new Bar.Entry
                {
                    Name = "A000",
                    Stream = motionStream,
                    Type = Bar.EntryType.Motion,
                },
            };

            Bar.Write(
                anbStream, anbBarEntries
            );

            anbStream.Position = 0;

            File.WriteAllBytes("temp.anb", anbStream.ToArray());

            var anbIndir = new AnbIndir(anbBarEntries);

            try
            {
                var provider = anbIndir.GetAnimProvider(
                    mdlxStream
                );

                matricesOut = provider.ProvideMatrices(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Render();
        }

        void Render()
        {
            upperView.Children.Clear();

            if (matricesOut == null)
            {
                return;
            }

            var offX = upperView.ActualWidth / 2;
            var offY = upperView.ActualHeight / 2;

            Point Conv(Vector2 v) => new Point(offX + v.X, offY + v.Y);

            var primaryPen = new Pen(Brushes.Lime, 1.5);
            var secondaryPen = new Pen(Brushes.Purple, 1.5);
            var boxPen = new Pen(Brushes.Blue, 1.5);

            {
                var visual = new DrawingVisual();
                var c = visual.RenderOpen();

                foreach (var bone in boneDefs)
                {
                    if (bone.Parent != -1)
                    {
                        var pos0 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.Parent]));
                        var pos1 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.Idx]));

                        c.DrawLine((bone.Idx < numA) ? primaryPen : secondaryPen, pos0, pos1);
                        c.DrawRectangle(null, boxPen, new Rect(pos1 - new Vector(2, 2), pos1 + new Vector(2, 2)));
                    }
                }

                c.Close();
                upperView.Children.Add(new VisualHost { Visual = visual, });
            }
        }

        private void upperView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Render();
        }

        private void boneGrid_KeyDown(object sender, KeyEventArgs e)
        {
            return;
            int delta = (e.Key == Key.F3) ? 1 : (e.Key == Key.F4) ? -1 : 0;
            if (delta != 0)
            {
                var item = boneGrid.CurrentItem;
                var col = boneGrid.CurrentColumn;
                if (item != null && col != null)
                {
                    var type = item.GetType();
                    var propInfo = type.GetProperty(col.Header + "");
                    if (propInfo != null && propInfo.PropertyType == typeof(float))
                    {
                        float val = (float)propInfo.GetValue(item);
                        propInfo.SetValue(item, val + delta * 0.1f);
                    }
                }
                e.Handled = true;
            }
        }
    }
}
