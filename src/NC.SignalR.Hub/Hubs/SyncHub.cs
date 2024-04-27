using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NC.SignalR.Hub.ViewModels;

namespace NC.SignalR.Hub.Hubs
{
    public class SyncHub : Hub<ISyncClient>
    {
        private MainWindowViewModel _viewModel;
        private ClientStorage _clientStorage;

        public SyncHub(ClientStorage clientStorage)
        {
            _viewModel = App.Current.Services.GetRequiredService<MainWindowViewModel>();
            _clientStorage = clientStorage;
        }

        public async Task SendMessage(string message)
        {
            var clientName = Context.GetCurrentClient();
            _viewModel.ShowMessage($"收到:{clientName},{message}", false, true);
            //await Clients.All.SendAsync("ReceiveMessage", "WCS", message);
            //_viewModel.AppendMessage($"发送：{message}");
        }

        public async Task<string> InvokeMessage(string message)
        {
            var clientName = Context.GetCurrentClient();
            _viewModel.ShowMessage($"收到:{clientName},{message}", false, true);
            //await Clients.All.SendAsync("ReceiveMessage", "WCS", message);
            //_viewModel.AppendMessage($"发送：{message}");
            return $"Resp-{message}";
        }

        public async Task SendImage(string fileName, byte[] data)
        {
            var clientName = Context.GetCurrentClient();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var file = Path.Combine(filePath, fileName);
            await File.WriteAllBytesAsync(file, data);
            _viewModel.ShowMessage($"收到:{clientName},图片已保存[{file}]");
        }

        public override async Task OnConnectedAsync()
        {
            var clientName = Context.GetCurrentClient();
            _clientStorage.TryAdd(Context.ConnectionId, clientName);
            //await Groups.AddToGroupAsync(Context.ConnectionId, "ClientGroup");
            _viewModel.ShowMessage($"客户端接入：{clientName}！");
            _viewModel.ConnectedClientCount = _clientStorage.Count();
            await base.OnConnectedAsync();
        }

        private object _lockCount = new();
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientName = Context.GetCurrentClient();
            _clientStorage.TryRemove(clientName);
            _viewModel.ShowMessage($"客户端断开连接:{clientName} {exception?.Message}!");
            _viewModel.ConnectedClientCount = _clientStorage.Count();
            lock (_lockCount)
            {
                _viewModel.DisconnectCount += 1;
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
