using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class InputSim
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public static implicit operator Vector2(POINT point)
        {
            return new Vector2(point.x, point.y);
        }
    }

    [DllImport("user32.dll")]
    public static extern void GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    public const int MOUSE_LEFTDOWN = 0x02;
    public const int MOUSE_LEFTUP = 0x04;

    public static void LeftClick(int xpos, int ypos)
    {
        SetCursorPos(xpos, ypos);
        mouse_event(MOUSE_LEFTDOWN, xpos, ypos, 0, 0);
        mouse_event(MOUSE_LEFTDOWN, xpos, ypos, 0, 0);
    }

    public static void LeftClick()
    {
        Vector2 mousepos = MousePosition;
        mouse_event(MOUSE_LEFTDOWN, (int)MousePosition.x, (int)MousePosition.y, 0, 0);
        mouse_event(MOUSE_LEFTDOWN, (int)MousePosition.x, (int)MousePosition.y, 0, 0);
    }

    public static void PressLeftClick()
    {
        Vector2 mousepos = MousePosition;
        mouse_event(MOUSE_LEFTDOWN, (int)MousePosition.x, (int)MousePosition.y, 0, 0);
    }

    public static void ReleaseLeftClick()
    {
        Vector2 mousepos = MousePosition;
        mouse_event(MOUSE_LEFTUP, (int)MousePosition.x, (int)MousePosition.y, 0, 0);
    }

    public static Vector2 MousePosition
    {
        get
        {
            POINT pt;
            GetCursorPos(out pt);
            return pt;
        }
    }
}
