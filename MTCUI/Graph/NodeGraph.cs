using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTCUI.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Windows.Devices.Input;
using Windows.Foundation;

namespace MTCUI.Graph
{
    public partial class NodeGraph : ItemsControl
    {
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(NodeGraph), new PropertyMetadata(1.0));

        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(Point), typeof(NodeGraph), new PropertyMetadata(new Point(0, 0), OffsetPropertyChanged));


        public ICommand BeginMoveNodesCommand
        {
            get => (ICommand)GetValue(BeginMoveNodesCommandProperty);
            set => SetValue(BeginMoveNodesCommandProperty, value);
        }
        public static readonly DependencyProperty BeginMoveNodesCommandProperty =
            DependencyProperty.Register(nameof(BeginMoveNodesCommand), typeof(ICommand), typeof(NodeGraph), new PropertyMetadata(null));

        public ICommand MouseMoveOnCanvasCommand
        {
            get => (ICommand)GetValue(MouseMoveOnCanvasCommandProperty);
            set => SetValue(MouseMoveOnCanvasCommandProperty, value);
        }
        public static readonly DependencyProperty MouseMoveOnCanvasCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveOnCanvasCommand), typeof(ICommand), typeof(NodeGraph), new PropertyMetadata(null));


        private static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Action<bool> OnChanged;

        public Canvas Canvas { get; private set; } = null;

        readonly List<object> _delayToBindNodeVMs = new List<object>();
        readonly List<NodeBase> _draggingNodes = new List<NodeBase>();

        bool _isStartDraggingNode = false;
        bool _pressedRightBotton = false;
        bool _pressedMouseToSelect = false;
        bool _isSelectionChanging = false;
        bool _pressedMouseToMove = false;
        bool _isRangeSelecting = false;
        bool _pressedKeyToMove = false;

        Point _dragStartPointToMoveNode = new Point();
        Point _dragStartPointToMoveOffset = new Point();
        Point _captureOffset = new Point();

        private NodeBase _selectedNode;


        public NodeGraph()
        {
            RegisterPropertyChangedCallback(ItemsSourceProperty, OnItemsSourceChanged);

        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is DefaultNode node)
            {
                node.DataContext = item; // <-- ТОВА Е КРИТИЧНО
            }
        }


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas = GetTemplateChild("PART_GraphControl") as Canvas;

            if (_delayToBindNodeVMs.Count > 0)
            {
                AddNodesToCanvas(_delayToBindNodeVMs.OfType<object>().ToArray());
                _delayToBindNodeVMs.Clear();
            }
        }

        private void OnItemsSourceChanged(DependencyObject sender, DependencyProperty dp)
        {
            var newValue = ItemsSource;

            CollectionPropertyChanged<DefaultNode>(
                this,
                newValue,
                NodeCollectionChanged,
                _delayToBindNodeVMs,
                AddNodesToCanvas);
        }

        private void NodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged<DefaultNode>(e.Action, e.OldItems, e.NewItems, RemoveNodesFromCanvas, RemoveNodesFromCanvas, AddNodesToCanvas);
        }

        static void CollectionPropertyChanged<T>(
            NodeGraph nodeGraph,
            object newValue,
            NotifyCollectionChangedEventHandler collectionChanged,
            List<object> delayToBindVMs,
            Action<object[]> addElementToCanvas) where T : UIElement, ICanvasObject
        {


            if (newValue != null && newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += collectionChanged;
            }

            // below process is node collection changed.
            if (nodeGraph.Canvas == null)
            {
                if (newValue != null && newValue is IEnumerable enumerable)
                {
                    delayToBindVMs.AddRange(enumerable.OfType<object>());
                }
                return;
            }

            // remove old node links
            var removeElements = nodeGraph.Canvas.Children.OfType<T>().ToArray();
            foreach (var removeElement in removeElements)
            {
                removeElement.Dispose();
                nodeGraph.Canvas.Children.Remove(removeElement);
            }

            // add new node links
            if (newValue != null)
            {
                var newEnumerable = newValue as IEnumerable;
                addElementToCanvas(newEnumerable.OfType<object>().ToArray());
            }
        }

        void CollectionChanged<T>(
            NotifyCollectionChangedAction action,
            IList oldItems,
            IList newItems,
            Action<object[]> removeItemWithDataContext,
            Action<T[]> removeItemDirectly,
            Action<object[]> addItemWithDataContext) where T : UIElement, ICanvasObject
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Reset:
                    if (Canvas != null)
                    {
                        removeItemDirectly(Canvas.Children.OfType<T>().ToArray());
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // ignore.
                    break;
                default:
                    if (oldItems?.Count > 0)
                    {
                        removeItemWithDataContext(oldItems.OfType<object>().ToArray());
                    }
                    if (newItems?.Count > 0)
                    {
                        addItemWithDataContext(newItems.OfType<object>().ToArray());
                    }
                    break;
            }
        }

        void RemoveNodesFromCanvas(object[] removeVMs)
        {
            var removeNodes = new List<DefaultNode>();
            var children = Canvas.Children.OfType<DefaultNode>().ToArray();

            foreach (var removeVM in removeVMs)
            {
                var removeElement = children.First(arg => arg.DataContext == removeVM);
                removeNodes.Add(removeElement);
            }

            RemoveNodesFromCanvas(removeNodes.ToArray());
        }

        void RemoveNodesFromCanvas(DefaultNode[] removeNodes)
        {
            foreach (var removeNode in removeNodes)
            {
                //UnsubscribeNodeEvent(removeNode);

                removeNode.Dispose();

                Canvas.Children.Remove(removeNode);
            }
        }


        void SubscribeNodeEvent(NodeBase node)
        {
            node.PointerPressed += Node_MouseDown;
            node.PointerReleased += Node_MouseUp;
            //node.BeginSelectionChanged += Node_BeginSelectionChanged;
            //node.EndSelectionChanged += Node_EndSelectionChanged;
        }

        private void AddNodesToCanvas(object[] addVMs)
        {
            if (Canvas == null)
            {
                _delayToBindNodeVMs.AddRange(addVMs);
                return;
            }

            foreach (var vm in addVMs)
            {
                var nvm = vm as NodeViewModel;
                var node = new DefaultNode(Canvas, Offset, Scale)
                {
                    Position = nvm.Node.Position,
                    DataContext = vm,
                    Content = vm,
                    Template = (ControlTemplate)Application.Current.Resources["__NodeTemplate__"],
                };
                node.ApplyTemplate();
                node.DoubleTapped += (s, e) =>
                {
                    nvm.MouseDoubleClick();
                };

                node.Style = GetNodeStyle(vm, node);

                SubscribeNodeEvent(node);

                Canvas.Children.Add(node);

                // recalculate header size, position.
                node.UpdateLayout();
            }
        }

        Style GetNodeStyle(object dataContext, DependencyObject element)
        {
            if (ItemContainerStyleSelector != null)
            {
                return ItemContainerStyleSelector.SelectStyle(dataContext, element);
            }

            return ItemContainerStyle;
        }

        private void Node_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint(this).Properties;
            if (properties.IsLeftButtonPressed)
            {
                _pressedMouseToSelect = true;

                var node = sender as NodeBase;
                if (node != null)
                {
                    node.CaptureDragStartPosition();
                    _selectedNode = node;
                    VisualStateManager.GoToState(node, "Selected", true);
                }

                _dragStartPointToMoveNode = e.GetCurrentPoint(Canvas).Position;
            }
        }

        private void Node_MouseUp(object sender, PointerRoutedEventArgs e)
        {
            var node = sender as NodeBase;
            UpdateNodeSelectionItem(node);
            VisualStateManager.GoToState(node, "Unselected", true);
            _selectedNode = null;

            
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            OnChanged?.Invoke(true);
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);

            if (_pressedMouseToSelect)
            {

            }

            if (_selectedNode is not null)
            {
                var current = e.GetCurrentPoint(this).Position;
                var diff = new Point(current.X - _dragStartPointToMoveNode.X, current.Y - _dragStartPointToMoveNode.Y);

                double x = _selectedNode.DragStartPosition.X + diff.X;
                double y = _selectedNode.DragStartPosition.Y + diff.Y;

                var x1 = Math.Round(x);
                var y1 = Math.Round(y);

                _selectedNode.UpdatePosition(x1, y1);
            }
        }

        private void UpdateNodeSelectionItem<T>(T item) where T : ISelectableObject
        {
            var selectableContents = Canvas.Children.OfType<ISelectableObject>();
            foreach (var selectableContent in selectableContents)
            {
                selectableContent.IsSelected = false;
            }

            item.IsSelected = true;
            UpdateSelectedItems(item);
        }

        void UpdateSelectedItems<T>(T item) where T : ISelectableObject
        {
            if(item.IsSelected)
            {
                
            }
        }

        


    }
}
