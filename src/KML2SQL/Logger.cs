using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML2SQL
{
    class Logger
    {
        private StringBuilder sb = new StringBuilder();

        public void AddToLog(string message)
        {
            sb.Append(message + Environment.NewLine);
        }

        public void AddToLog(Exception ex)
        {
            sb.Append(ex.ToString() + Environment.NewLine);
        }

        private string GetLogDirectory()
        {
            var errorLogPath = Path.Combine(Utility.GetApplicationFolder(), "logs");
            if (!Directory.Exists(errorLogPath))
            {
                Directory.CreateDirectory(errorLogPath);
            }
            return errorLogPath;
        }

        public void WriteOut()
        {
            if (sb.Length > 0)
            {
                var logFileName = "log_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-ffff") + ".txt";
                var fullPath = Path.Combine(GetLogDirectory(), logFileName);
                File.WriteAllText(fullPath, sb.ToString());
            }
            sb = new StringBuilder();
        }
    }
}
