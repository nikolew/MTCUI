using CommunityToolkit.Mvvm.DependencyInjection;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GroupManagerWindow : Window, IInitializableWindow
    {
        private bool _centered;
        private readonly AppWindow _appWindow;

        public GroupManagerViewModel GroupManagerVm { get; set; }

        public GroupManagerWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            this.Activated += MainWindow_Activated;

            GroupManagerVm = Ioc.Default.GetRequiredService<GroupManagerViewModel>();

            RootGrid.DataContext = GroupManagerVm;

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            _appWindow.Closing += AppWindow_Closing;

            Closed += (s, e) =>
            {

            };
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_centered is false)
            {
                Center(this);
                _centered = true;
            }
        }

        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            try
            {
            }
            catch (Exception e)
            {

            }
        }

        public Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            return GroupManagerVm.InitializeAsync(dispatcher);
        }

        private static void Center(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            var appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow is null ||
                DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest) is not { } displayArea)
                return;

            appWindow.Resize(new SizeInt32(700, 800));
            var centeredPosition = appWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            centeredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(centeredPosition);
        }

        private void Picker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            SelectedBrush.Color = args.NewColor;
        }
    }
}
