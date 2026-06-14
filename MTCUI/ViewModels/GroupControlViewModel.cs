using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MTCCore.Messages.Groups;
using MTCCore.Protocol;
using MTCCore.Services.Groups;
using MTCCore.Services.Nodes;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MTCUI.ViewModels
{
    public partial class GroupControlViewModel : ViewModel
    {
        private readonly IGroupService _groupService;
        private readonly INodeService _nodeService;

        [ObservableProperty]
        private ObservableCollection<GroupModel> _groups = new();

        public GroupControlViewModel(IGroupService groupService, INodeService nodeService)
        {
            _groupService = groupService;
            _nodeService = nodeService;

            WeakReferenceMessenger.Default.Register<UpdateGroupMessage>(this, (r, m) =>
            {
                LoadGroups();
            });

            LoadGroups();
        }


        private void LoadGroups()
        {
            Groups.Clear();
            var data = _groupService.GetAllAsync().Result;
            foreach (var dto in data)
            {
                if (dto.Name != "None")
                {
                    var gr = new GroupModel(dto);
                    gr.SelectGroupAction += SelectGroup;
                    Groups.Add(gr);
                }
            }
        }

        [RelayCommand]
        public void SelectGroup(GroupModel groupModel)
        {
            var packet = new Envelope
            {
                Seq = 1,
                TsMs = (uint)Environment.TickCount,
                SendNodeCommandReq = new SendNodeCommandReq
                {
                    NodeId = 0xFE, // broadcast
                    NodeCommand = NodeCommand.CMD_GROUP,
                    Param = new byte[] { (byte)groupModel.Id }
                }
            };

            _nodeService.NodeCommand(packet);
        }
    }
}
