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
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NodeManagerWindow : Window, IInitializableWindow
{
    private bool centered;
    public NodeManagerViewModel NodeManagerVM;

    public NodeManagerWindow()
    {
        InitializeComponent();
        
        this.Activated += MainWindow_Activated;

        NodeManagerVM = Ioc.Default.GetRequiredService<NodeManagerViewModel>();

        RootGrid.DataContext = NodeManagerVM;

        Closed += (s, e) =>
        {
            NodeManagerVM.Clear();
        };
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        
        
        if (this.centered is false)
        {
            Center(this);
            centered = true;
        }
    }

    private static void Center(Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

        if (AppWindow.GetFromWindowId(windowId) is AppWindow appWindow &&
            DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest) is DisplayArea displayArea)
        {
            appWindow.Resize(new SizeInt32(1700, 800));
            PointInt32 CenteredPosition = appWindow.Position;
            CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(CenteredPosition);
        }
    }

    public Task InitializeAsync(DispatcherQueue dispatcher)
    {
        return NodeManagerVM.InitializeAsync(dispatcher);
    }
}
