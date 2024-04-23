using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NC.SignalR.Hub.Hubs
{
    /// <summary>
    /// Hub 上下文扩展
    /// @Created by Leo 2020/8/10 15:18:45
    /// </summary>
    public static class HubContextExtensions
    {
        /// <summary>
        /// 获取IP
        /// </summary>
        /// <param name="hubContext"></param>
        /// <returns></returns>
        public static string GetRemoteIpAddress(this HubCallerContext hubContext)
        {
            var ip = hubContext?.GetHttpContext()?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            return ip;
        }

        /// <summary>
        /// 获取URL参数
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetQueryString(this HubCallerContext hubContext, string name)
        {
            var param = hubContext?.GetHttpContext()?.Request.Query[name].ToString();
            return param;
        }

        /// <summary>
        /// 获取URL参数 "client"
        /// </summary>
        /// <param name="hubContext"></param>
        /// <returns></returns>
        public static string GetCurrentClient(this HubCallerContext hubContext)
        {
            var param = hubContext?.GetQueryString("client");
            return param;
        }

        /// <summary>
        /// 缓存当前用户角色信息
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="identity"></param>
        /// <param name="teacherCode"></param>
        public static void SaveIdentityInfo(this HubCallerContext hubContext, string identity, string teacherCode)
        {
            // TeacherCode 存入HubContext
            var identityInfo = new IdentityInfo(identity, teacherCode);
            if (!hubContext.Items.TryAdd("IdentityInfo", identityInfo))
            {
                hubContext.Items["IdentityInfo"] = identityInfo;
            }
        }

        /// <summary>
        /// 获取当前用户角色信息
        /// </summary>
        /// <param name="hubContext"></param>
        /// <returns></returns>
        public static IdentityInfo GetIdentityInfo(this HubCallerContext hubContext)
        {
            if (!hubContext.Items.TryGetValue("IdentityInfo", out object identityInfo))
            {
                //throw new AiBizECodeException("HubCallerContext.Items 中 IdentityInfo 不存在！");
            }
            return identityInfo as IdentityInfo;
        }
    }

    public class IdentityInfo
    {
        public string Identity { get; set; }
        public string TeacherCode { get; set; }

        public IdentityInfo(string identity, string teacherCode)
        {
            this.Identity = identity;
            this.TeacherCode = teacherCode;
        }
    }
}
