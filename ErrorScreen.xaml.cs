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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace beforewindeploy_custom_recovery
{
    /// <summary>
    /// Interaction logic for ComponentSelection.xaml
    /// </summary>
    public partial class ErrorScreen : Page
    {
        public ErrorScreen(string message)
        {
            InitializeComponent();
            errorLabel.Content = message;
        }
    }
}
