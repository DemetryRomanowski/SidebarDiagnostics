using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using OxyPlot.Wpf;
using SidebarDiagnostics.Framework;
using SidebarDiagnostics.Monitoring;

namespace SidebarDiagnostics.Models
{
    public class GraphModel : INotifyPropertyChanged, IDisposable
    {
        public GraphModel(Plot plot)
        {
            _plot = plot;
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
                    _monitorItems = null;
                    _monitor = null;

                    _hardwareItems = null;
                    _hardware = null;

                    _metricItems = null;

                    if (_metrics != null)
                    {
                        _metrics.CollectionChanged -= Metrics_CollectionChanged;

                        foreach (iMetric _metric in _metrics)
                        {
                            _metric.PropertyChanged -= Metric_PropertyChanged;
                        }

                        _metrics = null;
                    }

                    _plot = null;
                    _data = null;
                }

                _disposed = true;
            }
        }

        ~GraphModel()
        {
            Dispose(false);
        }

        public void BindData(MonitorManager manager)
        {
            BindMonitors(manager.MonitorPanels);

            ExpandConfig = true;
        }

        public void SetupPlot()
        {
            _data = new Dictionary<iMetric, ObservableCollection<MetricRecord>>();

            _plot.Series.Clear();

            foreach (iMetric _metric in Metrics)
            {
                ObservableCollection<MetricRecord> _records = new ObservableCollection<MetricRecord>();

                _data.Add(_metric, _records);
                
                _metric.PropertyChanged += Metric_PropertyChanged;

                _plot.Series.Add(
                    new LineSeries()
                    {
                        Title = _metric.FullName,
                        TrackerFormatString = String.Format("{0}\r\n{{Value:#,##0.##}}{1}\r\n{{Recorded:T}}", _metric.FullName, _metric.nAppend),
                        ItemsSource = _records,
                        DataFieldX = "Recorded",
                        DataFieldY = "Value"
                    });
            }
        }

        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BindMonitors(MonitorPanel[] panels)
        {
            MonitorItems = panels;

            if (panels.Length > 0)
            {
                Monitor = panels[0];
            }
            else
            {
                Monitor = null;
            }
        }

        private void BindHardware(iMonitor[] monitors)
        {
            HardwareItems = monitors;

            if (monitors.Length > 0)
            {
                Hardware = monitors[0];
            }
            else
            {
                Hardware = null;
            }
        }

        private void BindMetrics(iMetric[] metrics)
        {
            MetricItems = metrics;
            Metrics = new ObservableCollection<iMetric>();
        }

        private void Metrics_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (iMetric _metric in e.OldItems)
                {
                    _metric.PropertyChanged -= Metric_PropertyChanged;
                }
            }

            SetupPlot();
        }

        private void Metric_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (_disposed)
            {
                (sender as iMetric).PropertyChanged -= Metric_PropertyChanged;
                return;
            }

            if (e.PropertyName != "nValue")
            {
                return;
            }

            iMetric _metric = (iMetric)sender;

            if (_data == null || !_data.ContainsKey(_metric))
            {
                _metric.PropertyChanged -= Metric_PropertyChanged;
                return;
            }

            DateTime _now = DateTime.Now;

            try
            {
                ObservableCollection<MetricRecord> _mData = _data[_metric];

                foreach (MetricRecord _record in _mData.Where(r => (_now - r.Recorded).TotalSeconds > Duration).ToArray())
                {
                    _mData.Remove(_record);
                }

                _mData.Add(new MetricRecord(_metric.nValue, _now));
            }
            catch
            {
                _metric.PropertyChanged -= Metric_PropertyChanged;
            }
        }

        private String _title { get; set; } = Resources.GraphTitle;

        public String Title
        {
            get => _title;
            set
            {
                _title = value;

                NotifyPropertyChanged("Title");
            }
        }

        private MonitorPanel[] _monitorItems { get; set; }

        public MonitorPanel[] MonitorItems
        {
            get => _monitorItems;
            set
            {
                _monitorItems = value;

                NotifyPropertyChanged("MonitorItems");
            }
        }

        private MonitorPanel _monitor { get; set; }

        public MonitorPanel Monitor
        {
            get => _monitor;
            set
            {
                _monitor = value;

                if (_monitor == null)
                {
                    BindHardware(new iMonitor[0]);
                }
                else
                {
                    BindHardware(_monitor.Monitors);
                }

                NotifyPropertyChanged("Monitor");
            }
        }

        private iMonitor[] _hardwareItems { get; set; }

        public iMonitor[] HardwareItems
        {
            get => _hardwareItems;
            set
            {
                _hardwareItems = value;

                NotifyPropertyChanged("HardwareItems");
            }
        }

        private iMonitor _hardware { get; set; }

        public iMonitor Hardware
        {
            get => _hardware;
            set
            {
                _hardware = value;

                if (_hardware == null)
                {
                    BindMetrics(new iMetric[0]);

                    Title = Resources.GraphTitle;
                }
                else
                {
                    BindMetrics(_hardware.Metrics.Where(m => m.IsNumeric).ToArray());

                    Title = String.Format("{0} - {1}", Resources.GraphTitle, _hardware.Name);
                }

                NotifyPropertyChanged("Hardware");
            }
        }

        private iMetric[] _metricItems { get; set; }

        public iMetric[] MetricItems
        {
            get => _metricItems;
            set
            {
                _metricItems = value;

                NotifyPropertyChanged("MetricItems");
            }
        }

        private ObservableCollection<iMetric> _metrics { get; set; }

        public ObservableCollection<iMetric> Metrics
        {
            get => _metrics;
            set
            {
                if (_metrics != null)
                {
                    foreach (iMetric _metric in _metrics)
                    {
                        _metric.PropertyChanged -= Metric_PropertyChanged;
                    }
                }

                _metrics = value;

                if (_metrics != null)
                {
                    SetupPlot();

                    _metrics.CollectionChanged += Metrics_CollectionChanged;
                }

                NotifyPropertyChanged("Metrics");
            }
        }

        public DurationItem[] DurationItems => new DurationItem[5]
        {
            new DurationItem(15, String.Format("15 {0}", Resources.GraphDurationSeconds)),
            new DurationItem(30, String.Format("30 {0}", Resources.GraphDurationSeconds)),
            new DurationItem(60, String.Format("1 {0}", Resources.GraphDurationMinute)),
            new DurationItem(300, String.Format("5 {0}", Resources.GraphDurationMinutes)),
            new DurationItem(900, String.Format("15 {0}", Resources.GraphDurationMinutes))
        };

        private Int32 _duration { get; set; } = 15;

        public Int32 Duration
        {
            get => _duration;
            set
            {
                _duration = value;

                NotifyPropertyChanged("Duration");
            }
        }

        private Boolean _expandConfig { get; set; } = true;

        public Boolean ExpandConfig
        {
            get => _expandConfig;
            set
            {
                _expandConfig = value;

                NotifyPropertyChanged("ExpandConfig");
            }
        }

        private Plot _plot { get; set; }

        private Dictionary<iMetric, ObservableCollection<MetricRecord>> _data { get; set; }

        private Boolean _disposed { get; set; } = false;
    }

    public class DurationItem
    {
        public DurationItem(Int32 seconds, String text)
        {
            Seconds = seconds;
            Text = text;
        }

        public Int32 Seconds { get; set; }

        public String Text { get; set; }
    }

    public class MetricRecord
    {
        public MetricRecord(Double value, DateTime recorded)
        {
            Value = value > 0 ? value : 0.001d;
            Recorded = recorded;
        }

        public Double Value { get; set; }

        public DateTime Recorded { get; set; }
    }
}
