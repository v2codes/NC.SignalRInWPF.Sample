using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace NC.SignalR.Client.ViewModels
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
        private int _clientCount;

        [ObservableProperty]
        private int _messageLenth;

        [ObservableProperty]
        private int _sendInterval;

        [ObservableProperty]
        private int _messageCount;

        [ObservableProperty]
        private decimal _sendMessageCount;

        [ObservableProperty]
        private decimal _receivedMessageCount;

        [ObservableProperty]
        private bool _btnStartLongRunningEnable;

        [ObservableProperty]
        private bool _btnStopTestEnable;

        [ObservableProperty]
        private bool _btnStartRationRunningEnable;

        [ObservableProperty]
        private bool _isWaitServerResponse;

        [ObservableProperty]
        private bool _isShowMessage;

        [ObservableProperty]
        private BindingList<string> _showMessageContentList;

        [ObservableProperty]
        private decimal _disconnectCount;

        [ObservableProperty]
        private decimal _sendFailedCount;
        #endregion

        #region 手动测试 Command
        public AsyncRelayCommand ConnectCommand { get; set; }
        public AsyncRelayCommand SendMessageCommand { get; set; }
        public AsyncRelayCommand SendImageCommand { get; set; }
        public AsyncRelayCommand InvokeMessageCommand { get; set; }
        #endregion

        #region 模拟测试 Command
        private ConcurrentDictionary<int, HubConnection> _testHubConnectionList { get; set; }
        public AsyncRelayCommand StartLongRunningCommand { get; set; }
        public AsyncRelayCommand StartRationRunningCommand { get; set; }
        public AsyncRelayCommand StopTestCommand { get; set; }
        public AsyncRelayCommand ResetCommand { get; set; }
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
            IsWaitServerResponse = true;
            DisconnectCount = 0;
            IsShowMessage = true;
            SendFailedCount = 0;
            ShowMessageContentList = new BindingList<string>();

            _testHubConnectionList = new ConcurrentDictionary<int, HubConnection>();
            #endregion

            #region Command
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
            InvokeMessageCommand = new AsyncRelayCommand(InvokeMessageAsync);
            SendImageCommand = new AsyncRelayCommand(SendImageAsync);
            StartLongRunningCommand = new AsyncRelayCommand(StartLongRunningAsync);
            StartRationRunningCommand = new AsyncRelayCommand(StartRationRunningAsync);
            StopTestCommand = new AsyncRelayCommand(StopTestAsync);
            ResetCommand = new AsyncRelayCommand(ResetAsync);
            #endregion
        }

        #region 手动测试
        private async Task ConnectAsync()
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
                await DisconnectAsync();
            }
        }

        private async Task DisconnectAsync()
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
                ShowMessage($"Client-X发送:{TxtInputMessage}", true);
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
                ShowMessage($"Client-X发送文件:{fileName}", true);
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
                ShowMessage($"Client-X发送:{TxtInputMessage},返回:{result}", true);
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

            BtnStartLongRunningEnable = false;
            BtnStartRationRunningEnable = false;
            BtnStopTestEnable = true;

            // 停止手动连接
            await DisconnectAsync();
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
                            await Task.Delay(1000);
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
            BtnStartLongRunningEnable = false;
            BtnStartRationRunningEnable = false;
            BtnStopTestEnable = true;

            // 停止手动连接
            await DisconnectAsync();
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
                            await Task.Delay(1000);
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
                ShowMessage($"Client-{clientIndex}{timeString}发送:{message},返回:{response}，耗时ms：{stopwatch.ElapsedTicks / 10000}", true);
            }
            else
            {
                await connection.SendAsync("SendMessage", message);
                ShowMessage($"Client-{clientIndex}{timeString}发送:{message},耗时ms:{stopwatch.ElapsedTicks / 10000}", true);
            }

            stopwatch.Stop();
        }

        private async Task StopTestAsync()
        {
            SendMessageCancelTokenSource.Cancel();
            BtnStartLongRunningEnable = true;
            BtnStartRationRunningEnable = true;
            BtnStopTestEnable = false;
        }

        #endregion

        private async Task ResetAsync()
        {
            SendMessageCount = 0;
            ReceivedMessageCount = 0;
            DisconnectCount = 0;
            IsShowMessage = true;
            ShowMessageContentList.Clear();
        }

        #region CreateHubConnection
        private object _lock = new();
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
                lock (_lock)
                {
                    DisconnectCount += 1;
                }
                return Task.CompletedTask;
            };
            connect.On<string>("ReceiveMessage", (message) =>
            {
                ReceivedMessageCount += 1;
                ShowMessage($"{clientName}收到:{message}", false, true);
            });
            return connect;
        }

        #endregion

        private object _lockMessage = new();
        private void ShowMessage(string message, bool isSend = false, bool isReceive = false, bool isFailed = false)
        {
            lock (_lockMessage)
            {
                if (isSend)
                    SendMessageCount += 1;
                if (isReceive)
                    ReceivedMessageCount += 1;
                if (isFailed)
                    SendFailedCount += 1;
                LogToFile.Write(message);
            }

            if (!IsShowMessage)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowMessageContentList.Insert(0, $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}]{message}");
            });
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
