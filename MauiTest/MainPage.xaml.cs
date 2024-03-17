using SkiaSharp.Views.Maui;
using SkiaSharp;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using Microsoft.Maui.Layouts;

namespace MauiTest
{
    public partial class MainPage : ContentPage
    {
        //AdjustBasis
        //SetSelectedAsBasis
        //Reverse
        //NegateSelection
        //FlipBasis
        //FlipSelected
        //FlipSelectedPolarity
        //DeleteSelected
        //ClearPaths
        //AddTick
        //SubtractTick
        //ExpandMinMax
        //ContractMinMax
        public MainPage()
        {
            InitializeComponent(); 
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Clear(SKColors.White);

            var paint = new SKPaint
            {
                Color = SKColors.Blue,
                StrokeWidth = 5
            };

            canvas.DrawLine(0, 0, e.Info.Width, e.Info.Height, paint);
        }

        private void OnCanvasViewTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Handle mouse down / touch begin
                    break;
                case SKTouchAction.Released:
                    // Handle mouse up / touch end
                    break;
            }

            e.Handled = true;
            //canvasView.InvalidateSurface();
        }
        private void OnCreateDomain(object sender, EventArgs e)
        {
            Debug.WriteLine("OnCreateDomain:");
        }
        private void OnNumberMode(object sender, EventArgs e)
        {
            Debug.WriteLine("OnNumberMode:");
        }
        private void OnBasisMode(object sender, EventArgs e)
        {
            Debug.WriteLine("OnBasisMode:");
        }
    }

}
