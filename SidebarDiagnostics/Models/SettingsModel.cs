using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using SidebarDiagnostics.Framework;
using SidebarDiagnostics.Monitoring;
using SidebarDiagnostics.Utilities;
using SidebarDiagnostics.Windows;

namespace SidebarDiagnostics.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public SettingsModel(Sidebar sidebar)
        {
            DockEdgeItems = new DockItem[2]
            {
                new DockItem() { Text = Resources.SettingsDockLeft, Value = DockEdge.Left },
                new DockItem() { Text = Resources.SettingsDockRight, Value = DockEdge.Right }
            };

            DockEdge = Framework.Settings.Instance.DockEdge;

            Monitor[] _monitors = Monitor.GetMonitors();

            ScreenItems = _monitors.Select((s, i) => new ScreenItem() { Index = i, Text = String.Format("#{0}", i + 1) }).ToArray();

            if (Framework.Settings.Instance.ScreenIndex < _monitors.Length)
            {
                ScreenIndex = Framework.Settings.Instance.ScreenIndex;
            }
            else
            {
                ScreenIndex = _monitors.Where(s => s.IsPrimary).Select((s, i) => i).Single();
            }

            CultureItems = Utilities.Culture.GetAll();
            Culture = Framework.Settings.Instance.Culture;

            UIScale = Framework.Settings.Instance.UIScale;
            XOffset = Framework.Settings.Instance.XOffset;
            YOffset = Framework.Settings.Instance.YOffset;
            PollingInterval = Framework.Settings.Instance.PollingInterval;
            UseAppBar = Framework.Settings.Instance.UseAppBar;
            AlwaysTop = Framework.Settings.Instance.AlwaysTop;
            ToolbarMode = Framework.Settings.Instance.ToolbarMode;
            ClickThrough = Framework.Settings.Instance.ClickThrough;
            ShowTrayIcon = Framework.Settings.Instance.ShowTrayIcon;
            AutoUpdate = Framework.Settings.Instance.AutoUpdate;
            RunAtStartup = Framework.Settings.Instance.RunAtStartup;
            SidebarWidth = Framework.Settings.Instance.SidebarWidth;
            AutoBGColor = Framework.Settings.Instance.AutoBGColor;
            BGColor = Framework.Settings.Instance.BGColor;
            BGOpacity = Framework.Settings.Instance.BGOpacity;

            TextAlignItems = new TextAlignItem[2]
            {
                new TextAlignItem() { Text = Resources.SettingsTextAlignLeft, Value = TextAlign.Left },
                new TextAlignItem() { Text = Resources.SettingsTextAlignRight, Value = TextAlign.Right }
            };

            TextAlign = Framework.Settings.Instance.TextAlign;

            FontSettingItems = new FontSetting[5]
            {
                FontSetting.x10,
                FontSetting.x12,
                FontSetting.x14,
                FontSetting.x16,
                FontSetting.x18
            };

            FontSetting = Framework.Settings.Instance.FontSetting;
            FontColor = Framework.Settings.Instance.FontColor;
            AlertFontColor = Framework.Settings.Instance.AlertFontColor;
            AlertBlink = Framework.Settings.Instance.AlertBlink;

            DateSettingItems = new DateSetting[4]
            {
                DateSetting.Disabled,
                DateSetting.Short,
                DateSetting.Normal,
                DateSetting.Long
            };

            DateSetting = Framework.Settings.Instance.DateSetting;
            CollapseMenuBar = Framework.Settings.Instance.CollapseMenuBar;
            InitiallyHidden = Framework.Settings.Instance.InitiallyHidden;
            ShowClock = Framework.Settings.Instance.ShowClock;
            Clock24HR = Framework.Settings.Instance.Clock24HR;

            ObservableCollection<MonitorConfig> _config = new ObservableCollection<MonitorConfig>(Framework.Settings.Instance.MonitorConfig.Select(c => c.Clone()).OrderByDescending(c => c.Order));

            if (sidebar.Ready)
            {
                foreach (MonitorConfig _record in _config)
                {
                    _record.HardwareOC = new ObservableCollection<HardwareConfig>(
                        from hw in sidebar.Model.MonitorManager.GetHardware(_record.Type)
                        join config in _record.Hardware on hw.ID equals config.ID into merged
                        from newhw in merged.DefaultIfEmpty(hw).Select(newhw => { newhw.ActualName = hw.ActualName; if (String.IsNullOrEmpty(newhw.Name)) { newhw.Name = hw.ActualName; } return newhw; })
                        orderby newhw.Order descending, newhw.Name ascending
                        select newhw
                        );
                }
            }

            MonitorConfig = _config;

            if (Framework.Settings.Instance.Hotkeys != null)
            {
                ToggleKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.Toggle);
                ShowKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.Show);
                HideKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.Hide);
                ReloadKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.Reload);
                CloseKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.Close);
                CycleEdgeKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.CycleEdge);
                CycleScreenKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.CycleScreen);
                ReserveSpaceKey = Framework.Settings.Instance.Hotkeys.FirstOrDefault(k => k.Action == Hotkey.KeyAction.ReserveSpace);
            }

            IsChanged = false;
        }

        public void Save()
        {
            if (!String.Equals(Culture, Framework.Settings.Instance.Culture, StringComparison.Ordinal))
            {
                MessageBox.Show(Resources.LanguageChangedText, Resources.LanguageChangedTitle, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }

            Framework.Settings.Instance.DockEdge = DockEdge;
            Framework.Settings.Instance.ScreenIndex = ScreenIndex;
            Framework.Settings.Instance.Culture = Culture;
            Framework.Settings.Instance.UIScale = UIScale;
            Framework.Settings.Instance.XOffset = XOffset;
            Framework.Settings.Instance.YOffset = YOffset;
            Framework.Settings.Instance.PollingInterval = PollingInterval;
            Framework.Settings.Instance.UseAppBar = UseAppBar;
            Framework.Settings.Instance.AlwaysTop = AlwaysTop;
            Framework.Settings.Instance.ToolbarMode = ToolbarMode;
            Framework.Settings.Instance.ClickThrough = ClickThrough;
            Framework.Settings.Instance.ShowTrayIcon = ShowTrayIcon;
            Framework.Settings.Instance.AutoUpdate = AutoUpdate;
            Framework.Settings.Instance.RunAtStartup = RunAtStartup;
            Framework.Settings.Instance.SidebarWidth = SidebarWidth;
            Framework.Settings.Instance.AutoBGColor = AutoBGColor;
            Framework.Settings.Instance.BGColor = BGColor;
            Framework.Settings.Instance.BGOpacity = BGOpacity;
            Framework.Settings.Instance.TextAlign = TextAlign;
            Framework.Settings.Instance.FontSetting = FontSetting;
            Framework.Settings.Instance.FontColor = FontColor;
            Framework.Settings.Instance.AlertFontColor = AlertFontColor;
            Framework.Settings.Instance.AlertBlink = AlertBlink;
            Framework.Settings.Instance.DateSetting = DateSetting;
            Framework.Settings.Instance.CollapseMenuBar = CollapseMenuBar;
            Framework.Settings.Instance.InitiallyHidden = InitiallyHidden;
            Framework.Settings.Instance.ShowClock = ShowClock;
            Framework.Settings.Instance.Clock24HR = Clock24HR;

            MonitorConfig[] _config = MonitorConfig.Select(c => c.Clone()).ToArray();

            for (Int32 i = 0; i < _config.Length; i++)
            {
                HardwareConfig[] _hardware = _config[i].HardwareOC.ToArray();

                for (Int32 v = 0; v < _hardware.Length; v++)
                {
                    _hardware[v].Order = Convert.ToByte(_hardware.Length - v);

                    if (String.IsNullOrEmpty(_hardware[v].Name) || String.Equals(_hardware[v].Name, _hardware[v].ActualName, StringComparison.Ordinal))
                    {
                        _hardware[v].Name = null;
                    }
                }

                _config[i].Hardware = _hardware;
                _config[i].HardwareOC = null;

                _config[i].Order = Convert.ToByte(_config.Length - i);
            }

            Framework.Settings.Instance.MonitorConfig = _config;

            List<Hotkey> _hotkeys = new List<Hotkey>();

            if (ToggleKey != null)
            {
                _hotkeys.Add(ToggleKey);
            }
            
            if (ShowKey != null)
            {
                _hotkeys.Add(ShowKey);
            }

            if (HideKey != null)
            {
                _hotkeys.Add(HideKey);
            }

            if (ReloadKey != null)
            {
                _hotkeys.Add(ReloadKey);
            }

            if (CloseKey != null)
            {
                _hotkeys.Add(CloseKey);
            }

            if (CycleEdgeKey != null)
            {
                _hotkeys.Add(CycleEdgeKey);
            }

            if (CycleScreenKey != null)
            {
                _hotkeys.Add(CycleScreenKey);
            }

            if (ReserveSpaceKey != null)
            {
                _hotkeys.Add(ReserveSpaceKey);
            }

            Framework.Settings.Instance.Hotkeys = _hotkeys.ToArray();

            Framework.Settings.Instance.Save();

            App.RefreshIcon();

            if (RunAtStartup)
            {
                Startup.EnableStartupTask();
            }
            else
            {
                Startup.DisableStartupTask();
            }

            IsChanged = false;
        }

        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            if (propertyName != "IsChanged")
            {
                IsChanged = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Child_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            IsChanged = true;
        }

        private void Child_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            IsChanged = true;
        }

        private Boolean _isChanged { get; set; } = false;

        public Boolean IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;

                NotifyPropertyChanged("IsChanged");
            }
        }

        private DockEdge _dockEdge { get; set; }

        public DockEdge DockEdge
        {
            get => _dockEdge;
            set
            {
                _dockEdge = value;

                NotifyPropertyChanged("DockEdge");
            }
        }

        private DockItem[] _dockEdgeItems { get; set; }

        public DockItem[] DockEdgeItems
        {
            get => _dockEdgeItems;
            set
            {
                _dockEdgeItems = value;

                NotifyPropertyChanged("DockEdgeItems");
            }
        }

        private Int32 _screenIndex { get; set; }

        public Int32 ScreenIndex
        {
            get => _screenIndex;
            set
            {
                _screenIndex = value;

                NotifyPropertyChanged("ScreenIndex");
            }
        }

        private ScreenItem[] _screenItems { get; set; }

        public ScreenItem[] ScreenItems
        {
            get => _screenItems;
            set
            {
                _screenItems = value;

                NotifyPropertyChanged("ScreenItems");
            }
        }

        private String _culture { get; set; }

        public String Culture
        {
            get => _culture;
            set
            {
                _culture = value;

                NotifyPropertyChanged("Culture");
            }
        }

        private CultureItem[] _cultureItems { get; set; }

        public CultureItem[] CultureItems
        {
            get => _cultureItems;
            set
            {
                _cultureItems = value;

                NotifyPropertyChanged("CultureItems");
            }
        }

        private Double _uiScale { get; set; }

        public Double UIScale
        {
            get => _uiScale;
            set
            {
                _uiScale = value;

                NotifyPropertyChanged("UIScale");
            }
        }

        private Int32 _xOffset { get; set; }

        public Int32 XOffset
        {
            get => _xOffset;
            set
            {
                _xOffset = value;

                NotifyPropertyChanged("XOffset");
            }
        }

        private Int32 _yOffset { get; set; }

        public Int32 YOffset
        {
            get => _yOffset;
            set
            {
                _yOffset = value;

                NotifyPropertyChanged("YOffset");
            }
        }

        private Int32 _pollingInterval { get; set; }

        public Int32 PollingInterval
        {
            get => _pollingInterval;
            set
            {
                _pollingInterval = value;

                NotifyPropertyChanged("PollingInterval");
            }
        }

        private Boolean _useAppBar { get; set; }

        public Boolean UseAppBar
        {
            get => _useAppBar;
            set
            {
                _useAppBar = value;

                NotifyPropertyChanged("UseAppBar");
            }
        }

        private Boolean _alwaysTop { get; set; }

        public Boolean AlwaysTop
        {
            get => _alwaysTop;
            set
            {
                _alwaysTop = value;

                NotifyPropertyChanged("AlwaysTop");
            }
        }

        private Boolean _toolbarMode { get; set; }
        
        public Boolean ToolbarMode
        {
            get => _toolbarMode;
            set
            {
                _toolbarMode = value;

                NotifyPropertyChanged("ToolbarMode");
            }
        }

        private Boolean _clickThrough { get; set; }

        public Boolean ClickThrough
        {
            get => _clickThrough;
            set
            {
                _clickThrough = value;

                NotifyPropertyChanged("ClickThrough");
            }
        }

        private Boolean _showTrayIcon { get; set; }

        public Boolean ShowTrayIcon
        {
            get => _showTrayIcon;
            set
            {
                _showTrayIcon = value;

                NotifyPropertyChanged("ShowTrayIcon");
            }
        }

        private Boolean _autoUpdate { get; set; }

        public Boolean AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                _autoUpdate = value;

                NotifyPropertyChanged("AutoUpdate");
            }
        }

        private Boolean _runAtStartup { get; set; }

        public Boolean RunAtStartup
        {
            get => _runAtStartup;
            set
            {
                _runAtStartup = value;

                NotifyPropertyChanged("RunAtStartup");
            }
        }

        private Int32 _sidebarWidth { get; set; }

        public Int32 SidebarWidth
        {
            get => _sidebarWidth;
            set
            {
                _sidebarWidth = value;

                NotifyPropertyChanged("SidebarWidth");
            }
        }

        private Boolean _autoBGColor { get; set; }

        public Boolean AutoBGColor
        {
            get => _autoBGColor;
            set
            {
                _autoBGColor = value;

                NotifyPropertyChanged("AutoBGColor");
            }
        }

        private String _bgColor { get; set; }

        public String BGColor
        {
            get => _bgColor;
            set
            {
                _bgColor = value;

                NotifyPropertyChanged("BGColor");
            }
        }

        private Double _bgOpacity { get; set; }

        public Double BGOpacity
        {
            get => _bgOpacity;
            set
            {
                _bgOpacity = value;

                NotifyPropertyChanged("BGOpacity");
            }
        }

        private TextAlign _textAlign { get; set; }

        public TextAlign TextAlign
        {
            get => _textAlign;
            set
            {
                _textAlign = value;

                NotifyPropertyChanged("TextAlign");
            }
        }

        private TextAlignItem[] _textAlignItems { get; set; }

        public TextAlignItem[] TextAlignItems
        {
            get => _textAlignItems;
            set
            {
                _textAlignItems = value;

                NotifyPropertyChanged("TextAlignItems");
            }
        }

        private FontSetting _fontSetting { get; set; }

        public FontSetting FontSetting
        {
            get => _fontSetting;
            set
            {
                _fontSetting = value;

                NotifyPropertyChanged("FontSize");
            }
        }

        private FontSetting[] _fontSettingItems { get;  set;}

        public FontSetting[] FontSettingItems
        {
            get => _fontSettingItems;
            set
            {
                _fontSettingItems = value;

                NotifyPropertyChanged("FontSizeItems");
            }
        }

        private String _fontColor { get; set; }

        public String FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;

                NotifyPropertyChanged("FontColor");
            }
        }

        private String _alertFontColor { get; set; }

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
        
        public Boolean AlertBlink
        {
            get => _alertBlink;
            set
            {
                _alertBlink = value;

                NotifyPropertyChanged("AlertBlink");
            }
        }

        private DateSetting _dateSetting { get; set; }

        public DateSetting DateSetting
        {
            get => _dateSetting;
            set
            {
                _dateSetting = value;

                NotifyPropertyChanged("DateSetting");
            }
        }

        private DateSetting[] _dateSettingItems { get; set; }

        public DateSetting[] DateSettingItems
        {
            get => _dateSettingItems;
            set
            {
                _dateSettingItems = value;

                NotifyPropertyChanged("DateSettingItems");
            }
        }

        private Boolean _collapseMenuBar { get; set; }

        public Boolean CollapseMenuBar
        {
            get => _collapseMenuBar;
            set
            {
                _collapseMenuBar = value;

                NotifyPropertyChanged("CollapseMenuBar");
            }
        }

        private Boolean _initiallyHidden { get; set; }
        
        public Boolean InitiallyHidden
        {
            get => _initiallyHidden;
            set
            {
                _initiallyHidden = value;

                NotifyPropertyChanged("InitiallyHidden");
            }
        }

        private Boolean _showClock { get; set; }

        public Boolean ShowClock
        {
            get => _showClock;
            set
            {
                _showClock = value;

                NotifyPropertyChanged("ShowClock");
            }
        }

        private Boolean _clock24HR { get; set; }

        public Boolean Clock24HR
        {
            get => _clock24HR;
            set
            {
                _clock24HR = value;

                NotifyPropertyChanged("Clock24HR");
            }
        }

        private ObservableCollection<MonitorConfig> _monitorConfig { get; set; }

        public ObservableCollection<MonitorConfig> MonitorConfig
        {
            get => _monitorConfig;
            set
            {
                _monitorConfig = value;

                _monitorConfig.CollectionChanged += Child_CollectionChanged;

                foreach (MonitorConfig _config in _monitorConfig)
                {
                    _config.PropertyChanged += Child_PropertyChanged;

                    _config.HardwareOC.CollectionChanged += Child_CollectionChanged;

                    foreach (HardwareConfig _hardware in _config.HardwareOC)
                    {
                        _hardware.PropertyChanged += Child_PropertyChanged;
                    }

                    foreach (MetricConfig _metric in _config.Metrics)
                    {
                        _metric.PropertyChanged += Child_PropertyChanged;
                    }

                    foreach (ConfigParam _param in _config.Params)
                    {
                        _param.PropertyChanged += Child_PropertyChanged;
                    }
                }

                NotifyPropertyChanged("MonitorConfig");
            }
        }

        private Hotkey _toggleKey { get; set; }

        public Hotkey ToggleKey
        {
            get => _toggleKey;
            set
            {
                _toggleKey = value;

                NotifyPropertyChanged("ToggleKey");
            }
        }

        private Hotkey _showKey { get; set; }

        public Hotkey ShowKey
        {
            get => _showKey;
            set
            {
                _showKey = value;

                NotifyPropertyChanged("ShowKey");
            }
        }

        private Hotkey _hideKey { get; set; }

        public Hotkey HideKey
        {
            get => _hideKey;
            set
            {
                _hideKey = value;

                NotifyPropertyChanged("HideKey");
            }
        }

        private Hotkey _reloadKey { get; set; }

        public Hotkey ReloadKey
        {
            get => _reloadKey;
            set
            {
                _reloadKey = value;

                NotifyPropertyChanged("ReloadKey");
            }
        }

        private Hotkey _closeKey { get; set; }

        public Hotkey CloseKey
        {
            get => _closeKey;
            set
            {
                _closeKey = value;

                NotifyPropertyChanged("CloseKey");
            }
        }

        private Hotkey _cycleEdgeKey { get; set; }

        public Hotkey CycleEdgeKey
        {
            get => _cycleEdgeKey;
            set
            {
                _cycleEdgeKey = value;

                NotifyPropertyChanged("CycleEdgeKey");
            }
        }

        private Hotkey _cycleScreenKey { get; set; }

        public Hotkey CycleScreenKey
        {
            get => _cycleScreenKey;
            set
            {
                _cycleScreenKey = value;

                NotifyPropertyChanged("CycleScreenKey");
            }
        }

        private Hotkey _reserveSpaceKey { get; set; }

        public Hotkey ReserveSpaceKey
        {
            get => _reserveSpaceKey;
            set
            {
                _reserveSpaceKey = value;

                NotifyPropertyChanged("ReserveSpaceKey");
            }
        }
    }

    public class DockItem
    {
        public DockEdge Value { get; set; }

        public String Text { get; set; }
    }

    public class ScreenItem
    {
        public Int32 Index { get; set; }

        public String Text { get; set; }
    }

    public class TextAlignItem
    {
        public TextAlign Value { get; set; }

        public String Text { get; set; }
    }
}