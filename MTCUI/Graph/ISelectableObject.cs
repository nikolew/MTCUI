using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;


namespace MTCUI.Graph
{
    internal interface ISelectableObject
    {
        bool IsSelected { get; set; }
        object DataContext { get; set; }

        bool Contains(Rect rect);
        bool IntersectsWith(Rect rect);
    }
}
