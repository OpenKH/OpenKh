using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using static OpenKh.Kh2.ObjectCollision;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class CollisionProperties_Control : UserControl
    {
        public ObjectCollision? Collision { get; set; }

        public CollisionProperties_Control()
        {
            InitializeComponent();

            _viewBagging.DataContext = new ViewBagging(
                TypeItems: EnumPairingUtils.GetEnumDict<byte, TypeEnum>(),
                ShapeItems: EnumPairingUtils.GetEnumDict<byte, ShapeEnum>()
            );
        }

        public CollisionProperties_Control(ObjectCollision collision) : this()
        {
            this.Collision = collision;
            DataContext = Collision;
        }

        private record ViewBagging(
            KeyValuePair<byte, string>[] TypeItems,
            KeyValuePair<byte, string>[] ShapeItems
        );
    }
}
