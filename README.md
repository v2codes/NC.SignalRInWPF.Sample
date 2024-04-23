# NC.SignalRInWPF.Sample
Use SignalR in WPF Application on .Net 8.0.

- /NC.SignalR.Hub：SignalR服务端
- /NC.SignalR.Client：SignalR客户端
- /perf：性能测试工具目录，Crank、Microbenchmarks
  - /benchmarkapps：Crankier 目录
  - /Microsoft.AspNetCore.SignalR.Microbenchmarks：Microbenchmarks 目录
- /pubish：应用发布目录
  
<div  align="center">    
  <img src="https://github.com/v2codes/NC.SignalRInWPF.Sample/assets/32858631/5c85d119-b712-4282-81b5-39bd7d07d339" width = "48%" alt="SignalR 服务端"/>
  <img src="https://github.com/v2codes/NC.SignalRInWPF.Sample/assets/32858631/3f8776b2-0c97-498d-bab8-5e44dd2fccc4" width = "48%" alt="SignalR 服务端"/>
</div>

### 使用 Crank 工具通过多个模拟客户端测试SignalR服务

#### 参考文档

- https://learn.microsoft.com/en-us/aspnet/signalr/overview/performance/signalr-connection-density-testing-with-crank
- https://github.com/dotnet/aspnetcore/tree/main/src/SignalR/perf

#### Crank 命令选项:
> 	sage:  [options] [command]
> 	Options:
> 	  -h|--help  Show help information
> 	Commands:
> 	  agent
> 	  help
> 	  local - 启动一组本地工作客户端以建立与 SignalR 服务器的连接
> 	  server - 启动一组本地工作客户端以建立与 Azure SignalR 服务器的连接
> 	  worker

#### 使用示例

##### 本地SignalR服务

> 	-- 尝试使用10个线程，分别与服务器建立1000个连接，WebSocket协议
> 	.\Crankier.exe local --target-url http://localhost:8080/sync?client=crankier --workers 10
> 		
> 	-- 与服务器建立5000个连接，指定 LongPolling 协议
> 	.\Crankier.exe local --target-url http://localhost:8080/sync?client=crankier --connections 5000 --transport LongPolling

##### Azure SignalR服务

> .\Crankier.exe server --azure-signalr-connectionstring Endpoint=https://your-url.service.signalr.net;AccessKey=yourAccessKey;Version=1.0



