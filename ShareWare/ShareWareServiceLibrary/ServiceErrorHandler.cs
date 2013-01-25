using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ServiceLibrary
{
    class ServiceErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            HandleException(error);
        }


        public static void HandleException(Exception ex)
        {
            //记录日志
           
            string path = ConfigurationManager.AppSettings["LogPath"];

            if (path == null)
            {
                path = Environment.CurrentDirectory;
                if (!System.IO.Directory.Exists("Log"))
                {
                    System.IO.Directory.CreateDirectory("Log");
                }
            }
            else
            {
                if (!System.IO.Directory.Exists(path))
                {
                     System.IO.Directory.CreateDirectory(path);
                }
            }

            try
            {
                DateTime now = DateTime.Now;
                string logpath = string.Format(@"{3}\fatal_{0}{1}{2}.log", now.Year, now.Month, now.Day, path);
                System.IO.File.AppendAllText(logpath, string.Format("\r\n----------------------{0}--------------------------\r\n", now.ToString("yyyy-MM-dd HH:mm:ss")));
                System.IO.File.AppendAllText(logpath, ex.Message);
                System.IO.File.AppendAllText(logpath, "\r\n");
                System.IO.File.AppendAllText(logpath, ex.StackTrace);
                System.IO.File.AppendAllText(logpath, "\r\n");
                System.IO.File.AppendAllText(logpath, "\r\n----------------------footer--------------------------\r\n");
            }
            catch (IOException)
            {

                Console.WriteLine(ex);
            }

        }
    }



}
