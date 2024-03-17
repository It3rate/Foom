using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace NumbersSkia.Agent
{
    public class MouseArgs
    {
        public MouseButtons Button { get; private set; }
        public int Clicks { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Delta { get; private set; }

        public MouseArgs(int x, int y, int delta, int clicks, MouseButtons button) 
        { 
            X = x;
            Y = y;
            Delta = delta;
            Clicks = clicks;
            Button = button;
        }

        public SKPoint Location => new SKPoint(X, Y);
    }
    public enum MouseButtons
    {
        //
        // Summary:
        //     The left mouse button was pressed.
        Left = 0x100000,
        //
        // Summary:
        //     No mouse button was pressed.
        None = 0,
        //
        // Summary:
        //     The right mouse button was pressed.
        Right = 0x200000,
        //
        // Summary:
        //     The middle mouse button was pressed.
        Middle = 0x400000,
        //
        // Summary:
        //     The first XButton was pressed.
        XButton1 = 0x800000,
        //
        // Summary:
        //     The second XButton was pressed.
        XButton2 = 0x1000000
    }
}
