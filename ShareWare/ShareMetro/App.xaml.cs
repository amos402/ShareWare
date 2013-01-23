using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ShareMetro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            #if DEBUG
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            #endif
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
                e.Handled = true;
            }
            catch
            {

            }
        }

        public static void HandleException(Exception ex)
        {
            //记录日志
            if (!System.IO.Directory.Exists("Log"))
            {
                System.IO.Directory.CreateDirectory("Log");
            }
            DateTime now = DateTime.Now;
            string logpath = string.Format(@"Log\fatal_{0}{1}{2}.log", now.Year, now.Month, now.Day);
            System.IO.File.AppendAllText(logpath, string.Format("\r\n----------------------{0}--------------------------\r\n", now.ToString("yyyy-MM-dd HH:mm:ss")));
            System.IO.File.AppendAllText(logpath, ex.Message);
            System.IO.File.AppendAllText(logpath, "\r\n");
            System.IO.File.AppendAllText(logpath, ex.StackTrace);
            System.IO.File.AppendAllText(logpath, "\r\n");
            System.IO.File.AppendAllText(logpath, "\r\n----------------------footer--------------------------\r\n");

        }
    }
}
