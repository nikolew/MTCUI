using ProtoBuf;
using System;

namespace MTCCore.Protocol;

[ProtoContract]
public enum StatusCode
{
    [ProtoEnum] STATUS_OK = 0,
    [ProtoEnum] STATUS_ERR_UNKNOWN = 1,
    [ProtoEnum] STATUS_ERR_NOT_FOUND = 2,
    [ProtoEnum] STATUS_ERR_INVALID_ARG = 3,
    [ProtoEnum] STATUS_ERR_TABLE_FULL = 4,
    [ProtoEnum] STATUS_ERR_LORA_TX_FAIL = 5,
    [ProtoEnum] STATUS_ERR_NO_ACK = 6,
    [ProtoEnum] STATUS_ERR_BUSY = 7,
}

[ProtoContract]
public enum NodeCommand
{
    [ProtoEnum] CMD_NOP = 0,
    [ProtoEnum] CMD_RESET = 1,
    [ProtoEnum] CMD_LIGHT_TYPE = 2,   // param[0..1] = нов интервал в секунди (uint16 LE)
    [ProtoEnum] CMD_GPIO_SET = 3,     // param[0] = пин, param[1] = стойност (0/1)
    [ProtoEnum] CMD_GPIO_GET = 4,     // param[0] = пин
    [ProtoEnum] CMD_SLEEP = 5,       // param[0..3] = sleep duration ms (uint32 LE)
    [ProtoEnum] CMD_GROUP = 6,       // Групова команда
    [ProtoEnum] CMD_CONFIG = 7,
    [ProtoEnum] CMD_READCONFIG = 8
}

[ProtoContract]
public class LoraParams
{
    int frequency_hz = 1;   // напр. 868000000
    int spreading_factor = 2; // 7..12
    int bandwidth_khz = 3;   // 125, 250, 500
    int coding_rate = 4;   // 1=4/5 .. 4=4/8
    int tx_power_dbm = 5;
    int preamble_len = 6;
    int frame_period_ms = 7;
}

[ProtoContract]
public class PingReq
{
    [ProtoMember(1)]
    public uint Payload { get; set; }
}

[ProtoContract]
public class PongResp
{
    [ProtoMember(1)]
    public uint Payload { get; set; }

    [ProtoMember(2)]
    public uint UptimeMs { get; set; }

    [ProtoMember(3)]
    public uint FreeHeap { get; set; }

    [ProtoMember(4)]
    public uint BleConnMs { get; set; }
}

[ProtoContract]
public class GetNetworkStatusReq
{
    
}

[ProtoContract]
public class ResetMasterReq
{
    [ProtoMember(1)]
    public int DelayMs { get; set; }
}

[ProtoContract]
public class ConfigAckResp
{
    [ProtoMember(1)]
    public StatusCode Status { get; set; }

    [ProtoMember(2)]
    public int ReqSeq { get; set; }

    [ProtoMember(3)]
    public string Message { get; set; }
}



[ProtoContract]
public class NetworkStatusResp
{
    [ProtoMember(1)]
    StatusCode Status { get; set; }

    [ProtoMember(2)]
    int Frame_number { get; set; }

    [ProtoMember(3)]
    int Frame_period_ms { get; set; }

    [ProtoMember(4)]
    int Active_nodes { get; set; }

    [ProtoMember(5)]
    int Active_mask { get; set; }   // бит N = node N активен

    [ProtoMember(6)]
    int Master_uptime_ms { get; set; }

    [ProtoMember(7)]
    int Ble_rssi { get; set; }   // RSSI на BLE връзката

    [ProtoMember(8)]
    LoraParams Lora_params { get; set; }   // текущи LoRa параметри

    [ProtoMember(9)]
    int Total_frames { get; set; }

    [ProtoMember(10)]
    int Total_rx_packets { get; set; }
}

[ProtoContract]
public class NodeInfo
{
    [ProtoMember(1)]
    public int NodeId { get; set; }

    [ProtoMember(2)]
    public byte[] Uid { get; set; }

    [ProtoMember(3)]
    public int SlotOffsetMs { get; set; }

    [ProtoMember(4)]
    public int FwVersion { get; set; }

    [ProtoMember(5)]
    public int Capabilities { get; set; }

    [ProtoMember(6)]
    public int LastRssi { get; set; }

    [ProtoMember(7)]
    public int LastSnr { get; set; }

    [ProtoMember(8)]
    public int LastDriftMs { get; set; }

    [ProtoMember(9)]
    public int LastSeenMs { get; set; }

    [ProtoMember(10)]
    public int MissedFrames { get; set; }

    [ProtoMember(11)]
    public int TotalRx { get; set; }

    [ProtoMember(12)]
    public bool Active { get; set; }
}

[ProtoContract]
public class GetNodeListReq
{
    [ProtoMember(1)]
    public bool ActiveOnly { get; set; }
}

[ProtoContract]
public class NodeListResp
{
    [ProtoMember(1)]
    public StatusCode Status { get; set; }

    [ProtoMember(2)]
    public NodeInfo[] Nodes { get; set; }
}

[ProtoContract]
public class SendNodeCommandReq
{
    [ProtoMember(1)]
    public int NodeId { get; set; }

    [ProtoMember(2)]
    public NodeCommand NodeCommand { get; set; }

    [ProtoMember(3)]
    public byte[] Param { get; set; }
}

[ProtoContract]
public class SensorReading
{
    [ProtoMember(1)]
    public int Position { get; set; }

    [ProtoMember(2)]
    public int State { get; set; }

    [ProtoMember(3)]
    public float VoltageMv { get; set; }

    [ProtoMember(4)]
    public int GpioState { get; set; }

    [ProtoMember(5)]
    public int StatusFlags { get; set; }

    [ProtoMember(6)]
    public int UptimeMs { get; set; }

    [ProtoMember(7)]
    public int Soc { get; set; }
}

[ProtoContract]
public class NodeDataEvent
{
    [ProtoMember(1)]
    public int NodeId { get; set; }

    [ProtoMember(2)]
    public int Seq { get; set; }

    [ProtoMember(3)]
    public SensorReading Reading { get; set; }

    [ProtoMember(4)]
    public int Rssi { get; set; }

    [ProtoMember(5)]
    public int Snr { get; set; }

    [ProtoMember(6)]
    public int DriftMs { get; set; }
}

[ProtoContract]
public class ConfigNodeResp
{
    [ProtoMember(1)]
    public int NodeId { get; set; }

    [ProtoMember(2)]
    public int GroupId { get; set; }

    [ProtoMember(3)]
    public int Light { get; set; }
}

[ProtoContract]
public class NodeStatusEvent
{
    public enum ChangeType
    {
        REGISTERED = 0,
        DEREGISTERED = 1,
        TIMEOUT = 2,
        RECONNECTED = 3
    }

    [ProtoMember(1)]
    public int NodeId { get; set; }


    [ProtoMember(2)]
    public ChangeType Change_Type { get; set; }


    [ProtoMember(3)]
    public NodeInfo Info { get; set; }
}

[ProtoContract]
public class Envelope
{
    [ProtoMember(1)]
    public uint Seq { get; set; }

    [ProtoMember(2)]
    public uint TsMs { get; set; }

    [ProtoMember(10)]
    public PingReq Ping { get; set; }

    [ProtoMember(11)]
    public GetNetworkStatusReq GetNetworkStatus { get; set; }

    [ProtoMember(12)]
    public GetNodeListReq GetNodeList { get; set; }

    [ProtoMember(13)]
    public SendNodeCommandReq SendNodeCommandReq { get; set; }

    [ProtoMember(14)]
    public ResetMasterReq ResetMaster { get; set;}




    [ProtoMember(30)]
    public PongResp Pong { get; set; }

    [ProtoMember(31)]
    public NetworkStatusResp NetworkStatus { get; set; }

    [ProtoMember(32)]
    public NodeListResp NodeList { get; set; }

    [ProtoMember(33)]
    public NodeDataEvent NodeData { get; set; }

    [ProtoMember(34)]
    public ConfigAckResp ConfigAck { get; set; }

    [ProtoMember(35)]
    public ConfigNodeResp ConfigNode { get; set; }

    [ProtoMember(36)]
    public NodeStatusEvent NodeStatus { get; set; }
}

