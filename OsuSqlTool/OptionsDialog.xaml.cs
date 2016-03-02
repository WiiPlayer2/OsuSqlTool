﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsuSqlTool
{
    /// <summary>
    /// Interaktionslogik für OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!DialogResult.HasValue
                || !DialogResult.Value)
            {
                Settings.Instance.Reload();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.Save();
            DialogResult = true;
            Close();
        }
    }
}