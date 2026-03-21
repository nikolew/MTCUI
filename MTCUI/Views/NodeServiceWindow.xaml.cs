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
using MTCUI.Models;
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

namespace MTCUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NodeServiceWindow : Window, IInitializableWindow
{
    private bool _centered;
    private readonly AppWindow _appWindow;

    NodeServiceViewModel _nodeServiceVM;

    public NodeServiceWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _nodeServiceVM = Ioc.Default.GetRequiredService<NodeServiceViewModel>();

        RootGrid.DataContext = _nodeServiceVM;

        Activated += MainWindow_Activated;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        _appWindow.Closing += AppWindow_Closing;

        Closed += (s, e) =>
        {
            _nodeServiceVM.Nodes.Clear();
        };
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_centered is false)
        {
            Center(this);
            _centered = true;
        }
    }

    public Task InitializeAsync(DispatcherQueue dispatcher, object o)
    {
        return _nodeServiceVM.InitializeAsync(dispatcher);
    }

    private static void Center(Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow is null ||
            DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest) is not { } displayArea)
            return;

        appWindow.Resize(new SizeInt32(1700, 800));
        var centeredPosition = appWindow.Position;
        centeredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
        centeredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
        appWindow.Move(centeredPosition);
    }

    private void ConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int id)
        {
            var node = _nodeServiceVM.Nodes.FirstOrDefault(n => n.NodeId == id);
            if (node != null) 
                ShowConfigPanel(node);
        }
    }

    private void ShowConfigPanel(NodeInfo node)
    {
        _nodeServiceVM.SelectedNode = node;
        ConfigPanel.Visibility = Visibility.Visible;
        ConfigColumn.Width = new GridLength(320);

        DevEUIRow.Value = node.UniqueId;
    }

    private void CloseConfig_Click(object sender, RoutedEventArgs e)
    {
        ConfigPanel.Visibility = Visibility.Collapsed;
        ConfigColumn.Width = new GridLength(0);
        _nodeServiceVM.SelectedNode = null;
    }

    private void NodeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NodeListView.SelectedItem is NodeInfo node)
            ShowConfigPanel(node);
    }
}
