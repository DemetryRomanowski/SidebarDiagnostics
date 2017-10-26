using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SidebarDiagnostics.Style;
using SidebarDiagnostics.Windows;

namespace SidebarDiagnostics
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : FlatWindow
    {
        public Setup()
        {
            InitializeComponent();

            Framework.Settings.Instance.ScreenIndex = 0;
            Framework.Settings.Instance.DockEdge = DockEdge.Right;
            Framework.Settings.Instance.XOffset = 0;
            Framework.Settings.Instance.YOffset = 0;

            Sidebar = new Dummy(this);
            Sidebar.Show();
        }

        private void ShowPage(Page page)
        {
            foreach (DockPanel _panel in SetupGrid.Children)
            {
                _panel.IsEnabled = _panel.Name == page.ToString();
            }

            CurrentPage = page;
        }

        private void Yes_Click(Object sender, RoutedEventArgs e)
        {
            switch (CurrentPage)
            {
                case Page.Initial:
                case Page.BeginCustom:
                    ShowPage(Page.Final);
                    return;
            }
        }

        private void No_Click(Object sender, RoutedEventArgs e)
        {
            switch (CurrentPage)
            {
                case Page.Initial:
                    ShowPage(Page.BeginCustom);
                    return;
            }
        }

        private void OffsetSlider_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<Double> e)
        {
            if (_cancelReposition != null)
            {
                _cancelReposition.Cancel();
            }

            _cancelReposition = new CancellationTokenSource();

            Task.Delay(TimeSpan.FromMilliseconds(500), _cancelReposition.Token).ContinueWith(_ =>
            {
                if (_.IsCanceled)
                {
                    return;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    Framework.Settings.Instance.XOffset = (Int32)XOffsetSlider.Value;
                    Framework.Settings.Instance.YOffset = (Int32)YOffsetSlider.Value;

                    Sidebar.Reposition();
                }));

                _cancelReposition = null;
            });
        }

        private void NumberBox_PreviewTextInput(Object sender, TextCompositionEventArgs e)
        {
            if (new Regex("[^0-9.-]+").IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void Settings_Click(Object sender, RoutedEventArgs e)
        {
            _openSettings = true;

            Close();
        }

        private void Close_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(Object sender, EventArgs e)
        {
            if (Sidebar != null && Sidebar.IsInitialized)
            {
                Sidebar.Close();
            }

            Framework.Settings.Instance.InitialSetup = false;
            Framework.Settings.Instance.Save();

            App.StartApp(_openSettings);
        }

        public Dummy Sidebar { get; private set; }

        public Page CurrentPage { get; set; } = Page.Initial;

        private CancellationTokenSource _cancelReposition { get; set; }

        private Boolean _openSettings { get; set; } = false;

        public enum Page : byte
        {
            Initial,
            BeginCustom,
            Final
        }
    }
}