using MTCCore.Models;

namespace MTCCore.Messages.Nodes
{
    public record NodeSendCommandMessage(string Id);
    public record NodeAddToViewGraphMessage(NodeModel Node);
    public record NodeUpdateStatusMessage(NodeModel Node);
    public record NodeUpdateMessage(string Id);
    public record NodeEventMessage(NodeEventModel NodeEvent);
    public record NodeSetConfigMessage(NodeConfigModel NodeConfig);
    public record NodeGetConfigMessage(NodeConfigModel NodeConfig);
}
