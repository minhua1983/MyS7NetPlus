using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MyS7NetPlus.Common.Tools
{
    public class MyLogger
    {
        private readonly Logger _logger;
        public event EventHandler<MyLogEventArgs> Logged;

        public MyLogger(string loggerName)
        {
            _logger = loggerName.ToUpper() switch
            {
                "WINFORMLOGGER" => LogManager.GetLogger("WinFormLogger"),
                "WEBAPILOGGER" => LogManager.GetLogger("WebApiLogger"),
                _ => LogManager.GetLogger("*"),
            };
        }

        public void Log(LogLevel logLevel, string messsage, Exception? e = null)
        {
            _logger.Log(logLevel, messsage, e);
            if (logLevel.Ordinal >= 3) {
                OnLogged(new()
                {
                    LogLevel = logLevel,
                    Message = messsage
                });
            }
        }

        protected virtual void OnLogged(MyLogEventArgs myLogEventArgs)
        {
            Logged?.Invoke(this, myLogEventArgs);
        }
    }
}
