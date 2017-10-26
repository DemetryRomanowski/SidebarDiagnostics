using System;
using System.IO;
using System.ComponentModel;
using Newtonsoft.Json;
using SidebarDiagnostics.Monitoring;
using SidebarDiagnostics.Utilities;
using SidebarDiagnostics.Windows;

namespace SidebarDiagnostics.Framework
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Settings : INotifyPropertyChanged
    {
        private Settings() { }

        public void Save()
        {
            if (!Directory.Exists(Paths.LocalApp))
            {
                Directory.CreateDirectory(Paths.LocalApp);
            }

            using (StreamWriter _writer = File.CreateText(Paths.SettingsFile))
            {
                new JsonSerializer() { Formatting = Formatting.Indented }.Serialize(_writer, this);
            }
        }

        public void Reload()
        {
            _instance = Load();
        }

        private static Settings Load()
        {
            Settings _return = null;

            if (File.Exists(Paths.SettingsFile))
            {
                using (StreamReader _reader = File.OpenText(Paths.SettingsFile))
                {
                    _return = (Settings)new JsonSerializer().Deserialize(_reader, typeof(Settings));
                }
            }

            return _return ?? new Settings();
        }

        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private String _changeLog { get; set; } = null;

        [JsonProperty]
        public String ChangeLog
        {
            get => _changeLog;
            set
            {
                _changeLog = value;

                NotifyPropertyChanged("ChangeLog");
            }
        }

        private Boolean _initialSetup { get; set; } = true;

        [JsonProperty]
        public Boolean InitialSetup
        {
            get => _initialSetup;
            set
            {
                _initialSetup = value;

                NotifyPropertyChanged("InitialSetup");
            }
        }

        private DockEdge _dockEdge { get; set; } = DockEdge.Right;

        [JsonProperty]
        public DockEdge DockEdge
        {
            get => _dockEdge;
            set
            {
                _dockEdge = value;

                NotifyPropertyChanged("DockEdge");
            }
        }

        private Int32 _screenIndex { get; set; } = 0;

        [JsonProperty]
        public Int32 ScreenIndex
        {
            get => _screenIndex;
            set
            {
                _screenIndex = value;

                NotifyPropertyChanged("ScreenIndex");
            }
        }

        private String _culture { get; set; } = Utilities.Culture.DEFAULT;

        [JsonProperty]
        public String Culture
        {
            get => _culture;
            set
            {
                _culture = value;

                NotifyPropertyChanged("Culture");
            }
        }

        private Boolean _useAppBar { get; set; } = true;
        
        [JsonProperty]
        public Boolean UseAppBar
        {
            get => _useAppBar;
            set
            {
                _useAppBar = value;

                NotifyPropertyChanged("UseAppBar");
            }
        }

        private Boolean _alwaysTop { get; set; } = true;

        [JsonProperty]
        public Boolean AlwaysTop
        {
            get => _alwaysTop;
            set
            {
                _alwaysTop = value;

                NotifyPropertyChanged("AlwaysTop");
            }
        }

        private Boolean _autoUpdate { get; set; } = true;

        [JsonProperty]
        public Boolean AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                _autoUpdate = value;

                NotifyPropertyChanged("AutoUpdate");
            }
        }

        private Boolean _runAtStartup { get; set; } = true;

        [JsonProperty]
        public Boolean RunAtStartup
        {
            get => _runAtStartup;
            set
            {
                _runAtStartup = value;

                NotifyPropertyChanged("RunAtStartup");
            }
        }

        private Double _uiScale { get; set; } = 1d;

        [JsonProperty]
        public Double UIScale
        {
            get => _uiScale;
            set
            {
                _uiScale = value;

                NotifyPropertyChanged("UIScale");
            }
        }

        private Int32 _xOffset { get; set; } = 0;

        [JsonProperty]
        public Int32 XOffset
        {
            get => _xOffset;
            set
            {
                _xOffset = value;

                NotifyPropertyChanged("XOffset");
            }
        }

        private Int32 _yOffset { get; set; } = 0;

        [JsonProperty]
        public Int32 YOffset
        {
            get => _yOffset;
            set
            {
                _yOffset = value;

                NotifyPropertyChanged("YOffset");
            }
        }

        private Int32 _pollingInterval { get; set; } = 1000;

        [JsonProperty]
        public Int32 PollingInterval
        {
            get => _pollingInterval;
            set
            {
                _pollingInterval = value;

                NotifyPropertyChanged("PollingInterval");
            }
        }

        private Boolean _toolbarMode { get; set; } = true;

        [JsonProperty]
        public Boolean ToolbarMode
        {
            get => _toolbarMode;
            set
            {
                _toolbarMode = value;

                NotifyPropertyChanged("ToolbarMode");
            }
        }

        private Boolean _clickThrough { get; set; } = false;

        [JsonProperty]
        public Boolean ClickThrough
        {
            get => _clickThrough;
            set
            {
                _clickThrough = value;

                NotifyPropertyChanged("ClickThrough");
            }
        }

        private Boolean _showTrayIcon { get; set; } = true;

        [JsonProperty]
        public Boolean ShowTrayIcon
        {
            get => _showTrayIcon;
            set
            {
                _showTrayIcon = value;

                NotifyPropertyChanged("ShowTrayIcon");
            }
        }

        private Boolean _collapseMenuBar { get; set; } = false;

        [JsonProperty]
        public Boolean CollapseMenuBar
        {
            get => _collapseMenuBar;
            set
            {
                _collapseMenuBar = value;

                NotifyPropertyChanged("CollapseMenuBar");
            }
        }

        private Boolean _initiallyHidden { get; set; } = false;

        [JsonProperty]
        public Boolean InitiallyHidden
        {
            get => _initiallyHidden;
            set
            {
                _initiallyHidden = value;
                
                NotifyPropertyChanged("InitiallyHidden");
            }
        }

        private Int32 _sidebarWidth { get; set; } = 180;

        [JsonProperty]
        public Int32 SidebarWidth
        {
            get => _sidebarWidth;
            set
            {
                _sidebarWidth = value;

                NotifyPropertyChanged("SidebarWidth");
            }
        }

        private Boolean _autoBGColor { get; set; } = false;

        [JsonProperty]
        public Boolean AutoBGColor
        {
            get => _autoBGColor;
            set
            {
                _autoBGColor = value;

                NotifyPropertyChanged("AutoBGColor");
            }
        }

        private String _bgColor { get; set; } = "#000000";

        [JsonProperty]
        public String BGColor
        {
            get => _bgColor;
            set
            {
                _bgColor = value;

                NotifyPropertyChanged("BGColor");
            }
        }

        private Double _bgOpacity { get; set; } = 0.85d;

        [JsonProperty]
        public Double BGOpacity
        {
            get => _bgOpacity;
            set
            {
                _bgOpacity = value;

                NotifyPropertyChanged("BGOpacity");
            }
        }

        private TextAlign _textAlign { get; set; } = TextAlign.Left;

        [JsonProperty]
        public TextAlign TextAlign
        {
            get => _textAlign;
            set
            {
                _textAlign = value;

                NotifyPropertyChanged("TextAlign");
            }
        }

        private FontSetting _fontSetting { get; set; } = FontSetting.x14;

        [JsonProperty]
        public FontSetting FontSetting
        {
            get => _fontSetting;
            set
            {
                _fontSetting = value;

                NotifyPropertyChanged("FontSetting");
            }
        }

        private String _fontColor { get; set; } = "#FFFFFF";
        
        [JsonProperty]
        public String FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;

                NotifyPropertyChanged("FontColor");
            }
        }

        private String _alertFontColor { get; set; } = "#FF4136";

        [JsonProperty]
        public String AlertFontColor
        {
            get => _alertFontColor;
            set
            {
                _alertFontColor = value;

                NotifyPropertyChanged("AlertFontColor");
            }
        }

        private Boolean _alertBlink { get; set; } = true;

        [JsonProperty]
        public Boolean AlertBlink
        {
            get => _alertBlink;
            set
            {
                _alertBlink = value;

                NotifyPropertyChanged("AlertBlink");
            }
        }

        private Boolean _showClock { get; set; } = true;

        [JsonProperty]
        public Boolean ShowClock
        {
            get => _showClock;
            set
            {
                _showClock = value;

                NotifyPropertyChanged("ShowClock");
            }
        }

        private Boolean _clock24HR { get; set; } = false;

        [JsonProperty]
        public Boolean Clock24HR
        {
            get => _clock24HR;
            set
            {
                _clock24HR = value;

                NotifyPropertyChanged("Clock24HR");
            }
        }

        private DateSetting _dateSetting { get; set; } = DateSetting.Short;

        [JsonProperty]
        public DateSetting DateSetting
        {
            get => _dateSetting;
            set
            {
                _dateSetting = value;

                NotifyPropertyChanged("DateSetting");
            }
        }

        private MonitorConfig[] _monitorConfig { get; set; } = null;

        [JsonProperty]
        public MonitorConfig[] MonitorConfig
        {
            get => _monitorConfig;
            set
            {
                _monitorConfig = value;

                NotifyPropertyChanged("MonitorConfig");
            }
        }

        private Hotkey[] _hotkeys { get; set; } = new Hotkey[0];

        [JsonProperty]
        public Hotkey[] Hotkeys
        {
            get => _hotkeys;
            set
            {
                _hotkeys = value;

                NotifyPropertyChanged("Hotkeys");
            }
        }

        private static Settings _instance { get; set; } = null;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }

                return _instance;
            }
        }
    }

    public enum TextAlign : byte
    {
        Left,
        Right
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class FontSetting
    {
        internal FontSetting() { }

        private FontSetting(Int32 fontSize)
        {
            FontSize = fontSize;
        }

        public override Boolean Equals(Object obj)
        {
            FontSetting _that = obj as FontSetting;

            if (_that == null)
            {
                return false;
            }

            return this.FontSize == _that.FontSize;
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public static FontSetting x10 => new FontSetting(10);

        public static FontSetting x12 => new FontSetting(12);

        public static FontSetting x14 => new FontSetting(14);

        public static FontSetting x16 => new FontSetting(16);

        public static FontSetting x18 => new FontSetting(18);

        [JsonProperty]
        public Int32 FontSize { get; set; }

        public Int32 TitleFontSize => FontSize + 2;

        public Int32 SmallFontSize => FontSize - 2;

        public Int32 IconSize
        {
            get
            {
                switch (FontSize)
                {
                    case 10:
                        return 18;

                    case 12:
                        return 22;

                    case 14:
                    default:
                        return 24;

                    case 16:
                        return 28;

                    case 18:
                        return 32;
                }
            }
        }

        public Int32 BarHeight => FontSize - 3;

        public Int32 BarWidth => BarHeight * 6;

        public Int32 BarWidthWide => BarHeight * 8;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DateSetting
    {
        internal DateSetting() { }

        private DateSetting(String format)
        {
            Format = format;
        }

        [JsonProperty]
        public String Format { get; set; }

        public String Display
        {
            get
            {
                if (String.Equals(Format, "Disabled", StringComparison.Ordinal))
                {
                    return Resources.SettingsDateFormatDisabled;
                }

                return DateTime.Today.ToString(Format);
            }
        }

        public override Boolean Equals(Object obj)
        {
            DateSetting _that = obj as DateSetting;

            if (_that == null)
            {
                return false;
            }

            return String.Equals(this.Format, _that.Format, StringComparison.Ordinal);
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public static readonly DateSetting Disabled = new DateSetting("Disabled");
        public static readonly DateSetting Short = new DateSetting("M");
        public static readonly DateSetting Normal = new DateSetting("d");
        public static readonly DateSetting Long = new DateSetting("D");
    }
}
