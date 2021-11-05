using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace CopyCat.Wpf.Helpers
{
    class BindableHost : INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        private const int GWL_STYLE = (-16);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        // Window Styles 
        const UInt32 WS_OVERLAPPED = 0;
        const UInt32 WS_POPUP = 0x80000000;
        const UInt32 WS_CHILD = 0x40000000;
        const UInt32 WS_MINIMIZE = 0x20000000;
        const UInt32 WS_VISIBLE = 0x10000000;
        const UInt32 WS_DISABLED = 0x8000000;
        const UInt32 WS_CLIPSIBLINGS = 0x4000000;
        const UInt32 WS_CLIPCHILDREN = 0x2000000;
        const UInt32 WS_MAXIMIZE = 0x1000000;
        const UInt32 WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
        const UInt32 WS_BORDER = 0x800000;
        const UInt32 WS_DLGFRAME = 0x400000;
        const UInt32 WS_VSCROLL = 0x200000;
        const UInt32 WS_HSCROLL = 0x100000;
        const UInt32 WS_SYSMENU = 0x80000;
        const UInt32 WS_THICKFRAME = 0x40000;
        const UInt32 WS_GROUP = 0x20000;
        const UInt32 WS_TABSTOP = 0x10000;
        const UInt32 WS_MINIMIZEBOX = 0x20000;
        const UInt32 WS_MAXIMIZEBOX = 0x10000;
        const UInt32 WS_TILED = WS_OVERLAPPED;
        const UInt32 WS_ICONIC = WS_MINIMIZE;
        const UInt32 WS_SIZEBOX = WS_THICKFRAME;

        public event PropertyChangedEventHandler PropertyChanged;

        private UserControl _hostView;
        private Process _process;

        public BindableHost()
        {

        }

        public void SetHostView(UserControl hostView, string exeName)
        {
            _hostView = hostView;

            if (exeName.EndsWith(".exe") == false)
                exeName += ".exe";

            IntPtr handle = ((HwndSource)PresentationSource.FromVisual(hostView)).Handle;

            var info = new ProcessStartInfo(exeName);
            info.UseShellExecute = true;
            info.WindowStyle = ProcessWindowStyle.Minimized;

            _process = Process.Start(info);
            _process.WaitForInputIdle();

            var process = _process;
            while (process.MainWindowHandle == IntPtr.Zero)
                process = Process.GetProcessById(_process.Id);

            _process = process;

            SetParent(_process.MainWindowHandle, handle);
            SetWindowLong(_process.MainWindowHandle, GWL_STYLE, WS_VISIBLE);
            Refresh();
        }

        public void Refresh()
        {
            if (_hostView == null || _process == null)
                return;

            var window = Window.GetWindow(_hostView);
            Point coordinates = _hostView.TransformToAncestor(window).Transform(new Point(0, 0));

            MoveWindow(_process.MainWindowHandle, (int)coordinates.X, (int)coordinates.Y, (int)_hostView.ActualWidth, (int)_hostView.ActualHeight, true);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;

            if (eventHandler != null)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
