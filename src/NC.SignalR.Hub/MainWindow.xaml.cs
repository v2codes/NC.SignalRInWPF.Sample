﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using NC.SignalR.Hub.ViewModels;

namespace NC.SignalR.Hub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = App.Current.Services.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null && e.Key == Key.C)
            {
                var sb = new StringBuilder();
                foreach (var item in _viewModel.ShowMessageContentList)
                {
                    sb.AppendLine(item);
                }
                Clipboard.SetDataObject(sb.ToString());
            }
        }
    }
}