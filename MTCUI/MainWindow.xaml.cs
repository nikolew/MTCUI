using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MTCUI.Services;
using MTCUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;
using WinRT.Interop;
using AppWindow = Microsoft.UI.Windowing.AppWindow;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, IInitializableWindow
    {
        public MainViewModel MainViewModel { get; set; }

        private readonly IWindowService _windowService;


        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            MainViewModel = Ioc.Default.GetRequiredService<MainViewModel>();
            RootGrid.DataContext = MainViewModel;

            _windowService = Ioc.Default.GetRequiredService<IWindowService>();

            var appWindow = GetAppWindow(this);
            var presenter = appWindow.Presenter as OverlappedPresenter;
            presenter.Maximize();

            Closed += (s, e) => _windowService.CloseAll();
        }


        public static AppWindow GetAppWindow(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        public async Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            await MainViewModel.InitializeAsync(dispatcher, o);
            await TimerPanel.TimerVM.InitializeAsync(dispatcher);
            
        }

        private void NodeManager_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ConfigNodeCommand.Execute(null);
        }

        private void LoadNodes_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.LoadCommand.Execute(null);
        }

        private async void SaveScene_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.SaveSceneCommand.Execute(null);
            await MainViewModel.ShowNotificationAsync("Запис", "Сцената беше записана успешно!", InfoBarSeverity.Success);
        }

        private void Scheduler_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel?.SchedulerCommand.Execute(null);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ResetNodesCommand.Execute(null);
            
        }

        private void ShowTimer_Click(object sender, RoutedEventArgs e)
        {
            if (TimerPanel.Visibility == Visibility.Visible)
            { 
                TimerPanel.Visibility = Visibility.Collapsed;
                return;
            }

            TimerPanel.Visibility = Visibility.Visible;
        }

        private void ShowGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupPanel.Visibility == Visibility.Visible)
            {
                GroupPanel.Visibility = Visibility.Collapsed;
                return;
            }

            GroupPanel.Visibility = Visibility.Visible;
        }

        private async void ResetMaster_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ResetMasterCommand.Execute(null);
           // await MainViewModel.ShowNotificationAsync("Нулиране", "Системата беше рестартирана.", InfoBarSeverity.Warning);
        }
    }
}
