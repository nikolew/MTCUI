using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCUI.Models
{
    public partial class NodeInfo : ObservableObject
    {
        [ObservableProperty]
        private string _uniqueId;

        [ObservableProperty]
        private int _nodeId;

        [ObservableProperty]
        private string _group;

        [ObservableProperty]
        private string _targetType;

        [ObservableProperty]
        private string _lightMode;

        [ObservableProperty]
        private string _rssi;

        [ObservableProperty]
        private string _snr;

        [ObservableProperty]
        private string _battVoltage;

        [ObservableProperty]
        private int _baterrySoc;
    }
}
