using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NC.SignalR.Hub.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System.Windows;

namespace NC.SignalR.Hub.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Property
        [ObservableProperty]
        private string _hubServerUrl;

        [ObservableProperty]
        private string _btnConnectText;

        [ObservableProperty]
        private string _txtInputMessage;

        [ObservableProperty]
        private decimal _sendMessageCount;

        [ObservableProperty]
        private decimal _receivedMessageCount;

        [ObservableProperty]
        private int _connectedClientCount;

        [ObservableProperty]
        private BindingList<string> _showMessageContentList;
        #endregion

        #region Command
        public AsyncRelayCommand StartSignalRServerCommand { get; set; }
        public AsyncRelayCommand SendMessageCommand { get; set; }
        public AsyncRelayCommand ClearShowMessageCommand { get; set; }
        #endregion

        private IHost _host;
        private bool _hostIsRunning = false;

        public MainWindowViewModel()
        {
            ThreadPool.GetMaxThreads(out var workerThreads, out var ioThreads);
            HubServerUrl = "http://*.*:8080";
            BtnConnectText = "启动服务";
            ConnectedClientCount = 0;
            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            ShowMessageContentList = new BindingList<string>();
            StartSignalRServerCommand = new AsyncRelayCommand(StartSignalRServerAsync);
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
            ClearShowMessageCommand = new AsyncRelayCommand(ClearShowMessageAsync);
        }

        #region SignalR Server
        private async Task StartSignalRServerAsync()
        {
            if (!_hostIsRunning)
            {
                _host = CreateSignalRWebHost();
                await _host.StartAsync();
                _hostIsRunning = true;
                SendMessageCount = 0;
                ReceivedMessageCount = 0;
                BtnConnectText = "关闭服务";
                ShowMessage("SignalR服务已启动！");
            }
            else
            {
                await _host.StopAsync();
                _hostIsRunning = false;
                BtnConnectText = "启动服务";
                ShowMessage("SignalR服务已关闭！");
            }
        }

        private IHost CreateSignalRWebHost()
        {
            var host = Host.CreateDefaultBuilder()
                           .ConfigureWebHostDefaults(builder =>
                           {
                               builder.UseUrls(HubServerUrl)
                                      .ConfigureServices(services =>
                                      {
                                          services.AddControllers();
                                          // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                                          services.AddEndpointsApiExplorer();
                                          services.AddSwaggerGen();
                                          services.AddSignalR(options =>
                                          {
                                              // 如果服务端在30s内收到客户端发送任何消息，那么则会认为已经断开连接，则进入OnDisconnectedAsync方法
                                              // 实际上 客户端此时可能并没有断开连接，或者断开连接还需要一段时间，因为客户端断开连接是走的另外一套机制的。【以服务器端为基准，判断客户端是否断开连接，从而断开服务器端连接】
                                              options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                                              // 如果服务端未在15s内向客户端发送消息，在15s的时候会自动ping客户端，是连接保持打开的状态。
                                              options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                                              options.EnableDetailedErrors = true;
                                              options.MaximumReceiveMessageSize = 102400000;
                                          })
                                          .AddMessagePackProtocol();

                                          // 注入客户端缓存
                                          services.AddSingleton(new ClientStorage());
                                      })
                                      .Configure(app =>
                                      {
                                          app.UseHttpsRedirection();
                                          app.UseSwagger();
                                          app.UseSwaggerUI();
                                          app.UseCors();
                                          app.UseRouting();
                                          app.UseEndpoints(endpoints => endpoints.MapHub<SyncHub>("/sync"));
                                      });
                           }).Build();
            return host;
        }
        #endregion

        private async Task SendMessageAsync()
        {
            var hubContext = _host.Services.GetService<IHubContext<SyncHub>>();
            if (hubContext != null)
            {
                // 约定参数MethodName、user、message
                //hubContext.Clients.Client("xxx").SendAsync("ReceiveMessage", TxtInputMessage);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", TxtInputMessage);
                ShowMessage($"广播发送:{TxtInputMessage}");
                SendMessageCount += 1;
            }
        }

        private async Task ClearShowMessageAsync()
        {
            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            ShowMessageContentList.Clear();
        }

        public void ShowMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowMessageContentList.Insert(0, $"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{message}");
            });
        }
    }
}
