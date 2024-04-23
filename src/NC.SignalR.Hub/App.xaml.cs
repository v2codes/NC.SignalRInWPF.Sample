using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NC.SignalR.Hub.ViewModels;

namespace NC.SignalR.Hub
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
