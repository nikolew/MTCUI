using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTCUI.Utilities;
using MTCUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;

namespace MTCUI.Graph
{
    public class NodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Target6 { get; set; }
        public DataTemplate Target7 { get; set; }
        public DataTemplate Target8 { get; set; }
        public DataTemplate Target9 { get; set; }
        public DataTemplate Target10 { get; set; }
        public DataTemplate Target10A { get; set; }
        public DataTemplate Default { get; set; }


        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => Select(item);

        private DataTemplate Select(object item)
        {
            var vm = item as NodeViewModel;
            return (DataTemplate)Application.Current.Resources[vm.Node.TargetType.ToString()];

        }
    }
}
