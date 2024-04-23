using Microsoft.Extensions.DependencyInjection;
using NC.SignalR.Client.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NC.SignalR.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }


        public App()
        {
            Services = ConfigureServices();
        }


        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainWindowViewModel>();
            return services.BuildServiceProvider();
        }
    }

}
