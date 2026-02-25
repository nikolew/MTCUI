using System;

namespace MTCCore.Protocol.Events
{
    public class NodeListReceivedEventArgs : EventArgs
    {
        public NodeList NodeList { get; set; }
        public NodeListReceivedEventArgs(NodeList nodeList)
        {
            NodeList = nodeList;
        }
    }
}
