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
        Left = 0x100000,
        None = 0,
        Right = 0x200000,
        Middle = 0x400000,
        XButton1 = 0x800000,
        XButton2 = 0x1000000
    }
}
