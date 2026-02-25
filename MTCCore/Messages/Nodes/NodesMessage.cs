using MTCCore.DTO.Nodes;
using MTCCore.Models;
using MTCCore.Protocol;
using System.Collections.Generic;

namespace MTCCore.Messages.Nodes
{
    public record NodeSendCommandMessage(int Id, CommandType CommandType);
    public record NodeAddToViewGraphMessage(NodeModel Node);
    public record NodeUpdateStatusMessage(NodeModel Node);
    public record NodeUpdateMessage(string Id);
    public record NodeEventMessage(NodeEventModel NodeEvent);
    public record NodeSetConfigMessage(NodeConfigModel NodeConfig);
    public record NodeGetConfigMessage(NodeConfigModel NodeConfig);

    public record NodeListRequestMessage(List<ReadNodeDto> NodeListRequest);
}
