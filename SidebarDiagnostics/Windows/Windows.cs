using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using SidebarDiagnostics.Style;

namespace SidebarDiagnostics.Windows
{
    public enum WinOS : byte
    {
        Unknown,
        Other,
        Win7,
        Win8,
        Win8_1,
        Win10
    }

    public static class OS
    {
        private static WinOS _os { get; set; } = WinOS.Unknown;

        public static WinOS Get
        {
            get
            {
                if (_os != WinOS.Unknown)
                {
                    return _os;
                }

                Version _version = Environment.OSVersion.Version;

                if (_version.Major >= 10)
                {
                    _os = WinOS.Win10;
                }
                else if (_version.Major == 6 && _version.Minor == 3)
                {
                    _os = WinOS.Win8_1;
                }
                else if (_version.Major == 6 && _version.Minor == 2)
                {
                    _os = WinOS.Win8;
                }
                else if (_version.Major == 6 && _version.Minor == 1)
                {
                    _os = WinOS.Win7;
                }
                else
                {
                    _os = WinOS.Other;
                }

                return _os;
            }
        }

        public static Boolean SupportDPI => OS.Get >= WinOS.Win8_1;

        public static Boolean SupportVirtualDesktop => OS.Get >= WinOS.Win10;
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern Int64 GetWindowLong(IntPtr hwnd, Int32 index);

        [DllImport("user32.dll")]
        internal static extern Int64 GetWindowLongPtr(IntPtr hwnd, Int32 index);

        [DllImport("user32.dll")]
        internal static extern Int64 SetWindowLong(IntPtr hwnd, Int32 index, Int64 newStyle);

        [DllImport("user32.dll")]
        internal static extern Int64 SetWindowLongPtr(IntPtr hwnd, Int32 index, Int64 newStyle);

        [DllImport("user32.dll")]
        internal static extern Boolean SetWindowPos(IntPtr hwnd, IntPtr hwnd_after, Int32 x, Int32 y, Int32 cx, Int32 cy, UInt32 uflags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 RegisterWindowMessage(String msg);

        [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern UIntPtr SHAppBarMessage(Int32 dwMessage, ref AppBarWindow.APPBARDATA pData);

        [DllImport("user32.dll")]
        internal static extern Boolean EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, Monitor.EnumCallback callback, Int32 dwData);

        [DllImport("user32.dll")]
        internal static extern Boolean GetMonitorInfo(IntPtr hMonitor, ref Monitor.MONITORINFO lpmi);

        [DllImport("shcore.dll")]
        internal static extern IntPtr GetDpiForMonitor(IntPtr hmonitor, Monitor.MONITOR_DPI_TYPE dpiType, out UInt32 dpiX, out UInt32 dpiY);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, UInt32 dwFlags);

        [DllImport("user32.dll")]
        internal static extern Boolean RegisterHotKey(IntPtr hwnd, Int32 id, UInt32 modifiers, UInt32 vk);

        [DllImport("user32.dll")]
        internal static extern Boolean UnregisterHotKey(IntPtr hwnd, Int32 id);

        [DllImport("user32.dll")]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, Int32 flags);

        [DllImport("user32.dll")]
        internal static extern Boolean UnregisterDeviceNotification(IntPtr handle);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWinEventHook(UInt32 eventMin, UInt32 eventMax, IntPtr hmodWinEventProc, ShowDesktop.WinEventDelegate lpfnWinEventProc, UInt32 idProcess, UInt32 idThread, UInt32 dwFlags);

        [DllImport("user32.dll")]
        internal static extern Boolean UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern Int32 GetClassName(IntPtr hwnd, StringBuilder name, Int32 count);

        [DllImport("dwmapi.dll")]
        internal static extern Int32 DwmSetWindowAttribute(IntPtr hwnd, AppBarWindow.DWMWINDOWATTRIBUTE dwmAttribute, IntPtr pvAttribute, UInt32 cbAttribute);
    }

    public static class ShowDesktop
    {
        private const UInt32 WINEVENT_OUTOFCONTEXT = 0u;
        private const UInt32 EVENT_SYSTEM_FOREGROUND = 3u;

        private const String WORKERW = "WorkerW";
        private const String PROGMAN = "Progman";

        public static void AddHook(Sidebar sidebar)
        {
            if (IsHooked)
            {
                return;
            }

            IsHooked = true;

            _delegate = new WinEventDelegate(WinEventHook);
            _hookIntPtr = NativeMethods.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _delegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            _sidebar = sidebar;
            _sidebarHwnd = new WindowInteropHelper(sidebar).Handle;
        }

        public static void RemoveHook()
        {
            if (!IsHooked)
            {
                return;
            }

            IsHooked = false;

            NativeMethods.UnhookWinEvent(_hookIntPtr.Value);

            _delegate = null;
            _hookIntPtr = null;
            _sidebar = null;
            _sidebarHwnd = null;
        }

        private static String GetWindowClass(IntPtr hwnd)
        {
            StringBuilder _sb = new StringBuilder(32);
            NativeMethods.GetClassName(hwnd, _sb, _sb.Capacity);
            return _sb.ToString();
        }

        internal delegate void WinEventDelegate(IntPtr hWinEventHook, UInt32 eventType, IntPtr hwnd, Int32 idObject, Int32 idChild, UInt32 dwEventThread, UInt32 dwmsEventTime);

        private static void WinEventHook(IntPtr hWinEventHook, UInt32 eventType, IntPtr hwnd, Int32 idObject, Int32 idChild, UInt32 dwEventThread, UInt32 dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                String _class = GetWindowClass(hwnd);

                if (String.Equals(_class, WORKERW, StringComparison.Ordinal) /*|| string.Equals(_class, PROGMAN, StringComparison.Ordinal)*/ )
                {
                    _sidebar.SetTopMost(false);
                }
                else if (_sidebar.IsTopMost)
                {
                    _sidebar.SetBottom(false);
                }
            }
        }

        public static Boolean IsHooked { get; private set; } = false;

        private static IntPtr? _hookIntPtr { get; set; }

        private static WinEventDelegate _delegate { get; set; }

        private static Sidebar _sidebar { get; set; }

        private static IntPtr? _sidebarHwnd { get; set; }
    }

    public static class Devices
    {
        private const Int32 WM_DEVICECHANGE = 0x0219;

        private static class DBCH_DEVICETYPE
        {
            public const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
            public const Int32 DBT_DEVTYP_HANDLE = 6;
            public const Int32 DBT_DEVTYP_OEM = 0;
            public const Int32 DBT_DEVTYP_PORT = 3;
            public const Int32 DBT_DEVTYP_VOLUME = 2;
        }
        
        private static class FLAGS
        {
            public const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
            public const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
            public const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        }

        private static class WM_DEVICECHANGE_EVENT
        {
            public const Int32 DBT_CONFIGCHANGECANCELED = 0x0019;
            public const Int32 DBT_CONFIGCHANGED = 0x0018;
            public const Int32 DBT_CUSTOMEVENT = 0x8006;
            public const Int32 DBT_DEVICEARRIVAL = 0x8000;
            public const Int32 DBT_DEVICEQUERYREMOVE = 0x8001;
            public const Int32 DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
            public const Int32 DBT_DEVICEREMOVECOMPLETE = 0x8004;
            public const Int32 DBT_DEVICEREMOVEPENDING = 0x8003;
            public const Int32 DBT_DEVICETYPESPECIFIC = 0x8005;
            public const Int32 DBT_DEVNODES_CHANGED = 0x0007;
            public const Int32 DBT_QUERYCHANGECONFIG = 0x0017;
            public const Int32 DBT_USERDEFINED = 0xFFFF;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public Int32 dbch_size;
            public Int32 dbch_devicetype;
            public Int32 dbch_reserved;
        }

        public static void AddHook(Sidebar window)
        {
            if (IsHooked)
            {
                return;
            }

            IsHooked = true;

            DEV_BROADCAST_HDR _data = new DEV_BROADCAST_HDR();
            _data.dbch_size = Marshal.SizeOf(_data);
            _data.dbch_devicetype = DBCH_DEVICETYPE.DBT_DEVTYP_DEVICEINTERFACE;

            IntPtr _buffer = Marshal.AllocHGlobal(_data.dbch_size);
            Marshal.StructureToPtr(_data, _buffer, true);

            IntPtr _hwnd = new WindowInteropHelper(window).Handle;

            NativeMethods.RegisterDeviceNotification(
                _hwnd,
                _buffer,
                FLAGS.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES
                );

            window.HwndSource.AddHook(DeviceHook);
        }

        public static void RemoveHook(Sidebar window)
        {
            if (!IsHooked)
            {
                return;
            }

            IsHooked = false;

            window.HwndSource.RemoveHook(DeviceHook);
        }

        private static IntPtr DeviceHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == WM_DEVICECHANGE)
            {                
                switch (wParam.ToInt32())
                {
                    case WM_DEVICECHANGE_EVENT.DBT_DEVICEARRIVAL:
                    case WM_DEVICECHANGE_EVENT.DBT_DEVICEREMOVECOMPLETE:

                        if (_cancelRestart != null)
                        {
                            _cancelRestart.Cancel();
                        }

                        _cancelRestart = new CancellationTokenSource();

                        Task.Delay(TimeSpan.FromSeconds(1), _cancelRestart.Token).ContinueWith(_ =>
                        {
                            if (_.IsCanceled)
                            {
                                return;
                            }

                            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                            {
                                Sidebar _sidebar = App.Current.Sidebar;

                                if (_sidebar != null)
                                {
                                    _sidebar.ContentReload();
                                }
                            }));

                            _cancelRestart = null;
                        });
                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        public static Boolean IsHooked { get; private set; } = false;

        private static CancellationTokenSource _cancelRestart { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Hotkey
    {
        private const Int32 WM_HOTKEY = 0x0312;

        private static class MODIFIERS
        {
            public const UInt32 MOD_NOREPEAT = 0x4000;
            public const UInt32 MOD_ALT = 0x0001;
            public const UInt32 MOD_CONTROL = 0x0002;
            public const UInt32 MOD_SHIFT = 0x0004;
            public const UInt32 MOD_WIN = 0x0008;
        }

        public enum KeyAction : byte
        {
            Toggle,
            Show,
            Hide,
            Reload,
            Close,
            CycleEdge,
            CycleScreen,
            ReserveSpace
        }

        public Hotkey() { }

        public Hotkey(Int32 index, KeyAction action, UInt32 virtualKey, Boolean altMod = false, Boolean ctrlMod = false, Boolean shiftMod = false, Boolean winMod = false)
        {
            Index = index;
            Action = action;
            VirtualKey = virtualKey;
            AltMod = altMod;
            CtrlMod = ctrlMod;
            ShiftMod = shiftMod;
            WinMod = winMod;
        }

        [JsonProperty]
        public KeyAction Action { get; set; }

        [JsonProperty]
        public UInt32 VirtualKey { get; set; }

        [JsonProperty]
        public Boolean AltMod { get; set; }

        [JsonProperty]
        public Boolean CtrlMod { get; set; }

        [JsonProperty]
        public Boolean ShiftMod { get; set; }

        [JsonProperty]
        public Boolean WinMod { get; set; }

        public Key WinKey
        {
            get => KeyInterop.KeyFromVirtualKey((Int32)VirtualKey);
            set => VirtualKey = (UInt32)KeyInterop.VirtualKeyFromKey(value);
        }

        private Int32 Index { get; set; }

        public static void Initialize(Sidebar window, Hotkey[] settings)
        {
            if (settings == null || settings.Length == 0)
            {
                Dispose();
                return;
            }

            Disable();

            _sidebar = window;
            _index = 0;

            RegisteredKeys = settings.Select(h =>
            {
                h.Index = _index;
                _index++;
                return h;
            }).ToArray();

            window.HwndSource.AddHook(KeyHook);

            IsHooked = true;
        }

        public static void Dispose()
        {
            if (!IsHooked)
            {
                return;
            }

            IsHooked = false;

            Disable();

            RegisteredKeys = null;

            _sidebar.HwndSource.RemoveHook(KeyHook);
            _sidebar = null;
        }

        public static void Enable()
        {
            if (RegisteredKeys == null)
            {
                return;
            }

            foreach (Hotkey _hotkey in RegisteredKeys)
            {
                Register(_hotkey);
            }
        }

        public static void Disable()
        {
            if (RegisteredKeys == null)
            {
                return;
            }

            foreach (Hotkey _hotkey in RegisteredKeys)
            {
                Unregister(_hotkey);
            }
        }

        private static void Register(Hotkey hotkey)
        {
            UInt32 _mods = MODIFIERS.MOD_NOREPEAT;

            if (hotkey.AltMod)
            {
                _mods |= MODIFIERS.MOD_ALT;
            }

            if (hotkey.CtrlMod)
            {
                _mods |= MODIFIERS.MOD_CONTROL;
            }

            if (hotkey.ShiftMod)
            {
                _mods |= MODIFIERS.MOD_SHIFT;
            }

            if (hotkey.WinMod)
            {
                _mods |= MODIFIERS.MOD_WIN;
            }

            NativeMethods.RegisterHotKey(
                new WindowInteropHelper(_sidebar).Handle,
                hotkey.Index,
                _mods,
                hotkey.VirtualKey
                );
        }

        private static void Unregister(Hotkey hotkey)
        {
            NativeMethods.UnregisterHotKey(
                new WindowInteropHelper(_sidebar).Handle,
                hotkey.Index
                );
        }

        public static Hotkey[] RegisteredKeys { get; private set; }
        
        private static IntPtr KeyHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == WM_HOTKEY)
            {
                Int32 _id = wParam.ToInt32();

                Hotkey _hotkey = RegisteredKeys.FirstOrDefault(k => k.Index == _id);

                if (_hotkey != null && _sidebar != null && _sidebar.Ready)
                {
                    switch (_hotkey.Action)
                    {
                        case KeyAction.Toggle:
                            if (_sidebar.Visibility == Visibility.Visible)
                            {
                                _sidebar.AppBarHide();
                            }
                            else
                            {
                                _sidebar.AppBarShow();
                            }
                            break;

                        case KeyAction.Show:
                            _sidebar.AppBarShow();
                            break;

                        case KeyAction.Hide:
                            _sidebar.AppBarHide();
                            break;

                        case KeyAction.Reload:
                            _sidebar.Reload();
                            break;

                        case KeyAction.Close:
                            App.Current.Shutdown();
                            break;

                        case KeyAction.CycleEdge:
                            if (_sidebar.Visibility == Visibility.Visible)
                            {
                                switch (Framework.Settings.Instance.DockEdge)
                                {
                                    case DockEdge.Right:
                                        Framework.Settings.Instance.DockEdge = DockEdge.Left;
                                        break;

                                    default:
                                    case DockEdge.Left:
                                        Framework.Settings.Instance.DockEdge = DockEdge.Right;
                                        break;
                                }

                                Framework.Settings.Instance.Save();

                                _sidebar.Reposition();
                            }
                            break;

                        case KeyAction.CycleScreen:
                            if (_sidebar.Visibility == Visibility.Visible)
                            {
                                Monitor[] _monitors = Monitor.GetMonitors();

                                if (Framework.Settings.Instance.ScreenIndex < (_monitors.Length - 1))
                                {
                                    Framework.Settings.Instance.ScreenIndex++;
                                }
                                else
                                {
                                    Framework.Settings.Instance.ScreenIndex = 0;
                                }

                                Framework.Settings.Instance.Save();

                                _sidebar.Reposition();
                            }
                            break;

                        case KeyAction.ReserveSpace:
                            Framework.Settings.Instance.UseAppBar = !Framework.Settings.Instance.UseAppBar;
                            Framework.Settings.Instance.Save();

                            _sidebar.Reposition();
                            break;
                    }

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public static Boolean IsHooked { get; private set; } = false;

        private static Sidebar _sidebar { get; set; }

        private static Int32 _index { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public Int32 Left;
        public Int32 Top;
        public Int32 Right;
        public Int32 Bottom;

        public Int32 Width => Right - Left;

        public Int32 Height => Bottom - Top;
    }
    
    public class WorkArea
    {
        public Double Left { get; set; }

        public Double Top { get; set; }

        public Double Right { get; set; }

        public Double Bottom { get; set; }

        public Double Width => Right - Left;

        public Double Height => Bottom - Top;

        public void Scale(Double x, Double y)
        {
            Left *= x;
            Top *= y;
            Right *= x;
            Bottom *= y;
        }

        public void Offset(Double x, Double y)
        {
            Left += x;
            Top += y;
            Right += x;
            Bottom += y;
        }

        public void SetWidth(DockEdge edge, Double width)
        {
            switch (edge)
            {
                case DockEdge.Left:
                    Right = Left + width;
                    break;

                case DockEdge.Right:
                    Left = Right - width;
                    break;
            }
        }

        public static WorkArea FromRECT(RECT rect)
        {
            return new WorkArea()
            {
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right,
                Bottom = rect.Bottom
            };
        }
    }

    public class Monitor
    {
        private const UInt32 DPICONST = 96u;

        [StructLayout(LayoutKind.Sequential)]
        internal struct MONITORINFO
        {
            public Int32 cbSize;
            public RECT Size;
            public RECT WorkArea;
            public Boolean IsPrimary;
        }

        internal enum MONITOR_DPI_TYPE : int
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        public RECT Size { get; set; }

        public RECT WorkArea { get; set; }

        public Double DPIx { get; set; }

        public Double ScaleX => DPIx / DPICONST;

        public Double InverseScaleX => 1 / ScaleX;

        public Double DPIy { get; set; }

        public Double ScaleY => DPIy / DPICONST;

        public Double InverseScaleY => 1 / ScaleY;

        public Boolean IsPrimary { get; set; }

        internal delegate Boolean EnumCallback(IntPtr hDesktop, IntPtr hdc, ref RECT pRect, Int32 dwData);

        public static Monitor GetMonitor(IntPtr hMonitor)
        {
            MONITORINFO _info = new MONITORINFO();
            _info.cbSize = Marshal.SizeOf(_info);

            NativeMethods.GetMonitorInfo(hMonitor, ref _info);

            UInt32 _dpiX = Monitor.DPICONST;
            UInt32 _dpiY = Monitor.DPICONST;

            if (OS.SupportDPI)
            {
                NativeMethods.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out _dpiX, out _dpiY);
            }

            return new Monitor()
            {
                Size = _info.Size,
                WorkArea = _info.WorkArea,
                DPIx = _dpiX,
                DPIy = _dpiY,
                IsPrimary = _info.IsPrimary
            };
        }

        public static Monitor[] GetMonitors()
        {
            List<Monitor> _monitors = new List<Monitor>();

            EnumCallback _callback = (IntPtr hMonitor, IntPtr hdc, ref RECT pRect, Int32 dwData) =>
            {
                _monitors.Add(GetMonitor(hMonitor));

                return true;
            };

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _callback, 0);

            return _monitors.OrderByDescending(m => m.IsPrimary).ToArray();
        }

        public static Monitor GetMonitorFromIndex(Int32 index)
        {
            return GetMonitorFromIndex(index, GetMonitors());
        }

        private static Monitor GetMonitorFromIndex(Int32 index, Monitor[] monitors)
        {
            if (index < monitors.Length)
                return monitors[index];
            else
                return monitors.GetPrimary();
        }
        
        public static void GetWorkArea(AppBarWindow window, out Int32 screen, out DockEdge edge, out WorkArea windowWA, out WorkArea appbarWA)
        {
            screen = Framework.Settings.Instance.ScreenIndex;
            edge = Framework.Settings.Instance.DockEdge;

            Double _uiScale = Framework.Settings.Instance.UIScale;

            if (OS.SupportDPI)
            {
                window.UpdateScale(_uiScale, _uiScale, false);
            }

            Monitor[] _monitors = GetMonitors();

            Monitor _primary = _monitors.GetPrimary();            
            Monitor _active = GetMonitorFromIndex(screen, _monitors);

            windowWA = Windows.WorkArea.FromRECT(_active.WorkArea);
            windowWA.Scale(_primary.InverseScaleX, _primary.InverseScaleY);

            Double _modifyX = 0d;
            Double _modifyY = 0d;

            if (
                window.IsAppBar &&
                window.Screen == screen &&
                window.DockEdge == edge &&
                (_active.WorkArea.Width + window.ActualWidth) <= _active.Size.Width
                )
            {
                _modifyX = window.ActualWidth;

                if (edge == DockEdge.Left)
                {
                    _modifyX *= -1;
                }
            }

            windowWA.Offset(_modifyX, _modifyY);

            Double _windowWidth = Framework.Settings.Instance.SidebarWidth * _uiScale;

            windowWA.SetWidth(edge, _windowWidth);
            
            Int32 _offsetX = Framework.Settings.Instance.XOffset;
            Int32 _offsetY = Framework.Settings.Instance.YOffset;

            windowWA.Offset(_offsetX, _offsetY);

            appbarWA = Windows.WorkArea.FromRECT(_active.WorkArea);

            appbarWA.Offset(_modifyX, _modifyY);

            Double _appbarWidth = Framework.Settings.Instance.UseAppBar ? windowWA.Width * _primary.ScaleX : 0;

            appbarWA.SetWidth(edge, _appbarWidth);

            appbarWA.Offset(_offsetX, _offsetY);
        }
    }

    public static class MonitorExtensions
    {
        public static Monitor GetPrimary(this Monitor[] monitors)
        {
            return monitors.Where(m => m.IsPrimary).Single();
        }
    }

    public partial class DPIAwareWindow : FlatWindow
    {
        private static class WM_MESSAGES
        {
            public const Int32 WM_DPICHANGED = 0x02E0;
            public const Int32 WM_GETMINMAXINFO = 0x0024;
            public const Int32 WM_SIZE = 0x0005;
            public const Int32 WM_WINDOWPOSCHANGING = 0x0046;
            public const Int32 WM_WINDOWPOSCHANGED = 0x0047;
        }

        public override void BeginInit()
        {
            Utilities.Culture.SetCurrent(false);

            base.BeginInit();
        }

        public override void EndInit()
        {
            base.EndInit();

            _originalWidth = base.Width;
            _originalHeight = base.Height;

            if (AutoDPI && OS.SupportDPI)
            {
                Loaded += DPIAwareWindow_Loaded;
            }
        }

        public void HandleDPI()
        {
            //IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            //IntPtr _hmonitor = NativeMethods.MonitorFromWindow(_hwnd, 0);

            //Monitor _monitorInfo = Monitor.GetMonitor(_hmonitor);

            Double _uiScale = Framework.Settings.Instance.UIScale;

            UpdateScale(_uiScale, _uiScale, true);
        }

        public void UpdateScale(Double scaleX, Double scaleY, Boolean resize)
        {
            if (VisualChildrenCount > 0)
            {
                GetVisualChild(0).SetValue(LayoutTransformProperty, new ScaleTransform(scaleX, scaleY));
            }

            if (resize)
            {
                SizeToContent _autosize = SizeToContent;
                SizeToContent = SizeToContent.Manual;

                base.Width = _originalWidth * scaleX;
                base.Height = _originalHeight * scaleY;

                SizeToContent = _autosize;
            }
        }

        private void DPIAwareWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            HandleDPI();

            Framework.Settings.Instance.PropertyChanged += UIScale_PropertyChanged;

            //HwndSource.AddHook(WindowHook);
        }

        private void UIScale_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UIScale")
            {
                HandleDPI();
            }
        }

        //private IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == WM_MESSAGES.WM_DPICHANGED)
        //    {
        //        HandleDPI();

        //        handled = true;
        //    }

        //    return IntPtr.Zero;
        //}

        public HwndSource HwndSource => (HwndSource)PresentationSource.FromVisual(this);

        public static readonly DependencyProperty AutoDPIProperty = DependencyProperty.Register("AutoDPI", typeof(Boolean), typeof(DPIAwareWindow), new UIPropertyMetadata(true));

        public Boolean AutoDPI
        {
            get => (Boolean)GetValue(AutoDPIProperty);
            set => SetValue(AutoDPIProperty, value);
        }

        public new Double Width
        {
            get => base.Width;
            set => _originalWidth = base.Width = value;
        }

        public new Double Height
        {
            get => base.Height;
            set => _originalHeight = base.Height = value;
        }

        private Double _originalWidth { get; set; }

        private Double _originalHeight { get; set; }
    }

    [Serializable]
    public enum DockEdge : byte
    {
        Left,
        Top,
        Right,
        Bottom,
        None
    }

    public partial class AppBarWindow : DPIAwareWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct APPBARDATA
        {
            public Int32 cbSize;
            public IntPtr hWnd;
            public Int32 uCallbackMessage;
            public Int32 uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        private static class APPBARMSG
        {
            public const Int32 ABM_NEW = 0;
            public const Int32 ABM_REMOVE = 1;
            public const Int32 ABM_QUERYPOS = 2;
            public const Int32 ABM_SETPOS = 3;
            public const Int32 ABM_GETSTATE = 4;
            public const Int32 ABM_GETTASKBARPOS = 5;
            public const Int32 ABM_ACTIVATE = 6;
            public const Int32 ABM_GETAUTOHIDEBAR = 7;
            public const Int32 ABM_SETAUTOHIDEBAR = 8;
            public const Int32 ABM_WINDOWPOSCHANGED = 9;
            public const Int32 ABM_SETSTATE = 10;
        }

        private static class APPBARNOTIFY
        {
            public const Int32 ABN_STATECHANGE = 0;
            public const Int32 ABN_POSCHANGED = 1;
            public const Int32 ABN_FULLSCREENAPP = 2;
            public const Int32 ABN_WINDOWARRANGE = 3;
        }

        internal enum DWMWINDOWATTRIBUTE : int
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY = 2,
            DWMWA_TRANSITIONS_FORCEDISABLED = 3,
            DWMWA_ALLOW_NCPAINT = 4,
            DWMWA_CAPTION_BUTTON_BOUNDS = 5,
            DWMWA_NONCLIENT_RTL_LAYOUT = 6,
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7,
            DWMWA_FLIP3D_POLICY = 8,
            DWMWA_EXTENDED_FRAME_BOUNDS = 9,
            DWMWA_HAS_ICONIC_BITMAP = 10,
            DWMWA_DISALLOW_PEEK = 11,
            DWMWA_EXCLUDED_FROM_PEEK = 12,
            DWMWA_CLOAK = 13,
            DWMWA_CLOAKED = 14,
            DWMWA_FREEZE_REPRESENTATION = 15,
            DWMWA_LAST = 16
        }

        private static class HWND_FLAG
        {
            public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
            public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

            public const UInt32 SWP_NOSIZE = 0x0001;
            public const UInt32 SWP_NOMOVE = 0x0002;
            public const UInt32 SWP_NOACTIVATE = 0x0010;
        }

        private static class WND_STYLE
        {
            public const Int32 GWL_EXSTYLE = -20;

            public const Int64 WS_EX_TRANSPARENT = 32;
            public const Int64 WS_EX_TOOLWINDOW = 128;
        }

        private static class WM_WINDOWPOSCHANGING
        {
            public const Int32 MSG = 0x0046;
            public const Int32 SWP_NOMOVE = 0x0002;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hWnd;
            public IntPtr hWndInsertAfter;
            public Int32 x;
            public Int32 y;
            public Int32 cx;
            public Int32 cy;
            public UInt32 flags;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += AppBarWindow_Loaded;
        }

        private void AppBarWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            PreventMove();
        }

        public void Move(WorkArea workArea)
        {
            AllowMove();

            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;

            PreventMove();
        }

        private void PreventMove()
        {
            if (!_canMove)
            {
                return;
            }

            _canMove = false;

            HwndSource.AddHook(MoveHook);
        }

        private void AllowMove()
        {
            if (_canMove)
            {
                return;
            }

            _canMove = true;

            HwndSource.RemoveHook(MoveHook);
        }

        private IntPtr MoveHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == WM_WINDOWPOSCHANGING.MSG)
            {
                WINDOWPOS _pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                _pos.flags |= WM_WINDOWPOSCHANGING.SWP_NOMOVE;

                Marshal.StructureToPtr(_pos, lParam, true);

                handled = true;
            }

            return IntPtr.Zero;
        }

        public void SetTopMost(Boolean activate)
        {
            if (IsTopMost)
            {
                return;
            }

            IsTopMost = true;

            SetPos(HWND_FLAG.HWND_TOPMOST, activate);
        }

        public void ClearTopMost(Boolean activate)
        {
            if (!IsTopMost)
            {
                return;
            }

            IsTopMost = false;

            SetPos(HWND_FLAG.HWND_NOTOPMOST, activate);
        }

        public void SetBottom(Boolean activate)
        {
            IsTopMost = false;

            SetPos(HWND_FLAG.HWND_BOTTOM, activate);
        }

        private void SetPos(IntPtr hwnd_after, Boolean activate)
        {
            UInt32 _uflags = HWND_FLAG.SWP_NOMOVE | HWND_FLAG.SWP_NOSIZE;

            if (!activate)
            {
                _uflags |= HWND_FLAG.SWP_NOACTIVATE;
            }

            NativeMethods.SetWindowPos(
                new WindowInteropHelper(this).Handle,
                hwnd_after,
                0,
                0,
                0,
                0,
                _uflags
                );
        }

        public void SetClickThrough()
        {
            if (IsClickThrough)
            {
                return;
            }

            IsClickThrough = true;

            SetWindowLong(WND_STYLE.WS_EX_TRANSPARENT, null);
        }

        public void ClearClickThrough()
        {
            if (!IsClickThrough)
            {
                return;
            }

            IsClickThrough = false;

            SetWindowLong(null, WND_STYLE.WS_EX_TRANSPARENT);
        }

        public void ShowInAltTab()
        {
            if (IsInAltTab)
            {
                return;
            }

            IsInAltTab = true;

            SetWindowLong(null, WND_STYLE.WS_EX_TOOLWINDOW);
        }

        public void HideInAltTab()
        {
            if (!IsInAltTab)
            {
                return;
            }

            IsInAltTab = false;

            SetWindowLong(WND_STYLE.WS_EX_TOOLWINDOW, null);
        }

        public void DisableAeroPeek()
        {
            IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            IntPtr _status = Marshal.AllocHGlobal(sizeof(Int32));
            Marshal.WriteInt32(_status, 1);

            NativeMethods.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_EXCLUDED_FROM_PEEK, _status, sizeof(Int32));
        }

        private void SetWindowLong(Int64? add, Int64? remove)
        {
            IntPtr _hwnd = new WindowInteropHelper(this).Handle;

            Boolean _32bit = IntPtr.Size == 4;

            Int64 _style;

            if (_32bit)
            {
                _style = NativeMethods.GetWindowLong(_hwnd, WND_STYLE.GWL_EXSTYLE);
            }
            else
            {
                _style = NativeMethods.GetWindowLongPtr(_hwnd, WND_STYLE.GWL_EXSTYLE);
            }

            if (add.HasValue)
            {
                _style |= add.Value;
            }

            if (remove.HasValue)
            {
                _style &= ~remove.Value;
            }

            if (_32bit)
            {
                NativeMethods.SetWindowLong(_hwnd, WND_STYLE.GWL_EXSTYLE, _style);
            }
            else
            {
                NativeMethods.SetWindowLongPtr(_hwnd, WND_STYLE.GWL_EXSTYLE, _style);
            }
        }

        public void SetAppBar(Int32 screen, DockEdge edge, WorkArea windowWA, WorkArea appbarWA, Action callback)
        {
            if (edge == DockEdge.None)
            {
                throw new ArgumentException("This parameter cannot be set to 'none'.", "edge");
            }

            Boolean _init = false;

            APPBARDATA _data = NewData();

            if (!IsAppBar)
            {
                IsAppBar = _init = true;

                _callbackID = _data.uCallbackMessage = NativeMethods.RegisterWindowMessage("AppBarMessage");

                NativeMethods.SHAppBarMessage(APPBARMSG.ABM_NEW, ref _data);
            }

            Screen = screen;
            DockEdge = edge;
            
            _data.uEdge = (Int32)edge;
            _data.rc = new RECT()
            {
                Left = (Int32)Math.Round(appbarWA.Left),
                Top = (Int32)Math.Round(appbarWA.Top),
                Right = (Int32)Math.Round(appbarWA.Right),
                Bottom = (Int32)Math.Round(appbarWA.Bottom)
            };

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_QUERYPOS, ref _data);

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_SETPOS, ref _data);

            appbarWA.Left = _data.rc.Left;
            appbarWA.Top = _data.rc.Top;
            appbarWA.Right = _data.rc.Right;
            appbarWA.Bottom = _data.rc.Bottom;

            AppBarWidth = appbarWA.Width;

            Move(windowWA);

            if (_init)
            {
                Task.Delay(500).ContinueWith(_ =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                    {
                        HwndSource.AddHook(AppBarHook);

                        if (callback != null)
                        {
                            callback();
                        }
                    }));
                });
            }
            else if (callback != null)
            {
                callback();
            }
        }

        public void ClearAppBar()
        {
            if (!IsAppBar)
            {
                return;
            }

            HwndSource.RemoveHook(AppBarHook);

            APPBARDATA _data = NewData();

            NativeMethods.SHAppBarMessage(APPBARMSG.ABM_REMOVE, ref _data);

            IsAppBar = false;
        }

        public virtual void AppBarShow()
        {
            if (Framework.Settings.Instance.UseAppBar)
            {
                Int32 _screen;
                DockEdge _edge;
                WorkArea _windowWA;
                WorkArea _appbarWA;

                Monitor.GetWorkArea(this, out _screen, out _edge, out _windowWA, out _appbarWA);

                SetAppBar(_screen, _edge, _windowWA, _appbarWA, null);
            }

            Show();
        }

        public virtual void AppBarHide()
        {
            Hide();

            if (IsAppBar)
            {
                ClearAppBar();
            }
        }

        private APPBARDATA NewData()
        {
            APPBARDATA _data = new APPBARDATA();
            _data.cbSize = Marshal.SizeOf(_data);
            _data.hWnd = new WindowInteropHelper(this).Handle;

            return _data;
        }

        private IntPtr AppBarHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == _callbackID)
            {
                switch (wParam.ToInt32())
                {
                    case APPBARNOTIFY.ABN_POSCHANGED:
                        if (_cancelReposition != null)
                        {
                            _cancelReposition.Cancel();
                        }

                        _cancelReposition = new CancellationTokenSource();

                        Task.Delay(TimeSpan.FromMilliseconds(100), _cancelReposition.Token).ContinueWith(_ =>
                        {
                            if (_.IsCanceled)
                            {
                                return;
                            }

                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                            {
                                Int32 _screen;
                                DockEdge _edge;
                                WorkArea _windowWA;
                                WorkArea _appbarWA;

                                Monitor.GetWorkArea(this, out _screen, out _edge, out _windowWA, out _appbarWA);

                                SetAppBar(_screen, _edge, _windowWA, _appbarWA, null);
                            }));

                            _cancelReposition = null;
                        });
                        break;

                    case APPBARNOTIFY.ABN_FULLSCREENAPP:
                        if (lParam.ToInt32() == 1)
                        {
                            _wasTopMost = IsTopMost;

                            if (IsTopMost)
                            {
                                SetBottom(false);
                            }
                        }
                        else if (_wasTopMost)
                        {
                            SetTopMost(false);
                        }
                        break;
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        public Boolean IsTopMost { get; private set; } = false;

        public Boolean IsClickThrough { get; private set; } = false;

        public Boolean IsInAltTab { get; private set; } = true;

        public Boolean IsAppBar { get; private set; } = false;

        public Int32 Screen { get; private set; } = 0;

        public DockEdge DockEdge { get; private set; } = DockEdge.None;

        public Double AppBarWidth { get; private set; } = 0;

        private Boolean _canMove { get; set; } = true;

        private Boolean _wasTopMost { get; set; } = false;

        private Int32 _callbackID { get; set; }

        private CancellationTokenSource _cancelReposition { get; set; }
    }
}