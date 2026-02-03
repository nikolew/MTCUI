using CommunityToolkit.Mvvm.Messaging.Messages;
using MTCCore.Models;
using MTCUI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCUI.Messages
{
    public class ClientConnectMessage(string s) : ValueChangedMessage<string>(s) { }

    public class NodeClickMessage(string nodeId) : ValueChangedMessage<string>(nodeId) { }

    public class CommandMessage(int command) : ValueChangedMessage<int>(command) { }

    public class AddNodeToViewGraphMessage(NodeModel node) : ValueChangedMessage<NodeModel>(node) { }

    public class UpdateNodeStatusMessage(NodeModel node) : ValueChangedMessage<NodeModel>(node) { }

}
