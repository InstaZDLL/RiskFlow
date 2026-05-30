using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace RiskFlow
{
    /// <summary>Fenêtre principale : héberge la page du registre des risques.</summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow(MainPage page)
        {
            InitializeComponent();
            RootContainer.Children.Add(page);
            SetWindowIcon();
        }

        /// <summary>Applique l'icône RiskFlow à la fenêtre (barre de titre + barre des tâches).</summary>
        private void SetWindowIcon()
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("Assets/RiskFlow.ico");
        }
    }
}
