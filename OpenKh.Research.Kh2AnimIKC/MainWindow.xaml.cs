using Microsoft.Win32;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Research.Kh2AnimIKC.Extensions;
using OpenKh.Research.Kh2AnimIKC.Models;
using OpenKh.Research.Kh2AnimIKC.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static OpenKh.Kh2.Motion;
using Vector = System.Windows.Vector;

namespace OpenKh.Research.Kh2AnimIKC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int numA = 5;
        private int numB = 5;
        private Matrix4x4[] matricesOut;

        private readonly ObservableCollection<Mdlx.Bone> bone1List = new ObservableCollection<Mdlx.Bone>();
        private readonly ObservableCollection<IKHelperTable> bone2List = new ObservableCollection<IKHelperTable>();
        private readonly ObservableCollection<UnknownTable6> t6List = new ObservableCollection<UnknownTable6>();
        private readonly ObservableCollection<UnknownTable7> t7List = new ObservableCollection<UnknownTable7>();
        private readonly ObservableCollection<UnknownTable8> t8List = new ObservableCollection<UnknownTable8>();
        private readonly ObservableCollection<JointTable> jointsList = new ObservableCollection<JointTable>();
        private readonly ObservableCollection<IKChainTable> ikcList = new ObservableCollection<IKChainTable>();

        public MainWindow()
        {
            InitializeComponent();

            var UseIt = true;

            {
                int x = 0;
                for (; x < numA; x++)
                {
                    bone1List.Add(new Mdlx.Bone
                    {
                        Parent = x - 1,
                        Index = x,

                        ScaleX = 1,
                        ScaleY = 1,
                        ScaleZ = 1,

                        ScaleW = (x == (numA - 4) || x == (numA - 4)) ? 20 : 0,
                        Unk0c = ((x == numA - 100) || (x == numA - 2)) ? 3 : 0,

                        TranslationX = (x != 0) ? 50 : 0
                    });
                }
                for (; x < numA + numB; x++)
                {
                    bone2List.Add(new IKHelperTable
                    {
                        ParentIndex = (x == numA) ? 0 : x - 1,
                        Index = x,

                        ScaleX = 1,
                        ScaleY = 1,
                        ScaleZ = 1,

                        TranslateY = (x != numA) ? 50 : 0
                    });
                }
            }

            {
                for (int x = 0; x < numB; x++)
                {
                    jointsList.Add(new JointTable { JointIndex = (short)(numA + x), Flag = 0 });
                }
                for (int x = 0; x < numA; x++)
                {
                    if (UseIt && x == numA - 4)
                    {
                        jointsList.Add(new JointTable { JointIndex = (short)(x), Flag = 3 });
                    }
                    else if (UseIt && x == numA - 3)
                    {
                        jointsList.Add(new JointTable { JointIndex = (short)(x), Flag = 0x20 });
                    }
                    else if (UseIt && x == numA - 2)
                    {
                        jointsList.Add(new JointTable { JointIndex = (short)(x), Flag = 4 });
                    }
                    else
                    {
                        jointsList.Add(new JointTable { JointIndex = (short)(x), Flag = 0 });
                    }
                }
            }

            ikcList.Add(new IKChainTable { Unk00 = 3, Unk01 = 1, IKHelperIndex = (short)(numA + numB - 1), ModelBoneIndex = (short)(numA - 1), Table8Index = -1 });

            {
                var view = new CollectionViewSource();
                view.Source = bone1List;
                bone1Grid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = bone2List;
                bone2Grid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = jointsList;
                jointsGrid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = ikcList;
                ikcGrid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = t6List;
                t6Grid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = t7List;
                t7Grid.DataContext = view;
            }
            {
                var view = new CollectionViewSource();
                view.Source = t8List;
                t8Grid.DataContext = view;
            }
        }

        private void build_Click(object sender, RoutedEventArgs e)
        {
            var model = Mdlx.CreateModelFromScratch();
            model.SubModels[0].Bones.AddRange(bone1List);

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
            i.IKHelpers.AddRange(bone2List);
            i.Joints.AddRange(jointsList);
            i.IKChains.AddRange(ikcList);
            i.Table6.AddRange(t6List);
            i.Table7.AddRange(t7List);
            i.Table8.AddRange(t8List);

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

                foreach (var bone in bone1List)
                {
                    if (bone.Parent != -1)
                    {
                        var pos0 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.Parent]));
                        var pos1 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.Index]));

                        c.DrawLine(primaryPen, pos0, pos1);
                        c.DrawRectangle(null, boxPen, new Rect(pos1 - new Vector(2, 2), pos1 + new Vector(2, 2)));
                    }
                }
                foreach (var bone in bone2List)
                {
                    if (bone.ParentIndex != -1)
                    {
                        var pos0 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.ParentIndex]));
                        var pos1 = Conv(Vector2.Transform(Vector2.Zero, matricesOut[bone.Index]));

                        c.DrawLine(secondaryPen, pos0, pos1);
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

        private void openModel_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "*.mdlx|*.mdlx",
                ShowReadOnly = true,
                ReadOnlyChecked = true,
            };
            if (!ofd.ShowDialog() ?? false)
            {
                return;
            }
            var mdlxFile = ofd.FileName;
            var msetFile = Path.ChangeExtension(mdlxFile, ".mset");
            if (!File.Exists(msetFile))
            {
                MessageBox.Show($"It excepts that you have also corresponding mset file:\n\n{msetFile}");
                return;
            }
            var mdlxBarEnts = File.OpenRead(mdlxFile).Using(Bar.Read);
            var model = Mdlx.Read(mdlxBarEnts.First(it => it.Type == Bar.EntryType.Model).Stream);

            var msetBarEnts = File.OpenRead(msetFile).Using(Bar.Read);

            motions.Items.Clear();

            foreach (var anbEnt in msetBarEnts.Where(it => it.Type == Bar.EntryType.Anb && it.Stream.Length != 0))
            {
                var anbEnts = Bar.Read(anbEnt.Stream);
                var motion = Motion.Read(anbEnts.Single(it => it.Type == Bar.EntryType.Motion).Stream);

                var item = new MenuItem
                {
                    Header = anbEnt.Name,
                };

                item.Command = new CommandProxy
                {
                    OnExecute = () =>
                    {
                        ResetWith(model, motion);

                        motions.Items.Cast<MenuItem>().ForEach(it => it.IsChecked = ReferenceEquals(it, item));
                    }
                };

                motions.Items.Add(item);
            }

            motions.Items.Cast<MenuItem>().Take(1).ForEach(it => it.Command.Execute(null));
        }

        class CommandProxy : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                OnExecute?.Invoke();
            }

            public Action OnExecute { get; set; }
        }

        void ResetWith(Mdlx model, Motion motion)
        {
            var subModel = model.SubModels.First();

            bone1List.ClearAndAddItems(subModel.Bones);
            numA = bone1List.Count;

            var i = motion.Interpolated;

            bone2List.ClearAndAddItems(i.IKHelpers);
            numB = bone2List.Count;

            jointsList.ClearAndAddItems(i.Joints);
            ikcList.ClearAndAddItems(i.IKChains);
            t6List.ClearAndAddItems(i.Table6);
            t7List.ClearAndAddItems(i.Table7);
            t8List.ClearAndAddItems(i.Table8);
        }
    }
}
