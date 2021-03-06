﻿using System;
using System.Windows;
using SidebarDiagnostics.Models;
using SidebarDiagnostics.Style;

namespace SidebarDiagnostics
{
    /// <summary>
    /// Interaction logic for ChangeLog.xaml
    /// </summary>
    public partial class ChangeLog : FlatWindow
    {
        public ChangeLog(Version version)
        {
            InitializeComponent();

            DataContext = Model = new ChangeLogModel(version);
        }

        private void Close_Click(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        public ChangeLogModel Model { get; private set; }
    }
}
