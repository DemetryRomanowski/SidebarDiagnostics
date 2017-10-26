using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Documents;
using System.Windows.Threading;
using SidebarDiagnostics.JsonObjects;
using SidebarDiagnostics.Monitoring;

namespace SidebarDiagnostics.Models
{
    public class SidebarModel : INotifyPropertyChanged, IDisposable
    {
        public SidebarModel()
        {
            InitClock();
            InitMonitors();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeClock();
                    DisposeMonitors();
                }

                _disposed = true;
            }
        }

        ~SidebarModel()
        {
            Dispose(false);
        }

        public void Start()
        {
            StartClock();
            StartLaunchLib();
            StartMonitors();
        }

        public void Reload()
        {
            DisposeMonitors();
            InitMonitors();
            StartMonitors();
        }

        public void Pause()
        {
            PauseClock();
            PauseMonitors();
        }

        public void Resume()
        {
            ResumeClock();
            ResumeMonitors();
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitClock()
        {
            ShowClock = Framework.Settings.Instance.ShowClock;

            if (!ShowClock)
            {
                return;
            }

            ShowDate = !Framework.Settings.Instance.DateSetting.Equals(Framework.DateSetting.Disabled);

            UpdateClock();
        }

        private void InitMonitors()
        {
            MonitorManager = new MonitorManager(Framework.Settings.Instance.MonitorConfig);
            MonitorManager.Update();
        }

        private void StartClock()
        {
            if (!ShowClock)
            {
                return;
            }

            _clockTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();
        }

        LaunchLib launch_data;
        private Launch current_launch;

        private void StartLaunchLib()
        {
            ShowLaunch = true;
            ShowLaunchDate = true;
            if (!ShowLaunch)
            {
                return;
            }
            

            using (WebClient client = new WebClient())
            {
                launch_data = 
                    LaunchLib.FromJson(
                        client.DownloadString(
                            "https://launchlibrary.net/1.2.2/launch?next=3&mode=list&tbddate=0&tbdtime=0"));
                current_launch = launch_data.Launches[0];
            }

            if (launch_data != null)
            {
                {
                    this.LaunchName = $"Next Launch: {current_launch.Name}";
                    
                }
            }
        }
        
        private void StartMonitors()
        {
            _monitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Framework.Settings.Instance.PollingInterval)
            };
            _monitorTimer.Tick += MonitorTimer_Tick;
            _monitorTimer.Start();
        }

        private void UpdateClock()
        {
            DateTime _now = DateTime.Now;

            Time = _now.ToString(Framework.Settings.Instance.Clock24HR ? "H:mm:ss" : "h:mm:ss tt");

            if (current_launch != null)
            {
                DateTime current_time_utc = DateTime.UtcNow;

                DateTime parsed_utc_time = DateTime.Parse(current_launch.Net.Replace("UTC", "Z"));

                TimeSpan remaining = parsed_utc_time.Subtract(current_time_utc);

                Double hours = Math.Round(remaining.TotalHours, 2);
                
                if (ShowLaunchDate)
                    this.LaunchDate = $"{current_launch.Net}\n T-{hours}:{remaining.Minutes}:{remaining.Seconds}";
            }
            
            if (ShowDate)
            {
                Date = _now.ToString(Framework.Settings.Instance.DateSetting.Format);
            }
        }

        private void UpdateMonitors()
        {
            MonitorManager.Update();
        }

        private void PauseClock()
        {
            _clockTimer?.Stop();
        }

        private void PauseMonitors()
        {
            _monitorTimer?.Stop();
        }

        private void ResumeClock()
        {
            _clockTimer?.Start();
        }

        private void ResumeMonitors()
        {
            _monitorTimer?.Start();
        }

        private void DisposeClock()
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer = null;
            }
        }

        private void DisposeMonitors()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Stop();
                _monitorTimer = null;
            }
            
            if (MonitorManager != null)
            {
                MonitorManager.Dispose();
                _monitorManager = null;
            }
        }

        private void ClockTimer_Tick(Object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void MonitorTimer_Tick(Object sender, EventArgs e)
        {
            UpdateMonitors();
        }

        private Boolean _ready { get; set; }

        public Boolean Ready
        {
            get => _ready;
            set
            {
                _ready = value;

                NotifyPropertyChanged("Ready");
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
        
        private String _time { get; set; }

        public String Time
        {
            get => _time;
            set
            {
                _time = value;

                NotifyPropertyChanged("Time");
            }
        }

        private Boolean _showDate { get; set; }

        public Boolean ShowDate
        {
            get => _showDate;
            set
            {
                _showDate = value;

                NotifyPropertyChanged("ShowDate");
            }
        }

        private String _date { get; set; }

        public String Date
        {
            get => _date;
            set
            {
                _date = value;

                NotifyPropertyChanged("Date");
            }
        }
        
        private Boolean _showLaunch { get; set; }

        public Boolean ShowLaunch
        {
            get => _showLaunch;
            set
            {
                _showLaunch = value;
                
                NotifyPropertyChanged("ShowLaunch");
            }
        }
        
        private String _launchName { get; set; }

        public String LaunchName
        {
            get => _launchName;
            set
            {
                _launchName = value;

                NotifyPropertyChanged("LaunchName");
            }
        }

        private Boolean _showLaunchDate { get; set; }

        public Boolean ShowLaunchDate
        {
            get => _showLaunchDate;
            set
            {
                _showLaunchDate = value;

                NotifyPropertyChanged("ShowLaunchDate");
            }
        }

        private String _launchDate { get; set; }

        public String LaunchDate
        {
            get => _launchDate;
            set
            {
                _launchDate = value;

                NotifyPropertyChanged("LaunchDate");
            }
        }
        
        private MonitorManager _monitorManager { get; set; }

        public MonitorManager MonitorManager
        {
            get => _monitorManager;
            set
            {
                _monitorManager = value;

                NotifyPropertyChanged("MonitorManager");
            }
        }

        private DispatcherTimer _clockTimer { get; set; }

        private DispatcherTimer _monitorTimer { get; set; }

        private Boolean _disposed { get; set; } = false;
    }
}
