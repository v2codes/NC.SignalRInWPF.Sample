using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace NC.SignalR.Client.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Property
        private string _hubServerUrl;
        public string HubServerUrl
        {
            get => _hubServerUrl;
            set => SetProperty(ref _hubServerUrl, value);
        }

        private string _btnConnectText;
        public string BtnConnectText
        {
            get => _btnConnectText;
            set => SetProperty(ref _btnConnectText, value);
        }

        private string _txtInputMessage;
        public string TxtInputMessage
        {
            get => _txtInputMessage;
            set => SetProperty(ref _txtInputMessage, value);
        }

        private int _clientCount;
        public int ClientCount
        {
            get => _clientCount;
            set => SetProperty(ref _clientCount, value);
        }

        private int _messageLenth;
        public int MessageLenth
        {
            get => _messageLenth;
            set => SetProperty(ref _messageLenth, value);
        }

        private int _sendInterval;
        public int SendInterval
        {
            get => _sendInterval;
            set => SetProperty(ref _sendInterval, value);
        }

        private int _messageCount;
        public int MessageCount
        {
            get => _messageCount;
            set => SetProperty(ref _messageCount, value);
        }

        private string _showMessageContent;
        public string ShowMessageContent
        {
            get => _showMessageContent;
            set => SetProperty(ref _showMessageContent, value);
        }

        private int _sendMessageCount;
        public int SendMessageCount
        {
            get => _sendMessageCount;
            set => SetProperty(ref _sendMessageCount, value);
        }

        private int _receivedMessageCount;
        public int ReceivedMessageCount
        {
            get => _receivedMessageCount;
            set => SetProperty(ref _receivedMessageCount, value);
        }

        private bool _btnStartLongRunningEnable;
        public bool BtnStartLongRunningEnable
        {
            get => _btnStartLongRunningEnable;
            set => SetProperty(ref _btnStartLongRunningEnable, value);
        }

        private bool _btnStopTestEnable;
        public bool BtnStopTestEnable
        {
            get => _btnStopTestEnable;
            set => SetProperty(ref _btnStopTestEnable, value);
        }

        private bool _btnStartRationRunningEnable;
        public bool BtnStartRationRunningEnable
        {
            get => _btnStartRationRunningEnable;
            set => SetProperty(ref _btnStartRationRunningEnable, value);
        }

        private bool _isWaitServerResponse;
        public bool IsWaitServerResponse
        {
            get => _isWaitServerResponse;
            set => SetProperty(ref _isWaitServerResponse, value);
        }

        #endregion

        #region Command
        public AsyncRelayCommand ConnectSignalRServerCommand { get; set; }
        public AsyncRelayCommand SendMessageCommand { get; set; }
        public AsyncRelayCommand SendImageCommand { get; set; }
        public AsyncRelayCommand InvokeMessageCommand { get; set; }
        #endregion

        #region Test Command
        private ConcurrentDictionary<int, HubConnection> _testHubConnectionList { get; set; }
        public AsyncRelayCommand StartLongRunningCommand { get; set; }
        public AsyncRelayCommand StartRationRunningCommand { get; set; }
        public AsyncRelayCommand StopTestCommand { get; set; }
        private CancellationTokenSource SendMessageCancelTokenSource { get; set; }
        #endregion

        private HubConnection _hubConnection { get; set; }

        public MainWindowViewModel()
        {
            #region Property
            HubServerUrl = "http://localhost:8080";
            BtnConnectText = "连接";
            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            BtnStartLongRunningEnable = true;
            BtnStopTestEnable = false;

            ClientCount = 1;
            MessageLenth = 10;
            SendInterval = 10;
            MessageCount = 1000;

            _testHubConnectionList = new ConcurrentDictionary<int, HubConnection>();
            #endregion

            ConnectSignalRServerCommand = new AsyncRelayCommand(ConnectSignalRServerAsync);
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
            InvokeMessageCommand = new AsyncRelayCommand(InvokeMessageAsync);
            SendImageCommand = new AsyncRelayCommand(SendImageAsync);
            StartLongRunningCommand = new AsyncRelayCommand(StartLongRunningAsync);
            StartRationRunningCommand = new AsyncRelayCommand(StartRationRunningAsync);
            StopTestCommand = new AsyncRelayCommand(StopTestAsync);
        }

        #region 手动测试
        private async Task ConnectSignalRServerAsync()
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                var clientName = "Client-X";
                _hubConnection = CreateHubConnection(clientName);
                try
                {
                    await _hubConnection.StartAsync();
                    ShowMessage($"{clientName}，连接成功！");
                    BtnConnectText = "断开";
                }
                catch (Exception ex)
                {
                    ShowMessage($"{clientName}，连接失败：{ex.Message}");
                }
            }
            else
            {
                await DisconnectSignalRServerAsync();
            }
        }

        private async Task DisconnectSignalRServerAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
                _hubConnection?.DisposeAsync();
                BtnConnectText = "连接";
            }
        }

        private async Task SendMessageAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                // 约定参数MethodName、user、message
                await _hubConnection.SendAsync("SendMessage", TxtInputMessage);
                ShowMessage($"Client-X发送:{TxtInputMessage}");
                SendMessageCount += 1;
            }
            else
            {
                ShowMessage($"Client-X发送失败,未连接到SignalR服务器!");
            }
        }

        private async Task SendImageAsync()
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "Image";
            dialog.DefaultExt = ".jpg";
            dialog.Multiselect = false;
            dialog.Filter = "Image File (.jpg)|*.jpg|*.png|*.jpeg"; // Filter files by extension

            // Show open file dialog box
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                var fileName = Path.GetFileName(dialog.FileName);
                var data = await File.ReadAllBytesAsync(dialog.FileName);
                await _hubConnection.SendAsync("SendImage", fileName, data);
                ShowMessage($"Client-X发送文件:{fileName}");
            }
            else
            {
                ShowMessage($"Client-X发送文件失败,未连接到SignalR服务器!");
            }
        }

        private async Task InvokeMessageAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                // 约定参数MethodName、user、message
                var result = await _hubConnection.InvokeAsync<string>("GetData", TxtInputMessage);
                ShowMessage($"Client-X发送:{TxtInputMessage},返回:{result}");
                SendMessageCount += 1;
            }
            else
            {
                ShowMessage($"Client-X发送失败,未连接到SignalR服务器!");
            }
        }
        #endregion

        #region 模拟测试
        private async Task StartLongRunningAsync()
        {
            #region 输入校验
            if (ClientCount <= 0)
            {
                ShowMessage("请输入有效的客户端数量！");
                return;
            }

            if (MessageLenth <= 0)
            {
                ShowMessage("请输入有效的消息长度！");
                return;
            }

            if (SendInterval < 0)
            {
                ShowMessage("请输入有效的发送间隔！");
                return;
            }
            #endregion

            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            BtnStartLongRunningEnable = false;
            BtnStartRationRunningEnable = false;
            BtnStopTestEnable = true;

            // 停止手动连接
            await DisconnectSignalRServerAsync();
            _testHubConnectionList.Clear();

            #region 创建客户端并连接服务器
            var list = Enumerable.Range(1, ClientCount);
            await Parallel.ForEachAsync(list, async (index, _) =>
            {
                var clientName = $"Client-{index}";
                var connection = CreateHubConnection(clientName);
                try
                {
                    await connection.StartAsync();
                    _testHubConnectionList.TryAdd(index, connection);
                    ShowMessage($"{clientName}，连接成功：{connection.ConnectionId}！");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{clientName}，连接失败：{connection.ConnectionId} {ex.Message}！");
                }
            });
            ShowMessage($"模拟客户端 {ClientCount} 个，已创建完成， 已连接服务器，启动消息推送...");
            #endregion

            #region 推送消息
            SendMessageCancelTokenSource = new CancellationTokenSource();
            var randomString = GenerateRandomString(MessageLenth);
            var slim = new SemaphoreSlim(1);
            await Parallel.ForEachAsync(_testHubConnectionList, SendMessageCancelTokenSource.Token, async (connection, token) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    var stopwatch = new Stopwatch();
                    while (true)
                    {
                        await SendAsync(connection.Key, connection.Value, randomString, stopwatch, slim);

                        if (SendMessageCancelTokenSource.IsCancellationRequested)
                        {
                            await connection.Value.StopAsync();
                            token.ThrowIfCancellationRequested();
                        }
                        await Task.Delay(SendInterval);
                    }
                }, SendMessageCancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            });
            #endregion
        }

        private async Task StartRationRunningAsync()
        {
            #region 输入校验
            if (ClientCount <= 0)
            {
                ShowMessage("请输入有效的客户端数量！");
                return;
            }

            if (MessageLenth <= 0)
            {
                ShowMessage("请输入有效的消息长度！");
                return;
            }

            if (SendInterval < 0)
            {
                ShowMessage("请输入有效的发送间隔！");
                return;
            }

            if (MessageCount <= 0)
            {
                ShowMessage("请输入有效的发送次数！");
                return;
            }
            #endregion

            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            BtnStartLongRunningEnable = false;
            BtnStartRationRunningEnable = false;
            BtnStopTestEnable = true;

            // 停止手动连接
            await DisconnectSignalRServerAsync();
            _testHubConnectionList.Clear();

            #region 创建客户端并连接服务器
            var list = Enumerable.Range(1, ClientCount);
            await Parallel.ForEachAsync(list, async (index, _) =>
            {
                var clientName = $"Client-{index}";
                var connection = CreateHubConnection(clientName);
                try
                {
                    await connection.StartAsync();
                    _testHubConnectionList.TryAdd(index, connection);
                    ShowMessage($"{clientName}，连接成功：{connection.ConnectionId}！");
                }
                catch (Exception ex)
                {
                    ShowMessage($"{clientName}，连接失败：{connection.ConnectionId} {ex.Message}！");
                }
            });
            ShowMessage($"模拟客户端 {ClientCount} 个，已创建完成，已连接服务器，启动消息推送...");
            #endregion

            #region 推送消息
            SendMessageCancelTokenSource = new CancellationTokenSource();
            var randomString = GenerateRandomString(MessageLenth);
            var slim = new SemaphoreSlim(1);
            await Parallel.ForEachAsync(_testHubConnectionList, SendMessageCancelTokenSource.Token, async (connection, token) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    var stopwatch = new Stopwatch();
                    for (var i = 1; i <= MessageCount; i++)
                    {
                        await SendAsync(connection.Key, connection.Value, randomString, stopwatch, slim, i);

                        if (SendMessageCancelTokenSource.IsCancellationRequested)
                        {
                            await connection.Value.StopAsync();
                            token.ThrowIfCancellationRequested();
                        }
                        await Task.Delay(SendInterval);
                    }

                    // 发送完成，关闭连接
                    await connection.Value.StopAsync();

                }, SendMessageCancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            });
            #endregion
        }

        private async Task SendAsync(int clientIndex, HubConnection connection, string message, Stopwatch stopwatch, SemaphoreSlim slim, int? times = null)
        {
            var timeString = times.HasValue ? $"第{times}次" : string.Empty;
            stopwatch.Restart();
            if (IsWaitServerResponse)
            {
                var response = await connection.InvokeAsync<string>("InvokeMessage", message);
                ShowMessage($"Client-{clientIndex}{timeString}发送:{message},返回:{response}，耗时ms：{stopwatch.ElapsedTicks / 10000}");
            }
            else
            {
                await connection.SendAsync("SendMessage", message);
                ShowMessage($"Client-{clientIndex}{timeString}发送:{message},耗时ms:{stopwatch.ElapsedTicks / 10000}");
            }

            stopwatch.Stop();

            await slim.WaitAsync();
            SendMessageCount += 1;
            slim.Release();
        }

        private async Task StopTestAsync()
        {
            SendMessageCancelTokenSource.Cancel();
            BtnStartLongRunningEnable = true;
            BtnStartRationRunningEnable = true;
            BtnStopTestEnable = false;
        }
        #endregion

        #region CreateHubConnection
        private HubConnection CreateHubConnection(string clientName)
        {
            var connect = new HubConnectionBuilder()
                           .WithUrl($"{HubServerUrl}/sync?client={clientName}")
                           // 开启自动重连，默认重连4次
                           .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) })
                           // 如果客户端未在15s内向服务端发送消息，在15s的时候会自动ping服务端，是连接保持打开的状态。
                           .WithKeepAliveInterval(TimeSpan.FromSeconds(15))
                           // 如果客户端在30s内收到服务器发送任何消息，那么则会认为已经断开连接，则进入onclose事件。开启了自动重连将进入重连
                           .WithServerTimeout(TimeSpan.FromSeconds(30))
                           .AddMessagePackProtocol()
                           .Build();
            connect.Reconnecting += ex =>
            {
                ShowMessage($"{clientName},重新连接中...");
                return Task.CompletedTask;
            };
            connect.Reconnected += connectionId =>
            {
                ShowMessage($"{clientName},重新连接成功!");
                return Task.CompletedTask;
            };
            connect.Closed += ex =>
            {
                ShowMessage($"{clientName},已断开连接!");
                BtnConnectText = "连接";
                return Task.CompletedTask;
            };
            connect.On<string>("ReceiveMessage", (message) =>
            {
                ReceivedMessageCount += 1;
                ShowMessage($"{clientName}收到:{message}");
            });
            return connect;
        }

        #endregion

        private object _lockLog = new();
        private void ShowMessage(string message)
        {
            lock (_lockLog)
            {
                if (ShowMessageContent?.Length > 1024 * 1024 * 10) // 10MB 清空
                {
                    ShowMessageContent = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{message}";
                }
                else
                {
                    ShowMessageContent = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{message}{Environment.NewLine}{ShowMessageContent}";
                }
            }

        }

        //方法 需要传入长度
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // !@#$%^&*()-_=+<>?

        private string GenerateRandomString(int length)
        {
            var random = new Random();
            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }
    }
}
