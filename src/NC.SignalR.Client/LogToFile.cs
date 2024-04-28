using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.SignalR.Client
{
    public static class LogToFile
    {
        public static void Write(string message)
        {
            var dtNow = DateTime.Now.Date;
            var logs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client-log");
            if (!Directory.Exists(logs))
            {
                Directory.CreateDirectory(logs);
            }
            var file = Path.Combine(logs, $"{dtNow.ToString("yyyy-MM-dd")}.txt");
            if (!File.Exists(file))
            {
                using (File.Create(file)) { }
            }
            using (StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine(message);
            }
        }
    }
}
