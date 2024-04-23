using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.SignalR.Hub.Hubs
{
    /// <summary>
    /// 客户端缓存信息
    /// </summary>
    public class ClientStorage
    {
        private ConcurrentBag<SyncClientInfo> ClientList { get; set; }

        public ClientStorage()
        {
            ClientList = new ConcurrentBag<SyncClientInfo>();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="clientName"></param>
        public void TryAdd(string connectionId, string clientName)
        {
            var client = ClientList.FirstOrDefault(p => p.ClientName == clientName);
            if (client == null)
            {
                ClientList.Add(new SyncClientInfo
                {
                    ConnectionId = connectionId,
                    ClientName = clientName
                });
            }
            else
            {
                client.ConnectionId = connectionId;
                client.ClientName = clientName;
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="clientName"></param>
        public SyncClientInfo GetClient(string clientName)
        {
            var client = ClientList.FirstOrDefault(p => p.ClientName == clientName);
            return client;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="clientName"></param>
        public void TryRemove(string clientName)
        {
            var client = ClientList.FirstOrDefault(p => p.ClientName == clientName);
            ClientList.TryTake(out client);
        }

        /// <summary>
        /// 客户端总数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return ClientList.Count;
        }

        /// <summary>
        /// 客户端总数
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            ClientList.Clear();
        }
    }
}
