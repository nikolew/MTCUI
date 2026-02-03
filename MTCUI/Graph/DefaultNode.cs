using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTCUI.ViewModels;
using System;
using System.Windows.Input;
using Windows.Foundation;

namespace MTCUI.Graph
{
    public partial class DefaultNode : NodeBase
    {
        public DataTemplate TargetTemplate
        {
            get => (DataTemplate)GetValue(TargetTemplateProperty);
            set => SetValue(TargetTemplateProperty, value);
        }

        public static readonly DependencyProperty TargetTemplateProperty =
            DependencyProperty.Register(
                nameof(TargetTemplate),
                typeof(DataTemplate),
                typeof(DefaultNode),
                new PropertyMetadata(null));

        public DataTemplateSelector TargetTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(TargetTemplateSelectorProperty);
            set => SetValue(TargetTemplateSelectorProperty, value);
        }

        public static readonly DependencyProperty TargetTemplateSelectorProperty =
            DependencyProperty.Register(nameof(TargetTemplateSelector),
                                        typeof(DataTemplateSelector),
                                        typeof(DefaultNode),
                                        new PropertyMetadata(null));
        public string TargetId
        {
            get => (string)GetValue(TargetIdProperty);
            set => SetValue(TargetIdProperty, value);
        }

        public static readonly DependencyProperty TargetIdProperty =
            DependencyProperty.Register(
                nameof(TargetId),
                typeof(string),
                typeof(DefaultNode),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(DefaultNode),
                new PropertyMetadata(null, OnCommandChanged));


        public DefaultNode(Canvas canvas, Point offset, double scale) : base(canvas, offset)
        {
            DefaultStyleKey = typeof(DefaultNode);
        }


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();  
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposing()
        {

        }

        protected override void OnUpdateTranslation()
        {

        }

        public void ButtonClicked()
        {

        }

        public void TargetClicked()
        {

        }

       
    }
}
