using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using NC.SignalR.Client.ViewModels;

namespace NC.SignalR.Client
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