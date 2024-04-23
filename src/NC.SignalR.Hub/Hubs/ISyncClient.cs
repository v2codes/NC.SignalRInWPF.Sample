using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.SignalR.Hub.Hubs
{
    public interface ISyncClient
    {
        /// <summary>
        /// 接收客户端发送的消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task SendMessage(string message);

        /// <summary>
        /// 接收客户端发送的消息并返回数据
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task<string> InvokeMessage(string message);

        /// <summary>
        /// 接收客户端发送的图片
        /// </summary>
        /// <param name="fileName">图片文件名</param>
        /// <param name="data">图片文教</param>
        /// <returns></returns>
        Task SendImage(string fileName, byte[] data);

        /// <summary>
        /// 接受返回数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<string> GetData(string input);
    }
}
