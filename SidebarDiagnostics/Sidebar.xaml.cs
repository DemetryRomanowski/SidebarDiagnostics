using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SidebarDiagnostics.JsonObjects;
using SidebarDiagnostics.Models;
using SidebarDiagnostics.Windows;

namespace SidebarDiagnostics
{
    /// <inheritdoc cref="http://www.google.com/" />
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : AppBarWindow
    {
        public Sidebar(Boolean openSettings, Boolean initiallyHidden)
        {
            InitializeComponent();

            _openSettings = openSettings;
            _initiallyHidden = initiallyHidden;
        }

        public void Reload()
        {
            if (!Ready)
            {
                return;
            }

            Ready = false;

            App._reloading = true;

            Close();
        }

        public async Task Reset(Boolean enableHotkeys)
        {
            if (!Ready)
            {
                return;
            }

            Ready = false;

            BindSettings(enableHotkeys);

            await BindModel();
        }

        public void Reposition()
        {
            if (!Ready)
            {
                return;
            }

            Ready = false;

            BindPosition(() => Ready = true);
        }

        public void ContentReload()
        {
            if (!Ready)
            {
                return;
            }

            Ready = false;

            Model.Reload();

            Ready = true;

            BindGraphs();
        }

        public override void AppBarShow()
        {
            base.AppBarShow();

            Model.Resume();
        }

        public override void AppBarHide()
        {
            base.AppBarHide();

            Model.Pause();
        }

        private async Task Initialize()
        {
            Ready = false;

            Devices.AddHook(this);

            DisableAeroPeek();

            BindSettings(true);

            await BindModel();
        }

        private void BindSettings(Boolean enableHotkeys)
        {
            BindPosition(null);

            if (Framework.Settings.Instance.AlwaysTop)
            {
                SetTopMost(false);

                ShowDesktop.RemoveHook();
            }
            else
            {
                ClearTopMost(false);

                ShowDesktop.AddHook(this);
            }

            if (Framework.Settings.Instance.ClickThrough)
            {
                SetClickThrough();
            }
            else
            {
                ClearClickThrough();
            }

            if (Framework.Settings.Instance.ToolbarMode)
            {
                HideInAltTab();
            }
            else
            {
                ShowInAltTab();
            }

            if (WindowControls.Visibility != Visibility.Visible)
            {
                if (Framework.Settings.Instance.CollapseMenuBar)
                {
                    WindowControls.Visibility = Visibility.Collapsed;
                }
                else
                {
                    WindowControls.Visibility = Visibility.Hidden;
                }
            }

            Hotkey.Initialize(this, Framework.Settings.Instance.Hotkeys);

            if (enableHotkeys)
            {
                Hotkey.Enable();
            }
        }

        private void BindPosition(Action callback)
        {
            Int32 _screen;
            DockEdge _edge;
            WorkArea _windowWA;
            WorkArea _appbarWA;

            Monitor.GetWorkArea(this, out _screen, out _edge, out _windowWA, out _appbarWA);

            Move(_windowWA);

            SetAppBar(_screen, _edge, _windowWA, _appbarWA, callback);
        }
        
        private async Task BindModel()
        {
            await Task.Run(async () =>
            {
                if (Model != null)
                {
                    Model.Dispose();
                    Model = null;
                }

                await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ModelReadyHandler(ModelReady), new SidebarModel());
            });
        }

        private delegate void ModelReadyHandler(SidebarModel model);

        private void ModelReady(SidebarModel model)
        {
            DataContext = Model = model;
            model.Start();

            Ready = true;

            BindGraphs();

            if (_openSettings)
            {
                _openSettings = false;

                App.Current.OpenSettings();
            }

            if (_initiallyHidden)
            {
                _initiallyHidden = false;

                AppBarHide();
            }
        }

        private void BindGraphs()
        {
            foreach (Graph _graph in App.Current.Graphs)
            {
                _graph.Model.BindData(Model.MonitorManager);
            }
        }

        private void GraphButton_Click(Object sender, RoutedEventArgs e)
        {
            App.Current.OpenGraph();
        }

        private void SettingsButton_Click(Object sender, RoutedEventArgs e)
        {
            App.Current.OpenSettings();
        }

        private void CloseButton_Click(Object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        
        private void Window_MouseEnter(Object sender, MouseEventArgs e)
        {
            WindowControls.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeave(Object sender, MouseEventArgs e)
        {
            WindowControls.Visibility = Framework.Settings.Instance.CollapseMenuBar ? Visibility.Collapsed : Visibility.Hidden;
        }

        private async void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            await Initialize();
            
        }

        private void Window_StateChanged(Object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Window_Closing(Object sender, CancelEventArgs e)
        {
            Ready = false;

            DataContext = null;

            if (Model != null)
            {
                Model.Dispose();
                Model = null;
            }

            ClearAppBar();

            Devices.RemoveHook(this);
            ShowDesktop.RemoveHook();
            Hotkey.Dispose();
        }

        private void Window_Closed(Object sender, EventArgs e)
        {
            if (App._reloading)
            {
                App._reloading = false;

                new Sidebar(false, false).Show();
            }
            else
            {
                App.Current.Shutdown();
            }
        }

        private Boolean _ready { get; set; } = false;

        public Boolean Ready
        {
            get => _ready;
            set
            {
                _ready = value;

                if (Model != null)
                {
                    Model.Ready = value;
                }
            }
        }

        public SidebarModel Model { get; private set; }

        private Boolean _openSettings { get; set; } = false;

        private Boolean _initiallyHidden { get; set; } = false;
    }
}