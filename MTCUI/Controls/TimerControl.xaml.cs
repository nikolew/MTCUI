using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MTCUI.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI.Controls
{
    public sealed partial class TimerControl : UserControl
    {
        public TimerViewModel TimerVM { get; set; }

        public TimerControl()
        {
            InitializeComponent();

            TimerVM = Ioc.Default.GetRequiredService<TimerViewModel>();
        }
    }
}
