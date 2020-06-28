// The MIT License(MIT)
// 
// Copyright(c) 2016 Cedric Guillemet
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// This is a porting and modified version of ImSequencer from ImGuizmo repository.
// Please refer to https://github.com/CedricGuillemet/ImGuizmo for the original
// source code.

using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenKh.Tools.LayoutEditor.Controls
{
    public class ImRect
    {
        public Vector2 Min { get; }
        public Vector2 Max { get; }

        public ImRect()
        {

        }

        public ImRect(Vector2 a, Vector2 b)
        {
            Min = a;
            Max = b;
        }

        public bool Contains(Vector2 p) =>
            p.X >= Min.X && p.Y >= Min.Y && p.X < Max.X && p.Y < Max.Y;
    }

    class ImSequencer
    {
        [Flags]
        public enum SEQUENCER_OPTIONS
        {
            SEQUENCER_EDIT_NONE = 0,
            SEQUENCER_EDIT_STARTEND = 1 << 1,
            SEQUENCER_CHANGE_FRAME = 1 << 3,
            SEQUENCER_ADD = 1 << 4,
            SEQUENCER_DEL = 1 << 5,
            SEQUENCER_COPYPASTE = 1 << 6,
            SEQUENCER_EDIT_ALL = SEQUENCER_EDIT_STARTEND | SEQUENCER_CHANGE_FRAME
        };

        public interface IAnimation
        {
            int FrameStart { get; set; }
            int FrameEnd { get; set; }
            bool IsExpanded { get; set; }
            string Name { get; }
            uint Color { get; }
        }

        public interface SequenceInterface
        {
            bool focused { get; set; }
            int FrameMin { get; }
            int FrameMax { get;  }
            int ItemCount { get; }

            void BeginEdit(int index);
            void EndEdit();
            int GetItemTypeCount();
            string GetItemTypeName(int typeIndex);

            IAnimation GetAnimation(int index);
            void Add(int type);
            void Del(int index);
            void Duplicate(int index);

            void Copy();
            void Paste();

            int GetCustomHeight(int index);
            void DoubleClick(int index);
            void CustomDraw(int index, ImDrawListPtr draw_list, ImRect rc, ImRect legendRect, ImRect clippingRect, ImRect legendClippingRect);
            void CustomDrawCompact(int index, ImDrawListPtr draw_list, ImRect rc, ImRect clippingRect);
        }

        private class CustomDraw
        {
            public int index { get; set; }
            public ImRect customRect { get; set; }
            public ImRect legendRect { get; set; }
            public ImRect clippingRect { get; set; }
            public ImRect legendClippingRect { get; set; }
        };

        [Flags]
        private enum AnimationBarPart
        {
            None,
            SelectionLeft,
            SelectionRight,
            Bar = SelectionLeft | SelectionRight
        }

        const float AnimationBarSideSelectionWidth = 8f;
        static float framePixelWidth = 10f;
        static float framePixelWidthTarget = 10f;
        static int movingEntry = -1;
        static int movingPos = -1;
        static AnimationBarPart movingPart = AnimationBarPart.None;
        const int scrollBarHeight = 14;
        static bool MovingScrollBar = false;
        static bool MovingCurrentFrame = false;
        static bool panningView = false;
        static Vector2 panningViewSource;
        static int panningViewFrame;
        static bool sizingRBar = false;
        static bool sizingLBar = false;
        private const float MinBarWidth = 44f;
        private const float FLT_EPSILON = 1.192092896e-07F;

        public static float Lerp(float min, float max, float t) => min * (1 - t) + max * t;

        private static bool SequencerAddDelButton(ImDrawListPtr draw_list, Vector2 pos, bool add = true)
        {
            var io = ImGui.GetIO();
            var delRect = new ImRect(pos, new Vector2(pos.X + 16, pos.Y + 16));
            var overDel = delRect.Contains(io.MousePos);
            var delColor = overDel ? 0xFFAAAAAAu : 0x50000000u;
            var midy = pos.Y + 16 / 2 - 0.5f;
            var midx = pos.X + 16 / 2 - 0.5f;

            draw_list.AddRect(delRect.Min, delRect.Max, delColor, 4);
            draw_list.AddLine(new Vector2(delRect.Min.X + 3, midy), new Vector2(delRect.Max.X - 3, midy), delColor, 2);
            if (add)
                draw_list.AddLine(new Vector2(midx, delRect.Min.Y + 3), new Vector2(midx, delRect.Max.Y - 3), delColor, 2);

            return overDel;
        }

        public static bool Sequencer(SequenceInterface sequence, ref int currentFrame, ref bool expanded, ref int selectedEntry, ref int firstFrame, SEQUENCER_OPTIONS sequenceOptions)
        {
            var ret = false;
            var io = ImGui.GetIO();
            var cx = (int)(io.MousePos.X);
            var cy = (int)(io.MousePos.Y);
            var legendWidth = 100;

            var delEntry = -1;
            var dupEntry = -1;
            var ItemHeight = 20;

            var popupOpened = false;
            var sequenceCount = sequence.ItemCount;
            if (sequenceCount == 0)
                return false;
            ImGui.BeginGroup();

            var draw_list = ImGui.GetWindowDrawList();
            var canvas_pos = ImGui.GetCursorScreenPos();            // ImDrawList API uses screen coordinates!
            var canvas_size = ImGui.GetContentRegionAvail();        // Resize canvas to what's available
            int firstFrameUsed = firstFrame;

            int controlHeight = sequenceCount * ItemHeight;
            for (int i = 0; i < sequenceCount; i++)
                controlHeight += sequence.GetCustomHeight(i);
            int frameCount = Math.Max(sequence.FrameMax - sequence.FrameMin, 1);

            // ImVector<CustomDraw> customDraws;
            // ImVector<CustomDraw> compactCustomDraws;
            var customDraws = new List<CustomDraw>();
            var compactCustomDraws = new List<CustomDraw>();
            // zoom in/out
            int frameOverCursor = 0;
            int visibleFrameCount = (int)Math.Floor((canvas_size.X - legendWidth) / framePixelWidth);
            float barWidthRatio = Math.Min(visibleFrameCount / (float)frameCount, 1f);
            float barWidthInPixels = barWidthRatio * (canvas_size.X - legendWidth);

            var regionRect = new ImRect(canvas_pos, canvas_pos + canvas_size);

            if (ImGui.IsWindowFocused() && io.KeyAlt && io.MouseDown[2])
            {
                if (!panningView)
                {
                    panningViewSource = io.MousePos;
                    panningView = true;
                    panningViewFrame = firstFrame;
                }
                firstFrame = panningViewFrame - (int)((io.MousePos.X - panningViewSource.X) / framePixelWidth);
                firstFrame = Math.Clamp(firstFrame, sequence.FrameMin, sequence.FrameMax - visibleFrameCount);
            }
            if (panningView && !io.MouseDown[2])
            {
                panningView = false;
            }
            framePixelWidthTarget = Math.Clamp(framePixelWidthTarget, 0.1f, 50f);

            framePixelWidth = Lerp(framePixelWidth, framePixelWidthTarget, 0.33f);

            frameCount = sequence.FrameMax - sequence.FrameMin;
            if (visibleFrameCount >= frameCount)
                firstFrame = sequence.FrameMin;


            // --
            if (!expanded)
            {
                ImGui.InvisibleButton("canvas", new Vector2(canvas_size.X - canvas_pos.X, (float)ItemHeight));
                draw_list.AddRectFilled(canvas_pos, new Vector2(canvas_size.X + canvas_pos.X, canvas_pos.Y + ItemHeight), 0xFF3D3837, 0);

                var tmps = $"{frameCount} Frames / {sequenceCount} entries";
                draw_list.AddText(new Vector2(canvas_pos.X + 26, canvas_pos.Y + 2), 0xFFFFFFFF, tmps);
            }
            else
            {
                bool hasScrollBar = true;
                /*
                int framesPixelWidth = int(frameCount * framePixelWidth);
                if ((framesPixelWidth + legendWidth) >= canvas_size.X)
                {
                    hasScrollBar = true;
                }
                */
                // test scroll area
                var headerSize = new Vector2(canvas_size.X, (float)ItemHeight);
                var scrollBarSize = new Vector2(canvas_size.X, 14f);
                ImGui.InvisibleButton("topBar", headerSize);
                draw_list.AddRectFilled(canvas_pos, canvas_pos + headerSize, 0xFFFF0000, 0);
                Vector2 childFramePos = ImGui.GetCursorScreenPos();
                var childFrameSize = new Vector2(canvas_size.X, canvas_size.Y - 8f - headerSize.Y - (hasScrollBar ? scrollBarSize.Y : 0));
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                ImGui.BeginChildFrame(889, childFrameSize);
                sequence.focused = ImGui.IsWindowFocused();
                ImGui.InvisibleButton("contentBar", new Vector2(canvas_size.X, (float)(controlHeight)));
                Vector2 contentMin = ImGui.GetItemRectMin();
                Vector2 contentMax = ImGui.GetItemRectMax();
                var contentRect = new ImRect(contentMin, contentMax);
                float contentHeight = contentMax.Y - contentMin.Y;

                // full background
                draw_list.AddRectFilled(canvas_pos, canvas_pos + canvas_size, 0xFF242424, 0);

                // current frame top
                var topRect = new ImRect(new Vector2(canvas_pos.X + legendWidth, canvas_pos.Y), new Vector2(canvas_pos.X + canvas_size.X, canvas_pos.Y + ItemHeight));

                if (!MovingCurrentFrame && !MovingScrollBar && movingEntry == -1 && sequenceOptions.HasFlag(SEQUENCER_OPTIONS.SEQUENCER_CHANGE_FRAME) && currentFrame >= 0 && topRect.Contains(io.MousePos) && io.MouseDown[0])
                {
                    MovingCurrentFrame = true;
                }
                if (MovingCurrentFrame)
                {
                    if (true)
                    {
                        currentFrame = (int)((io.MousePos.X - topRect.Min.X) / framePixelWidth) + firstFrameUsed;
                        if (currentFrame < sequence.FrameMin)
                            currentFrame = sequence.FrameMin;
                        if (currentFrame >= sequence.FrameMax)
                            currentFrame = sequence.FrameMax;
                    }
                    if (!io.MouseDown[0])
                        MovingCurrentFrame = false;
                }

                //header
                draw_list.AddRectFilled(canvas_pos, new Vector2(canvas_size.X + canvas_pos.X, canvas_pos.Y + ItemHeight), 0xFF3D3837, 0);
                if (sequenceOptions.HasFlag(SEQUENCER_OPTIONS.SEQUENCER_ADD))
                {
                    if (SequencerAddDelButton(draw_list, new Vector2(canvas_pos.X + legendWidth - ItemHeight, canvas_pos.Y + 2), true) && io.MouseReleased[0])
                        ImGui.OpenPopup("addEntry");

                    if (ImGui.BeginPopup("addEntry"))
                    {
                        for (int i = 0; i < sequence.GetItemTypeCount(); i++)
                            if (ImGui.Selectable(sequence.GetItemTypeName(i)))
                            {
                                sequence.Add(i);
                                selectedEntry = sequence.ItemCount - 1;
                            }

                        ImGui.EndPopup();
                        popupOpened = true;
                    }
                }

                //header frame number and lines
                const int MinimumDistanceBetweenElements = 100;
                int modFrameCount = 5; // after how many frames should print the frame number
                int frameStep = 1;
                while ((modFrameCount * framePixelWidth) < MinimumDistanceBetweenElements)
                {
                    modFrameCount *= 2;
                    frameStep *= 2;
                };

                int halfModFrameCount = modFrameCount / 2;

                Action<int, int> drawLine = (int i, int regionHeight) => {
                    const uint TextColor = 0xFFBBBBBB;
                    
                    bool baseIndex = ((i % modFrameCount) == 0) || (i == sequence.FrameMax || i == sequence.FrameMin);
                    bool halfIndex = (i % halfModFrameCount) == 0;
                    int px = (int)canvas_pos.X + (int)(i * framePixelWidth) + legendWidth - (int)(firstFrameUsed * framePixelWidth);
                    int tiretStart = baseIndex ? 4 : (halfIndex ? 10 : 14);
                    int tiretEnd = baseIndex ? regionHeight : ItemHeight;

                    if (px <= (canvas_size.X + canvas_pos.X) && px >= (canvas_pos.X + legendWidth))
                    {
                        draw_list.AddLine(new Vector2((float)px, canvas_pos.Y + (float)tiretStart), new Vector2((float)px, canvas_pos.Y + (float)tiretEnd - 1), 0xFF606060, 1);
                        draw_list.AddLine(new Vector2((float)px, canvas_pos.Y + (float)ItemHeight), new Vector2((float)px, canvas_pos.Y + (float)regionHeight - 1), 0x30606060, 1);
                    }

                    if (baseIndex && px >= (canvas_pos.X + legendWidth))
                        draw_list.AddText(new Vector2((float)px + 3f, canvas_pos.Y), TextColor, $"{i}");
                };

                for (int i = sequence.FrameMin; i <= sequence.FrameMax; i += frameStep)
                {
                    drawLine(i, ItemHeight);
                }
                drawLine(sequence.FrameMin, ItemHeight);
                drawLine(sequence.FrameMax, ItemHeight);
                /*
                         draw_list.AddLine(canvas_pos, new Vector2(canvas_pos.X, canvas_pos.Y + controlHeight), 0xFF000000, 1);
                         draw_list.AddLine(new Vector2(canvas_pos.X, canvas_pos.Y + ItemHeight), new Vector2(canvas_size.X, canvas_pos.Y + ItemHeight), 0xFF000000, 1);
                         */

                // clip content
                draw_list.PushClipRect(childFramePos, childFramePos + childFrameSize);

                // draw item names in the legend rect on the left
                for (int i = 0, customHeight = 0; i < sequenceCount; i++)
                {
                    var animation = sequence.GetAnimation(i);
                    var tPos = new Vector2(contentMin.X + 3, contentMin.Y + i * ItemHeight + 2 + customHeight);
                    var tEndPos = new Vector2(contentMin.X + 3 + legendWidth, contentMin.Y + (i + 1) * ItemHeight + 2 + customHeight);
                    var rect = new ImRect(tPos, tEndPos);
                    if (rect.Contains(io.MousePos) && io.MouseDown[0])
                        selectedEntry = i;

                    draw_list.AddText(tPos, 0xFFFFFFFF, animation.Name ?? $"#{i + 1}");

                    if (sequenceOptions.HasFlag(SEQUENCER_OPTIONS.SEQUENCER_DEL))
                    {
                        bool overDel = SequencerAddDelButton(draw_list, new Vector2(contentMin.X + legendWidth - ItemHeight + 2 - 10, tPos.Y + 2), false);
                        if (overDel && io.MouseReleased[0])
                            delEntry = i;

                        bool overDup = SequencerAddDelButton(draw_list, new Vector2(contentMin.X + legendWidth - ItemHeight - ItemHeight + 2 - 10, tPos.Y + 2), true);
                        if (overDup && io.MouseReleased[0])
                            dupEntry = i;
                    }
                    customHeight += sequence.GetCustomHeight(i);
                }

                // clipping rect so items bars are not visible in the legend on the left when scrolled
                //

                // slots background
                for (int i = 0, customHeight = 0; i < sequenceCount; i++)
                {
                    // Draw as a zebra background
                    uint col = (i & 1) != 0 ? 0xFF3A3636 : 0xFF413D3D;

                    var localCustomHeight = sequence.GetCustomHeight(i);
                    Vector2 pos = new Vector2(contentMin.X + legendWidth, contentMin.Y + ItemHeight * i + 1 + customHeight);
                    Vector2 sz = new Vector2(canvas_size.X + canvas_pos.X, pos.Y + ItemHeight - 1 + localCustomHeight);
                    if (!popupOpened && cy >= pos.Y && cy < pos.Y + (ItemHeight + localCustomHeight) && movingEntry == -1 && cx > contentMin.X && cx < contentMin.X + canvas_size.X)
                    {
                        col += 0x80201008;
                        pos.X -= legendWidth;
                    }
                    draw_list.AddRectFilled(pos, sz, col, 0);
                    customHeight += localCustomHeight;
                }

                draw_list.PushClipRect(childFramePos + new Vector2((float)(legendWidth), 0f), childFramePos + childFrameSize);

                // vertical frame lines in content area
                Action<int, int> drawLineContent = (int i, int regionHeight) => {
                    int px = (int)canvas_pos.X + (int)(i * framePixelWidth) + legendWidth - (int)(firstFrameUsed * framePixelWidth);
                    int tiretStart = (int)(contentMin.Y);
                    int tiretEnd = (int)(contentMax.Y);

                    if (px <= (canvas_size.X + canvas_pos.X) && px >= (canvas_pos.X + legendWidth))
                    {
                        //draw_list.AddLine(new Vector2((float)px, canvas_pos.Y + (float)tiretStart), new Vector2((float)px, canvas_pos.Y + (float)tiretEnd - 1), 0xFF606060, 1);

                        draw_list.AddLine(new Vector2((float)(px), (float)(tiretStart)), new Vector2((float)(px), (float)(tiretEnd)), 0x30606060, 1);
                    }
                };
                for (int i = sequence.FrameMin; i <= sequence.FrameMax; i += frameStep)
                {
                    drawLineContent(i, (int)(contentHeight));
                }
                drawLineContent(sequence.FrameMin, (int)(contentHeight));
                drawLineContent(sequence.FrameMax, (int)(contentHeight));

                // selection
                bool selected = (selectedEntry >= 0);
                if (selected)
                {
                    // draw background differently if selected
                    var customHeight = 0;
                    for (int i = 0; i < selectedEntry; i++)
                        customHeight += sequence.GetCustomHeight(i); ;
                    draw_list.AddRectFilled(
                        new Vector2(contentMin.X, contentMin.Y + ItemHeight * selectedEntry + customHeight),
                        new Vector2(contentMin.X + canvas_size.X, contentMin.Y + ItemHeight * (selectedEntry + 1) + customHeight),
                        0x801080FF, 1f);
                }

                // slots
                for (int i = 0, customHeight = 0; i < sequenceCount; i++)
                {
                    var animation = sequence.GetAnimation(i);
                    var localCustomHeight = sequence.GetCustomHeight(i);

                    Vector2 pos = new Vector2(contentMin.X + legendWidth - firstFrameUsed * framePixelWidth, contentMin.Y + ItemHeight * i + 1 + customHeight);
                    var slotP1 = new Vector2(pos.X + animation.FrameStart * framePixelWidth, pos.Y + 2);
                    var slotP2 = new Vector2(pos.X + animation.FrameEnd * framePixelWidth + framePixelWidth, pos.Y + ItemHeight - 2);
                    var slotP3 = new Vector2(pos.X + animation.FrameEnd * framePixelWidth + framePixelWidth, pos.Y + ItemHeight - 2 + localCustomHeight);
                    uint slotColor = animation.Color | 0xFF000000;
                    uint slotColorHalf = (animation.Color & 0xFFFFFF) | 0x40000000;

                    if (slotP1.X <= (canvas_size.X + contentMin.X) && slotP2.X >= (contentMin.X + legendWidth))
                    {
                        draw_list.AddRectFilled(slotP1, slotP3, slotColorHalf, 2);
                        draw_list.AddRectFilled(slotP1, slotP2, slotColor, 2);
                    }
                    if (new ImRect(slotP1, slotP2).Contains(io.MousePos) && io.MouseDoubleClicked[0])
                    {
                        sequence.DoubleClick(i);
                    }

                    var rects = new ImRect[]
                    {
                        new ImRect(slotP1, new Vector2(slotP1.X + AnimationBarSideSelectionWidth, slotP2.Y)),
                        new ImRect(new Vector2(slotP2.X - AnimationBarSideSelectionWidth, slotP1.Y), slotP2),
                        new ImRect(slotP1, slotP2)
                    };

                    var quadColor = new uint[] { 0xFFFFFFFF, 0xFFFFFFFF, slotColor + (selected ? 0u : 0x202020u) };
                    if (movingEntry == -1 && (sequenceOptions.HasFlag(SEQUENCER_OPTIONS.SEQUENCER_EDIT_STARTEND))) // TODOFOCUS && backgroundRect.Contains(io.MousePos))
                    {
                        const uint AnimationResizeHoverColor = 0xFFFFFFFFu;
                        uint AnimationBarHoverColor = slotColor + 0x202020;

                        var animBarLeftRect = new ImRect(slotP1, new Vector2(slotP1.X + AnimationBarSideSelectionWidth, slotP2.Y));
                        var animBarRightRect = new ImRect(new Vector2(slotP2.X - AnimationBarSideSelectionWidth, slotP1.Y), slotP2);
                        var animBarRect = new ImRect(slotP1, slotP2);

                        var animationBarPartSelection = AnimationBarPart.None;
                        if (animBarLeftRect.Contains(io.MousePos))
                        {
                            animationBarPartSelection = AnimationBarPart.SelectionLeft;
                            draw_list.AddRectFilled(animBarLeftRect.Min, animBarLeftRect.Max, AnimationResizeHoverColor, 2);
                        }
                        else if (animBarRightRect.Contains(io.MousePos))
                        {
                            animationBarPartSelection = AnimationBarPart.SelectionRight;
                            draw_list.AddRectFilled(animBarRightRect.Min, animBarRightRect.Max, AnimationResizeHoverColor, 2);
                        }
                        else if (animBarRect.Contains(io.MousePos))
                        {
                            animationBarPartSelection = AnimationBarPart.Bar;
                            draw_list.AddRectFilled(animBarRect.Min, animBarRect.Max, AnimationBarHoverColor, 2);
                        }

                        if (ImGui.IsMouseClicked(0) && animationBarPartSelection != AnimationBarPart.None)
                        {
                            if (!new ImRect(childFramePos, childFramePos + childFrameSize).Contains(io.MousePos))
                                continue;

                            if (!MovingScrollBar && !MovingCurrentFrame)
                            {
                                movingEntry = i;
                                movingPos = cx;
                                movingPart = animationBarPartSelection;
                                sequence.BeginEdit(movingEntry);
                                break;
                            }
                        }
                    }

                    // custom draw
                    if (localCustomHeight > 0)
                    {
                        var rp = new Vector2(canvas_pos.X, contentMin.Y + ItemHeight * i + 1 + customHeight);
                        var customRect = new ImRect(rp + new Vector2(legendWidth - (firstFrameUsed - sequence.FrameMin - 0.5f) * framePixelWidth, (float)(ItemHeight)),
                                              rp + new Vector2(legendWidth + (sequence.FrameMax - firstFrameUsed - 0.5f + 2f) * framePixelWidth, (float)(localCustomHeight + ItemHeight)));
                        var clippingRect = new ImRect(rp + new Vector2((float)(legendWidth), (float)(ItemHeight)), rp + new Vector2(canvas_size.X, (float)(localCustomHeight + ItemHeight)));

                        var legendRect = new ImRect(rp + new Vector2(0f, (float)(ItemHeight)), rp + new Vector2((float)(legendWidth), (float)(localCustomHeight)));
                        var legendClippingRect = new ImRect(canvas_pos + new Vector2(0f, (float)(ItemHeight)), canvas_pos + new Vector2((float)(legendWidth), (float)(localCustomHeight + ItemHeight)));
                        customDraws.Add(new CustomDraw
                        {
                            index = i,
                            customRect = customRect,
                            legendRect = legendRect,
                            clippingRect = clippingRect,
                            legendClippingRect = legendClippingRect
                        });
                    }
                    else
                    {
                        var rp = new Vector2(canvas_pos.X, contentMin.Y + ItemHeight * i + customHeight);
                        var customRect = new ImRect(rp + new Vector2(legendWidth - (firstFrameUsed - sequence.FrameMin - 0.5f) * framePixelWidth, (float)(0f)),
                                              rp + new Vector2(legendWidth + (sequence.FrameMax - firstFrameUsed - 0.5f + 2f) * framePixelWidth, (float)(ItemHeight)));
                        var clippingRect = new ImRect(rp + new Vector2((float)(legendWidth), (float)(0f)), rp + new Vector2(canvas_size.X, (float)(ItemHeight)));

                        compactCustomDraws.Add(new CustomDraw
                        {
                            index = i,
                            customRect = customRect,
                            legendRect = new ImRect(),
                            clippingRect = clippingRect,
                            legendClippingRect = new ImRect()
                        });
                    }
                    customHeight += localCustomHeight;
                }


                // moving
                if (/*backgroundRect.Contains(io.MousePos) && */movingEntry >= 0)
                {
                    ImGui.CaptureMouseFromApp();
                    int diffFrame = (int)((cx - movingPos) / framePixelWidth);
                    if (Math.Abs(diffFrame) > 0)
                    {
                        var animation = sequence.GetAnimation(movingEntry);
                        selectedEntry = movingEntry;
                        if (movingPart.HasFlag(AnimationBarPart.SelectionLeft))
                            animation.FrameStart += diffFrame;
                        if (movingPart.HasFlag(AnimationBarPart.SelectionRight))
                            animation.FrameEnd += diffFrame;
                        if (animation.FrameStart < 0)
                        {
                            if (movingPart.HasFlag(AnimationBarPart.SelectionRight))
                                animation.FrameEnd -= animation.FrameStart;
                            animation.FrameStart = 0;
                        }
                        if (movingPart.HasFlag(AnimationBarPart.SelectionLeft) && animation.FrameStart > animation.FrameEnd)
                            animation.FrameStart = animation.FrameEnd;
                        if (movingPart.HasFlag(AnimationBarPart.SelectionRight) && animation.FrameEnd < animation.FrameStart)
                            animation.FrameEnd = animation.FrameStart;
                        movingPos += (int)(diffFrame * framePixelWidth);
                    }
                    if (!io.MouseDown[0])
                    {
                        // single select
                        if (/*diffFrame != 0 &&*/ movingPart != 0)
                        {
                            selectedEntry = movingEntry;
                            ret = true;
                        }

                        movingEntry = -1;
                        sequence.EndEdit();
                    }
                }

                // cursor
                if (currentFrame >= firstFrame && currentFrame <= sequence.FrameMax)
                {
                    const float cursorWidth = 8f;
                    float cursorOffset = contentMin.X + legendWidth + (currentFrame - firstFrameUsed) * framePixelWidth + framePixelWidth / 2 - cursorWidth * 0.5f;
                    draw_list.AddLine(new Vector2(cursorOffset, canvas_pos.Y), new Vector2(cursorOffset, contentMax.Y), 0xA02A2AFF, cursorWidth);
                    draw_list.AddText(new Vector2(cursorOffset + 10, canvas_pos.Y + 2), 0xFF2A2AFF, $"{currentFrame}");
                }

                draw_list.PopClipRect();
                draw_list.PopClipRect();

                foreach (var customDraw in customDraws)
                    sequence.CustomDraw(customDraw.index, draw_list, customDraw.customRect, customDraw.legendRect, customDraw.clippingRect, customDraw.legendClippingRect);
                foreach (var customDraw in compactCustomDraws)
                    sequence.CustomDrawCompact(customDraw.index, draw_list, customDraw.customRect, customDraw.clippingRect);

                // copy paste
                if (sequenceOptions.HasFlag(SEQUENCER_OPTIONS.SEQUENCER_COPYPASTE))
                {
                    var rectCopy = new ImRect(new Vector2(contentMin.X + 100, canvas_pos.Y + 2)
                        , new Vector2(contentMin.X + 100 + 30, canvas_pos.Y + ItemHeight - 2));
                    bool inRectCopy = rectCopy.Contains(io.MousePos);
                    uint copyColor = inRectCopy ? 0xFF1080FF : 0xFF000000;
                    draw_list.AddText(rectCopy.Min, copyColor, "Copy");

                    var rectPaste = new ImRect(new Vector2(contentMin.X + 140, canvas_pos.Y + 2)
                                        , new Vector2(contentMin.X + 140 + 30, canvas_pos.Y + ItemHeight - 2));
                    bool inRectPaste = rectPaste.Contains(io.MousePos);
                    uint pasteColor = inRectPaste ? 0xFF1080FF : 0xFF000000;
                    draw_list.AddText(rectPaste.Min, pasteColor, "Paste");

                    if (inRectCopy && io.MouseReleased[0])
                    {
                        sequence.Copy();
                    }
                    if (inRectPaste && io.MouseReleased[0])
                    {
                        sequence.Paste();
                    }
                }
                //

                ImGui.EndChildFrame();
                ImGui.PopStyleColor();
                if (hasScrollBar)
                {
                    ImGui.InvisibleButton("scrollBar", scrollBarSize);
                    Vector2 scrollBarMin = ImGui.GetItemRectMin();
                    Vector2 scrollBarMax = ImGui.GetItemRectMax();

                    // ratio = number of frames visible in control / number to total frames

                    float startFrameOffset = ((float)(firstFrameUsed - sequence.FrameMin) / (float)frameCount) * (canvas_size.X - legendWidth);
                    var scrollBarA = new Vector2(scrollBarMin.X + legendWidth, scrollBarMin.Y - 2);
                    var scrollBarB = new Vector2(scrollBarMin.X + canvas_size.X, scrollBarMax.Y - 1);
                    draw_list.AddRectFilled(scrollBarA, scrollBarB, 0xFF222222, 0);

                    var scrollBarRect = new ImRect(scrollBarA, scrollBarB);
                    bool inScrollBar = scrollBarRect.Contains(io.MousePos);

                    draw_list.AddRectFilled(scrollBarA, scrollBarB, 0xFF101010, 8);


                    var scrollBarC = new Vector2(scrollBarMin.X + legendWidth + startFrameOffset, scrollBarMin.Y);
                    var scrollBarD = new Vector2(scrollBarMin.X + legendWidth + barWidthInPixels + startFrameOffset, scrollBarMax.Y - 2);
                    draw_list.AddRectFilled(scrollBarC, scrollBarD, (inScrollBar || MovingScrollBar) ? 0xFF606060 : 0xFF505050, 6);

                    float handleRadius = (scrollBarMax.Y - scrollBarMin.Y) / 2;
                    var barHandleLeft = new ImRect(scrollBarC, new Vector2(scrollBarC.X + 14, scrollBarD.Y));
                    var barHandleRight = new ImRect(new Vector2(scrollBarD.X - 14, scrollBarC.Y), scrollBarD);

                    bool onLeft = barHandleLeft.Contains(io.MousePos);
                    bool onRight = barHandleRight.Contains(io.MousePos);


                    draw_list.AddRectFilled(barHandleLeft.Min, barHandleLeft.Max, (onLeft || sizingLBar) ? 0xFFAAAAAA : 0xFF666666, 6);
                    draw_list.AddRectFilled(barHandleRight.Min, barHandleRight.Max, (onRight || sizingRBar) ? 0xFFAAAAAA : 0xFF666666, 6);

                    var scrollBarThumb = new ImRect(scrollBarC, scrollBarD);
                    if (sizingRBar)
                    {
                        if (!io.MouseDown[0])
                        {
                            sizingRBar = false;
                        }
                        else
                        {
                            // Resize scrollbar from the right
                            float barNewWidth = Math.Max(barWidthInPixels + io.MouseDelta.X, MinBarWidth);
                            float barRatio = barNewWidth / barWidthInPixels;
                            framePixelWidthTarget = framePixelWidth = framePixelWidth / barRatio;
                            int newVisibleFrameCount = (int)((canvas_size.X - legendWidth) / framePixelWidthTarget);
                            int lastFrame = firstFrame + newVisibleFrameCount;
                            if (lastFrame > sequence.FrameMax)
                            {
                                framePixelWidthTarget = framePixelWidth = (canvas_size.X - legendWidth) / (float)(sequence.FrameMax - firstFrame);
                            }
                        }
                    }
                    else if (sizingLBar)
                    {
                        if (!io.MouseDown[0])
                        {
                            sizingLBar = false;
                        }
                        else
                        {
                            // Resize scrollbar from the left
                            if (Math.Abs(io.MouseDelta.X) > FLT_EPSILON)
                            {
                                float barNewWidth = Math.Max(barWidthInPixels - io.MouseDelta.X, MinBarWidth);
                                float barRatio = barNewWidth / barWidthInPixels;
                                float previousFramePixelWidthTarget = framePixelWidthTarget;
                                framePixelWidthTarget = framePixelWidth = framePixelWidth / barRatio;
                                int newVisibleFrameCount = (int)(visibleFrameCount / barRatio);
                                int newFirstFrame = firstFrame + newVisibleFrameCount - visibleFrameCount;
                                newFirstFrame = Math.Clamp(newFirstFrame, sequence.FrameMin, Math.Max(sequence.FrameMax - visibleFrameCount, sequence.FrameMin));
                                if (newFirstFrame == firstFrame)
                                {
                                    framePixelWidth = framePixelWidthTarget = previousFramePixelWidthTarget;
                                }
                                else
                                {
                                    firstFrame = newFirstFrame;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (MovingScrollBar)
                        {
                            if (!io.MouseDown[0])
                            {
                                MovingScrollBar = false;
                            }
                            else
                            {
                                float framesPerPixelInBar = barWidthInPixels / (float)visibleFrameCount;
                                firstFrame = (int)((io.MousePos.X - panningViewSource.X) / framesPerPixelInBar) - panningViewFrame;
                                firstFrame = Math.Clamp(firstFrame, sequence.FrameMin, Math.Max(sequence.FrameMax - visibleFrameCount, sequence.FrameMin));
                            }
                        }
                        else
                        {
                            if (scrollBarThumb.Contains(io.MousePos) && ImGui.IsMouseClicked(0) && !MovingCurrentFrame && movingEntry == -1)
                            {
                                MovingScrollBar = true;
                                panningViewSource = io.MousePos;
                                panningViewFrame = firstFrame;
                            }
                            if (!sizingRBar && onRight && ImGui.IsMouseClicked(0))
                                sizingRBar = true;
                            if (!sizingLBar && onLeft && ImGui.IsMouseClicked(0))
                                sizingLBar = true;

                        }
                    }
                }
            }

            ImGui.EndGroup();

            if (regionRect.Contains(io.MousePos))
            {
                bool overCustomDraw = false;
                foreach (var custom in customDraws)
                {
                    if (custom.customRect.Contains(io.MousePos))
                    {
                        overCustomDraw = true;
                    }
                }
                if (overCustomDraw)
                {
                }
                else
                {
                    //frameOverCursor = *firstFrame + (int)(visibleFrameCount * ((io.MousePos.X - (float)legendWidth - canvas_pos.X) / (canvas_size.X - legendWidth)));
                    ////frameOverCursor = max(min(*firstFrame - visibleFrameCount / 2, frameCount - visibleFrameCount), 0);

                    ///**firstFrame -= frameOverCursor;
                    //*firstFrame *= framePixelWidthTarget / framePixelWidth;
                    //*firstFrame += frameOverCursor;*/
                    //if (io.MouseWheel < -FLT_EPSILON)
                    //{
                    //    *firstFrame -= frameOverCursor;
                    //    *firstFrame = int(*firstFrame * 1.1f);
                    //    framePixelWidthTarget *= 0.9f;
                    //    *firstFrame += frameOverCursor;
                    //}

                    //if (io.MouseWheel > FLT_EPSILON)
                    //{
                    //    *firstFrame -= frameOverCursor;
                    //    *firstFrame = int(*firstFrame * 0.9f);
                    //    framePixelWidthTarget *= 1.1f;
                    //    *firstFrame += frameOverCursor;
                    //}
                }
            }

            if (expanded)
            {
                bool overExpanded = SequencerAddDelButton(draw_list, new Vector2(canvas_pos.X + 2, canvas_pos.Y + 2), !expanded);
                if (overExpanded && io.MouseReleased[0])
                    expanded = !expanded;
            }

            if (delEntry != -1)
            {
                sequence.Del(delEntry);
                if ((selectedEntry == delEntry || selectedEntry >= sequence.ItemCount))
                    selectedEntry = -1;
            }

            if (dupEntry != -1)
            {
                sequence.Duplicate(dupEntry);
            }
            return ret;
        }
    }
}
