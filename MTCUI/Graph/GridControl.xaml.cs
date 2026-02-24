using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI.Graph
{
    public sealed partial class GridControl : UserControl
    {
        public float GridStep { get; set; } = 25f;   // разстояние между линиите (px)
        public float GridThickness { get; set; } = 1f;
        
        public GridControl()
        {
            InitializeComponent();
           
            GridCanvas.Loaded += (_, __) => GridCanvas.Invalidate();
            
            GridCanvas.SizeChanged += (_, __) =>
            {
                GridCanvas.Invalidate();
            };
        }

        private void GridCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            float width = (float)sender.ActualWidth;
            float height = (float)sender.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            var minor = Microsoft.UI.Colors.LightGray;
            var major = Microsoft.UI.Colors.Gray;

            int index = 0;

            // Вертикални линии
            for (float x = 0; x <= width; x += GridStep, index++)
            {
                bool isMajor = index % 5 == 0;
                ds.DrawLine(x, 0, x, height,
                    isMajor ? major : minor,
                    isMajor ? 1.5f : 1f);
            }

            index = 0;

            // Хоризонтални линии
            for (float y = 0; y <= height; y += GridStep, index++)
            {
                bool isMajor = index % 5 == 0;
                ds.DrawLine(0, y, width, y,
                    isMajor ? major : minor,
                    isMajor ? 1.5f : 1f);
            }
        }
    }
}
