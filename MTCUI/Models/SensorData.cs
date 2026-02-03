using ProtoBuf;

namespace MTCUI.Models;

[ProtoContract]
public class SensorData
{
    [ProtoMember(1)] public float Temperature { get; set; }
    [ProtoMember(2)] public float Humidity { get; set; }
    [ProtoMember(3)] public uint Timestamp { get; set; }
}