using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.SignalR.Hub.Hubs
{
    public class SyncClientInfo
    {
        /// <summary>
        /// 链接ID
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; set; }
    }
}
