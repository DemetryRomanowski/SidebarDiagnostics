﻿using System;
using System.Windows.Input;
using SidebarDiagnostics.Models;
using System.ComponentModel;
using SidebarDiagnostics.Style;

namespace SidebarDiagnostics
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : FlatWindow
    {
        public Graph(Sidebar sidebar)
        {
            InitializeComponent();

            DataContext = Model = new GraphModel(OPGraph);
            Model.BindData(sidebar.Model.MonitorManager);
            
            Show();
        }

        private void Window_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                OPGraph.ResetAllAxes();
            }
        }

        private void Window_Closing(Object sender, CancelEventArgs e)
        {
            DataContext = null;

            if (Model != null)
            {
                Model.Dispose();
                Model = null;
            }
        }

        public GraphModel Model { get; private set; }
    }
}
