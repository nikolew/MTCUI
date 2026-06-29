using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCUI.Models
{
    public partial class SoundItem : ObservableObject
    {
        public string Name { get; }
        public string FileName { get; }

        [ObservableProperty]
        private string glyph = "\uE768";

        public SoundItem(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
        }
    }
}
