using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MTCCore.Extensions;
using MTCUI.Services;
using MTCUI.ViewModels;
using MTCUI.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        public IConfiguration Configuration { get; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton<MainViewModel>()
                .AddSingleton<GraphViewModel>()
                .AddSingleton<NodeViewModel>()
                .AddSingleton<NodeManagerViewModel>()
                .AddTransient<NodeManagerWindow>()
                .AddSingleton<MainWindow>()
                .AddSingleton<CoreService>()
                .AddSingleton<ClientSocketService>()
                .AddSingleton<ITimerWorker, TimerWorker>()
                .AddSingleton<BluetoothLEService>()
                .AddSingleton<IWindowService, WindowService>()

                .AddDependencyInjection(Configuration)
                .BuildServiceProvider());

            
        }



        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var windowService = Ioc.Default.GetRequiredService<IWindowService>();
            windowService.OpenWindow<MainWindow>();

        }
    }
}
