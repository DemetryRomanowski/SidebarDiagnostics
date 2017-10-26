using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32.TaskScheduler;
using SidebarDiagnostics.Framework;

namespace SidebarDiagnostics.Utilities
{
    public static class Paths
    {
        private const String SETTINGS = "settings.json";
        private const String CHANGELOG = "ChangeLog.json";

        public static String Install(Version version)
        {
            return Path.Combine(LocalApp, String.Format("app-{0}", version.ToString(3)));
        }

        public static String Exe(Version version)
        {
            return Path.Combine(Install(version), ExeName);
        }

        public static String ChangeLog => Path.Combine(CurrentDirectory, CHANGELOG);

        public static String CurrentDirectory => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        public static String TaskBar => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");

        private static String _assemblyName { get; set; } = null;

        public static String AssemblyName
        {
            get
            {
                if (_assemblyName == null)
                {
                    _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                }

                return _assemblyName;
            }
        }

        private static String _exeName { get; set; } = null;

        public static String ExeName
        {
            get
            {
                if (_exeName == null)
                {
                    _exeName = String.Format("{0}.exe", AssemblyName);
                }

                return _exeName;
            }
        }

        private static String _settingsFile { get; set; } = null;

        public static String SettingsFile
        {
            get
            {
                if (_settingsFile == null)
                {
                    _settingsFile = Path.Combine(LocalApp, SETTINGS);
                }

                return _settingsFile;
            }
        }

        private static String _localApp { get; set; } = null;

        public static String LocalApp
        {
            get
            {
                if (_localApp == null)
                {
                    _localApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AssemblyName);
                }

                return _localApp;
            }
        }
    }

    public static class Startup
    {        
        public static Boolean StartupTaskExists()
        {
            using (TaskService _taskService = new TaskService())
            {
                Task _task = _taskService.FindTask(Constants.Generic.TASKNAME);

                if (_task == null)
                {
                    return false;
                }

                ExecAction _action = _task.Definition.Actions.OfType<ExecAction>().FirstOrDefault();

                if (_action == null || _action.Path != Assembly.GetExecutingAssembly().Location)
                {
                    return false;
                }

                return true;
            }
        }

        public static void EnableStartupTask(String exePath = null)
        {
            using (TaskService _taskService = new TaskService())
            {
                TaskDefinition _def = _taskService.NewTask();
                _def.Triggers.Add(new LogonTrigger() { Enabled = true });
                _def.Actions.Add(new ExecAction(exePath ?? Assembly.GetExecutingAssembly().Location));
                _def.Principal.RunLevel = TaskRunLevel.Highest;

                _def.Settings.DisallowStartIfOnBatteries = false;
                _def.Settings.StopIfGoingOnBatteries = false;
                _def.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                _taskService.RootFolder.RegisterTaskDefinition(Constants.Generic.TASKNAME, _def);
            }
        }

        public static void DisableStartupTask()
        {
            using (TaskService _taskService = new TaskService())
            {
                _taskService.RootFolder.DeleteTask(Constants.Generic.TASKNAME, false);
            }
        }
    }

    public static class Culture
    {
        public const String DEFAULT = "Default";

        public static void SetDefault()
        {
            Default = Thread.CurrentThread.CurrentUICulture;
        }

        public static void SetCurrent(Boolean init)
        {
            SetCurrent(Framework.Settings.Instance.Culture, init);
        }

        public static void SetCurrent(String name, Boolean init)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = String.Equals(name, DEFAULT, StringComparison.Ordinal) ? Default : new CultureInfo(name);

            if (init)
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
            }
        }

        public static CultureItem[] GetAll()
        {
            return new CultureItem[1] { new CultureItem() { Value = DEFAULT, Text = Resources.SettingsLanguageDefault } }.Concat(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => Languages.Contains(c.TwoLetterISOLanguageName)).OrderBy(c => c.DisplayName).Select(c => new CultureItem() { Value = c.Name, Text = c.DisplayName })).ToArray();
        }

        public static String[] Languages => new String[7] { "en", "da", "de", "fr", "ja", "nl", "zh" };

        public static CultureInfo Default { get; private set; }
    }

    public class CultureItem
    {
        public String Value { get; set; }

        public String Text { get; set; }
    }
}
