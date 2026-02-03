using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MTCUI.ViewModels;
using MTCUI.Views;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinRT.Interop;

namespace MTCUI.Services
{
    public interface IWindowService
    {
        void CloseAll();
        T OpenWindow<T>() where T : Window;
    }

    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<Type, Window> _windows = new();

        public WindowService(IServiceProvider services)
        {
            _services = services;
        }

        public T OpenWindow<T>() where T : Window
        {
            // 1) Проверка за съществуващ прозорец
            if (_windows.TryGetValue(typeof(T), out var existing))
            {
                // 2) Проверка дали прозорецът е още жив
                if (IsWindowAlive(existing))
                {
                    existing.Activate();
                    return (T)existing;
                }

                // 3) Ако е мъртъв → премахваме го
                _windows.Remove(typeof(T));
            }

            // 4) Създаваме нов прозорец
            var window = _services.GetRequiredService<T>();
            _windows[typeof(T)] = window;

            window.Closed += (s, e) =>
            {
                _windows.Remove(typeof(T));
            };

            window.Activate();
            //MakeWindowTopMost(window);


            // 5) Автоматична инициализация
            if (window is IInitializableWindow init)
            {
                var dispatcher = window.DispatcherQueue;

                _ = dispatcher.EnqueueAsync(async () =>
                {
                    await init.InitializeAsync(dispatcher);
                });
            }

            return window;
        }

        private bool IsWindowAlive(Window window)
        {
            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                return hWnd != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }

        public void CloseAll()
        {
            foreach (var w in _windows.Values.ToList())
                w.Close();

            _windows.Clear();
        }

        public static void MakeWindowTopMost(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;
            }
        }

    }



}
