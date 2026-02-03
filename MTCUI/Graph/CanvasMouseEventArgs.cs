using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace MTCUI.Graph
{
    public class CanvasMouseEventArgs : EventArgs
    {
        // This position has taken scale and offset into account.
        public Point TransformedPosition { get; }

        public CanvasMouseEventArgs(Point transformedPosition)
        {
            TransformedPosition = transformedPosition;
        }
    }
}
