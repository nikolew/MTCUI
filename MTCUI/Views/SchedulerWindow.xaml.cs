using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using MTCUI.Services;
using MTCUI.ViewModels;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MTCUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SchedulerWindow : Window, IInitializableWindow
    {
        private bool _centered;
        private readonly AppWindow _appWindow;
        
        public readonly SchedulerViewModel SchedulerVm;

        private bool _isFormattingTime;

        public SchedulerWindow()
        {
            InitializeComponent();
            
            Activated += Window_Activated;

            SchedulerVm = Ioc.Default.GetRequiredService<SchedulerViewModel>();

            RootGrid.DataContext = SchedulerVm;

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            _appWindow.Closing += AppWindow_Closing;
        
            Closed += (s, e) =>
            {
                
            };
        }

        private static void Center(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            var appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow is null ||
                DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest) is not { } displayArea) 
                return;
        
            appWindow.Resize(new SizeInt32(1000, 750));
            var centeredPosition = appWindow.Position;
            centeredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            centeredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(centeredPosition);
        }
        
        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_centered is true) 
                return;
            Center(this);
            _centered = true;
        }

        public Task InitializeAsync(DispatcherQueue dispatcher, object o)
        {
            return SchedulerVm.InitializeAsync(dispatcher);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (_isFormattingTime)
                return;

            var original = sender.Text ?? string.Empty;
            var caret = sender.SelectionStart;

            // колко цифри има преди caret-а (за да върнем caret логично)
            int digitsBeforeCaret = original.Take(caret).Count(char.IsDigit);

            // само цифри (макс 4 за HHmm)
            var digits = new string(original.Where(char.IsDigit).Take(4).ToArray());

            // Форматираме към HH:mm (частично, докато се пише)
            string formatted = FormatPartialHHmm(digits);

            // Валидация: ограничава часовете/минутите (без да пречи докато се пише)
            formatted = ClampHHmm(formatted);

            // Ако няма промяна – нищо
            if (formatted == original)
                return;

            // Нов caret: след същия брой въведени цифри
            int newCaret = CaretFromDigitIndex(formatted, digitsBeforeCaret);

            _isFormattingTime = true;
            try
            {
                sender.Text = formatted;
                sender.SelectionStart = Math.Min(newCaret, formatted.Length);
            }
            finally
            {
                _isFormattingTime = false;
            }

            // синхронизирай VM веднага
            if (RootGrid.DataContext is SchedulerViewModel vm)
                vm.NewTime = formatted;
        }

        private static string FormatPartialHHmm(string digits)
        {
            // "" -> ""
            // "0" -> "0"
            // "00" -> "00:"
            // "001" -> "00:1"
            // "0012" -> "00:12"
            if (string.IsNullOrEmpty(digits))
                return string.Empty;

            if (digits.Length <= 2)
                return digits.Length == 2 ? digits + ":" : digits;

            // 3-4 цифри
            return digits.Insert(2, ":");
        }

        private static string ClampHHmm(string text)
        {
            // работим само ако имаме поне HH
            // приемаме форматиран текст като "H", "HH:", "HH:m", "HH:mm"
            var digits = new string(text.Where(char.IsDigit).ToArray());

            if (digits.Length < 2)
                return text;

            // HH
            int hh = int.Parse(digits.Substring(0, 2));
            hh = Math.Clamp(hh, 0, 59);

            string result;

            if (digits.Length == 2)
            {
                result = $"{hh:00}:";
                // ако потребителят още не е стигнал до 2 цифри, не насилваме ':'
                // но тук digits.Length==2 значи вече има 2 цифри -> ':' е ок
                return result;
            }

            // ако има минути (1 или 2 цифри)
            string mmDigits = digits.Length >= 3 ? digits.Substring(2, Math.Min(2, digits.Length - 2)) : "";
            int mm;
            if (mmDigits.Length == 0)
            {
                result = $"{hh:00}:";
                return result;
            }
            else if (mmDigits.Length == 1)
            {
                // още се пише, clamp на първата цифра не е нужен
                result = $"{hh:00}:{mmDigits}";
                return result;
            }
            else
            {
                mm = int.Parse(mmDigits);
                mm = Math.Clamp(mm, 0, 59);
                result = $"{hh:00}:{mm:00}";
                return result;
            }
        }

        private static int CaretFromDigitIndex(string formatted, int digitIndex)
        {
            // digitIndex = колко цифри трябва да са "вляво" от caret-а
            // намираме позицията в formatted след digitIndex цифри
            if (digitIndex <= 0)
                return 0;

            int digitsSeen = 0;
            for (int i = 0; i < formatted.Length; i++)
            {
                if (char.IsDigit(formatted[i]))
                {
                    digitsSeen++;
                    if (digitsSeen == digitIndex)
                        return i + 1; // caret след тази цифра
                }
            }

            return formatted.Length;
        }
    }
}
