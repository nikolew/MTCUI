using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Enums;
using MTCCore.Messages.Nodes;
using MTCCore.Models;
using MTCCore.Services;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeManagerViewModel : ViewModel
    {
        private readonly INodeService _nodeService;
        public event PropertyChangedEventHandler PropertyChanged;

        public Array TargetTypes { get; } = Enum.GetValues(typeof(TargetType));
        
        [ObservableProperty]
        private bool _isDirty;
        
        [ObservableProperty]
        private bool _buttonSaveEnabled;

        public Array TargetGroups { get; } = Enum.GetValues(typeof(Group));
        private Group _selectedGroup;
        public Group SelectedTargetGroup 
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTargetGroup)));
            }
        }

        private static readonly ObservableCollection<ItemModel> itemModels = new();
        
        [ObservableProperty]
        private ObservableCollection<ItemModel> _items = itemModels;
      

        public NodeManagerViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
            
            WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.NodeEvent));
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher)
        {
            try
            {
                ButtonSaveEnabled = false;
                
                _nodeService.GetAllNodes().ForEach(node => {
                    dispatcher.TryEnqueue(() =>
                    {
                        var item = new ItemModel
                        {
                            UniqueId = node.UniqueId,
                            Position = node.Position,
                            TargetId = node.TargetId,
                            Status = "offline"
                        };
                        item.SaveAction += OnSaveAction;
                        item.Load(node.TargetType, node.Group, node.Distance);
                        
                        Items.Add(item);                      
                    });
                });
            }
            catch(Exception ex)
            {
                // Log exception
                System.Diagnostics.Trace.WriteLine($"Error initializing NodeManagerViewModel: {ex.Message}");
            }
        }

       

        private void OnNodeEvent(NodeEventModel value)
        {
            var id = Convert.ToString(value.Id);
            var node = Items.Where(x => x.TargetId == id).SingleOrDefault();

            if (node == null)
            {
                return;
            }

            node.Status = value.Online ? "online" : "offline";
        }

        [RelayCommand]
        void Save()
        {
            var nodesToSave = new List<NodeModel>();
            foreach (var node in Items) { 
            
                nodesToSave.Add(new NodeModel
                {
                    UniqueId = node.UniqueId,
                    TargetType = node.TargetType,
                    Position = node.Position,
                    TargetId = node.TargetId,
                    Distance = node.Distance,
                    Group = node.Group
                });
            }

            _nodeService.UpdateNodes(nodesToSave);
        }

        internal void Clear()
        {
            Items.Clear();
        }

        private void OnSaveAction(ItemModel node)
        {
            var updateNode = new NodeModel
            {
                UniqueId = node.UniqueId,
                TargetType = node.TargetType,
                Position = node.Position,
                TargetId = node.TargetId,
                Distance = node.Distance,
                Group = node.Group
            };
            
            _nodeService.UpdateNode(updateNode);
            
            WeakReferenceMessenger.Default.Send(new NodeUpdateMessage(updateNode.UniqueId));
        }
    }
}
