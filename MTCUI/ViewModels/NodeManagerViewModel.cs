using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using MTCCore.Enums;
using MTCCore.Models;
using MTCCore.Services;
using MTCUI.Messages;
using MTCUI.Models;
using MTCUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCUI.ViewModels
{
    public partial class NodeManagerViewModel : ViewModel
    {
        private readonly INodeService _nodeService;

        [ObservableProperty]
        private ObservableCollection<NodeModel> _nodes = new ObservableCollection<NodeModel>();

        public Array TargetTypes { get; } = Enum.GetValues(typeof(TargetType));

        private TargetType _selectedTarget;
        public TargetType SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                _selectedTarget = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTarget)));
            }
        }

        public ObservableCollection<ItemModel> Items { get; } = new();
      

        public event PropertyChangedEventHandler PropertyChanged;


        public NodeManagerViewModel()
        {
            _nodeService = Ioc.Default.GetRequiredService<INodeService>();
        }

        internal async Task InitializeAsync(DispatcherQueue dispatcher)
        {
            try
            {
                WeakReferenceMessenger.Default.Register<NodeEventMessage>(this, (r, m) => OnNodeEvent(m.Value));

                _nodeService.GetAllNodes().ForEach(node => {
                    dispatcher.TryEnqueue(() =>
                    {
                        //Nodes.Add(node);
                        Items.Add(new ItemModel 
                        { 
                            UniqueId = node.UniqueId,
                            TargetType = node.TargetType, 
                            Position=node.Position, 
                            TargetId=node.TargetId,
                            Distance = node.Distance,
                            Status = "offline"
                        });
                        
                    });
                });
            }
            catch(Exception ex)
            {
                // Log exception
                System.Diagnostics.Trace.WriteLine($"Error initializing NodeManagerViewModel: {ex.Message}");
            }
        }

        private void OnNodeEvent(NodeEvent value)
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
                    Distance = node.Distance
                });
            }

            _nodeService.UpdateNodes(nodesToSave);
        }

        internal void Clear()
        {
            Items.Clear();
        }
    }
}
