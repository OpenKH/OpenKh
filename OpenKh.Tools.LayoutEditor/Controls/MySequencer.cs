using ImGuiNET;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.LayoutEditor.Controls
{
    class MySequencer : ImSequencer.SequenceInterface
    {
        static uint[] ColorType = new uint[]
        {
            0xfff09124, 0xffdebd00, 0xff85cf44, 0xff4cc25f,
            0xff35ab84, 0xff25b3b8, 0xff4c9ef5, 0xff8282f5,
            0xffa874e8, 0xffdc57eb, 0xffeb5bad,
            0xff2491f0, 0xff00bdde, 0xff44cf85, 0xff5fc24c,
            0xff84ab35, 0xffb8b325, 0xfff59e4c, 0xfff58282,
            0xffe874a8, 0xffeb57dc, 0xffad5beb,
        };

        private class MySequenceItem : ImSequencer.IAnimation
        {
            private readonly Sequence.Animation _animation;

            public int FrameStart { get => _animation.FrameStart; set => _animation.FrameStart = value; }
            public int FrameEnd { get => _animation.FrameEnd; set => _animation.FrameEnd = value; }
            public bool IsExpanded { get; set; }
            public string Name { get; set; }
            public uint Color => ColorType[Type % ColorType.Length];
            public int Height => IsExpanded ? 300 : 0;
            public int Type { get; set; }

            public MySequenceItem() : this(new Sequence.Animation())
            { }

            public MySequenceItem(Sequence.Animation animation)
            {
                _animation = animation;
            }
        };

        private readonly Sequence _sequence;
        private int _selectedAnimationGroup;

        public int SelectedAnimationGroup
        {
            get => _selectedAnimationGroup;
            set
            {
                _selectedAnimationGroup = value;
                var animationGroup = _sequence.AnimationGroups[_selectedAnimationGroup];
                myItems = animationGroup.Animations
                    .Select((x, index) => new MySequenceItem(x)
                    {
                        Type = index
                    })
                    .ToList();
            }
        }
        public int FrameMin { get; set; }
        public int FrameMax { get; set; }
        public int ItemCount => myItems.Count;

        public int GetItemTypeCount() => ColorType.Length;
        public string GetItemTypeName(int typeIndex) => $"type {typeIndex}";

        public ImSequencer.IAnimation GetAnimation(int index)
        {
            if (index >= 0 && index < myItems.Count)
                return myItems[index];

            return null;
        }

        public void Add(int type) => myItems.Add(new MySequenceItem
        {
            Type = new Random().Next(),
            FrameStart = 0,
            FrameEnd = 10,
            IsExpanded = false
        });

        public void Remove(int index) => myItems.RemoveAt(index);

        public void Duplicate(int index) => myItems.Add(myItems[index]);

        public int GetCustomHeight(int index) => myItems[index].Height;

        public MySequencer(Sequence sequence)
        {
            _sequence = sequence;

            FrameMin = 0;
            FrameMax = 500;
            myItems = new List<MySequenceItem>();
        }

        List<MySequenceItem> myItems;

        public bool focused { get; set; }

        //RampEdit rampEdit;

        public void DoubleClick(int index)
        {
            if (myItems[index].IsExpanded)
            {
                myItems[index].IsExpanded = false;
                return;
            }
            foreach (var item in myItems)
                item.IsExpanded = false;
            myItems[index].IsExpanded = !myItems[index].IsExpanded;
        }

        public void CustomDraw(int index, ImDrawListPtr draw_list, ImRect rc, ImRect legendRect, ImRect clippingRect, ImRect legendClippingRect)
        {
            var labels = new string[] { "Translation", "Rotation", "Scale" };

            draw_list.PushClipRect(legendClippingRect.Min, legendClippingRect.Max, true);
            //rampEdit.mMax = new Vector2(mFrameMax, 1f);
            //rampEdit.mMin = new Vector2(mFrameMin, 0f);
            //for (int i = 0; i < 3; i++)
            //{
            //    var pta = new Vector2(legendRect.Min.X + 30, legendRect.Min.Y + i * 14f);
            //    var ptb = new Vector2(legendRect.Max.X, legendRect.Min.Y + (i + 1) * 14f);
            //    draw_list.AddText(pta, rampEdit.mbVisible[i] ? 0xFFFFFFFF : 0x80FFFFFF, labels[i]);
            //    if (new ImRect(pta, ptb).Contains(ImGui.GetMousePos()) && ImGui.IsMouseClicked(0))
            //        rampEdit.mbVisible[i] = !rampEdit.mbVisible[i];
            //}
            draw_list.PopClipRect();

            ImGui.SetCursorScreenPos(rc.Min);
            //ImCurveEdit.Edit(rampEdit, rc.Max - rc.Min, 137 + index, clippingRect);
        }

        public void CustomDrawCompact(int index, ImDrawListPtr draw_list, ImRect rc, ImRect clippingRect)
        {
            //rampEdit.mMax = new Vector2(mFrameMax, 1f);
            //rampEdit.mMin = new Vector2(mFrameMin, 0f);
            draw_list.PushClipRect(clippingRect.Min, clippingRect.Max, true);
            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < rampEdit.mPointCount[i]; j++)
            //    {
            //        float p = rampEdit.mPts[i][j].X;
            //        if (p < myItems[index].mFrameStart || p > myItems[index].mFrameEnd)
            //            continue;
            //        float r = (p - mFrameMin) / (float)(mFrameMax - mFrameMin);
            //        float x = ImSequencer.Lerp(rc.Min.X, rc.Max.X, r);
            //        draw_list.AddLine(new Vector2(x, rc.Min.Y + 6), new Vector2(x, rc.Max.Y - 4), 0xAA000000, 4.f);
            //    }
            //}
            draw_list.PopClipRect();
        }

        public void BeginEdit(int index)
        {
        }

        public void EndEdit()
        {
        }

        public void Copy()
        {
        }

        public void Paste()
        {
        }
    };
}
