﻿using OpenKh.Engine.Renders;
using OpenKh.Research.Kh2AnimTest.Infrastructure;

namespace OpenKh.Research.Kh2AnimTest.States.Title
{
    public enum MainMenuState
    {
        Running,
        StartNewGame
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    interface ITitleMainMenu
    {
        Kernel Kernel { get; }
        InputManager InputManager { get; }
        IMessageRenderer MessageRenderer { get; }
        MainMenuState State { set; }

        byte[] GetMessage(ushort messageId);

        void Print(ushort messageId, float left, float top,
            uint color = 0xffffffff, TextAlignment alignment = TextAlignment.Left);
    }
}
