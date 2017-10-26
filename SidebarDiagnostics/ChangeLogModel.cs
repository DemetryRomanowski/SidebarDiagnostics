using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SidebarDiagnostics.Framework;
using SidebarDiagnostics.Utilities;

namespace SidebarDiagnostics.Models
{
    public class ChangeLogModel : INotifyPropertyChanged
    {
        public ChangeLogModel(Version version)
        {
            String _vstring = version.ToString(3);

            Title = String.Format("{0} v{1}", Resources.ChangeLogTitle, _vstring);

            ChangeLogEntry _log = ChangeLogEntry.Load().FirstOrDefault(e => String.Equals(e.Version, _vstring, StringComparison.OrdinalIgnoreCase));

            if (_log != null)
            {
                Changes = _log.Changes;
            }
            else
            {
                Changes = new String[0];
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

        private String _title { get; set; }

        public String Title
        {
            get => _title;
            set
            {
                _title = value;

                NotifyPropertyChanged("Title");
            }
        }

        private String[] _changes { get; set; }

        public String[] Changes
        {
            get => _changes;
            set
            {
                _changes = value;

                NotifyPropertyChanged("Changes");
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ChangeLogEntry
    {
        public static ChangeLogEntry[] Load()
        {
            ChangeLogEntry[] _return = null;

            String _file = Paths.ChangeLog;

            if (File.Exists(_file))
            {
                using (StreamReader _reader = File.OpenText(_file))
                {
                    _return = (ChangeLogEntry[])new JsonSerializer().Deserialize(_reader, typeof(ChangeLogEntry[]));
                }
            }

            return _return ?? new ChangeLogEntry[0];
        }

        [JsonProperty]
        public String Version { get; set; }

        [JsonProperty]
        public String[] Changes { get; set; }
    }
}
