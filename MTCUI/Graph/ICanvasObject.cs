using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace MTCUI.Graph
{
    public interface ICanvasObject : IDisposable
    {
        Guid Guid { get; set; }
        void UpdateOffset(Point offset);
    }
}
