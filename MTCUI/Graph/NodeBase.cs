using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MTCUI.ViewModels;
using System;
using Windows.Foundation;

namespace MTCUI.Graph
{
    public abstract class NodeBase : ContentControl, ISelectableObject, ICanvasObject
    {
        internal EventHandler BeginSelectionChanged { get; set; } = null;
        internal EventHandler EndSelectionChanged { get; set; } = null;

        public Guid Guid { get; set; }
       

        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            nameof(Position),
            typeof(Point),
            typeof(NodeBase),
            new PropertyMetadata(new Point(0, 0), PositionPropertyChanged));
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => UpdateSelectedState(value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(NodeBase),
            new PropertyMetadata(false, IsSelectedPropertyChanged));

        protected Point Offset { get; private set; } = new Point(0, 0);
        protected TranslateTransform Translate { get; private set; } = new TranslateTransform();

        public Point DragStartPosition { get; private set; } = new Point(0, 0);


        internal NodeBase(Canvas canvas, Point offset)
        {
            Translate.X = Position.X + Offset.X;
            Translate.Y = Position.Y + Offset.Y;

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(Translate);

            RenderTransform = transformGroup;
        }

        internal void UpdatePosition(double x, double y)
        {
            Position = new Point(x, y);
            if (DataContext is NodeViewModel vm)
            {
                vm.Node.Position = Position;
            }

            UpdateTranslation();
        }

        void UpdateTranslation()
        {
            Translate.X = Position.X + Offset.X;
            Translate.Y = Position.Y + Offset.Y;

            OnUpdateTranslation();
        }

        private static void PositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var node = (DefaultNode)d;
            //var p = (Point)e.NewValue;

            //Canvas.SetLeft(node, p.X);
            //Canvas.SetTop(node, p.Y);


            (d as NodeBase).UpdateTranslation();
        }

        public void UpdateOffset(Point offset)
        {
            throw new NotImplementedException();
        }

        internal void CaptureDragStartPosition()
        {
            DragStartPosition = Position;
        }
        void UpdateSelectedState(bool value)
        {
            SetValue(IsSelectedProperty, value);
            //Panel.SetZIndex(this, value ? 1 : 0);
        }

        static void IsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var node = (NodeBase)d;

            node.BeginSelectionChanged?.Invoke(node, EventArgs.Empty);

            if (node.IsFocusEngagementEnabled)
            {
                //node.Focus(FocusState.);
            }

            node.EndSelectionChanged?.Invoke(node, EventArgs.Empty);
        }

        public void Dispose()
        {
            //You need to clear Style.
            // Because implemented on style for binding.
            Style = null;

            // Clear binding for subscribing source changed event from old control.
            // throw exception about visual tree ancestor different if you not clear binding.
           // BindingOperations.ClearAllBindings(this);

            // Clear binding myself first.
            // Because children throw exception about visual tree ancestor different when my Style to be null.
            OnDisposing();
        }

        protected abstract void OnDisposing();
        protected abstract void OnUpdateTranslation();

        public bool Contains(Rect rect)
        {
            throw new NotImplementedException();
        }

        public bool IntersectsWith(Rect rect)
        {
            throw new NotImplementedException();
        }
    }
}
