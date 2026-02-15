using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Enums
{
    public enum TargetType
    {
        Default,
        Target6,
        Target7,
        Target8,
        Target8A,
        Target9,
        Target10,
        Target10A,
        Target13,
        Target14
    }

    public enum TargetState
    {
        TargetFolded,
        TargetRaised,
        TargetRaising,
        TargetRaisedHit,
        TargetFolding,
        TargetFoldedHit,
        TargetHit,
        TargetOffline
    }

    public enum Group
    {
        Group1,
        Group2,
        Group3,
        Group4,
        Group5,
        Group6,
        Group7,
        Group8
    }

    public enum LightMode
    {
        Off,
        On,
        Blink1,
        Blink2
    }
}
