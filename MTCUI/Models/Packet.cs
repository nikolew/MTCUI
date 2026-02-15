using ProtoBuf;

namespace MTCUI.Models;

[ProtoContract]
public enum CommandType {
    [ProtoEnum] CMD_UNDEFINED = 0,

    [ProtoEnum] CMD_PING      = 1,
    [ProtoEnum] CMD_STATUS    = 2,
    [ProtoEnum] CMD_CONTROL   = 3,
    [ProtoEnum] CMD_ADDNODE   = 4,
    [ProtoEnum] CMD_GETNODE   = 5,
    [ProtoEnum] CMD_GETNODES  = 6,
    [ProtoEnum] CMD_SERVER    = 7,
    [ProtoEnum] CMD_NODECMD   = 8,
    [ProtoEnum] CMD_NODEDEL   = 9,
    [ProtoEnum] CMD_PROVON    = 10,
    [ProtoEnum] CMD_PROVOFF   = 11,
    [ProtoEnum] CMD_NODEEVENT = 12,
    [ProtoEnum] CMD_NODERST   = 13,

    [ProtoEnum] CMD_ERROR     = 100
}
 
[ProtoContract]
public class Node {
    [ProtoMember(1)]public byte[] UniqueId { get ; set; }
    [ProtoMember(2)]public int NodeId { get; set; }
}

[ProtoContract]
public class Response {
    [ProtoMember(1)] public int Status { get; set; }
    [ProtoMember(2)] public string Message { get; set; }
}

[ProtoContract]
public class NodeList {
    [ProtoMember(1)] public Node[] Nodes { get; set; }
}

[ProtoContract]
public class Command
{
     
}

[ProtoContract]
public class NodeStatus
{
    [ProtoMember(1)] public int Id { get; set; }
    [ProtoMember(2)] public int Position { get; set; }
    [ProtoMember(3)] public int State { get; set; }
    [ProtoMember(4)] public byte[] BattVoltage { get; set; }
    [ProtoMember(5)] public int BattState { get; set; }
    [ProtoMember(6)] public int Rssi { get; set; }
    [ProtoMember(7)] public int Snr { get; set; }
}
 
[ProtoContract]
public class NodeEvent
{
    [ProtoMember(1)] public int Id { get; set; }
    [ProtoMember(2)] public bool Online { get; set; }
    [ProtoMember(3)] public int MissedFrames { get; set; }
    [ProtoMember(4)] public int LastSeenMs { get; set; }
}

[ProtoContract] 
public class Packet
{
    [ProtoMember(1)] public CommandType CommandType { get; set; }
    [ProtoMember(2)] public int MessageId { get; set; }
    [ProtoMember(3)] public Node Node { get; set; }
    [ProtoMember(4)] public Response Response { get; set; }
    [ProtoMember(5)] public NodeList NodeList { get; set; }
    [ProtoMember(6)] public NodeStatus NodeStatus { get; set; }
    [ProtoMember(7)] public NodeEvent NodeEvent { get; set; }
}