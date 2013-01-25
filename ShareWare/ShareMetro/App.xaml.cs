using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
                e.Handled = true;
            }
            catch(IOException)
            {
                MessageBox.Show(e.Exception.Message);
                //Console.WriteLine(e.Exception.Message);
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

        public void Activate()
        {
            // Reactivate application's main window
            MainWindow win = this.MainWindow as MainWindow;
            if (win != null)
            {
                win.ShowMainWindow();
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }


    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App app;

        public SingleInstanceManager()
        {
#if !DEBUG
            this.IsSingleInstance = true;
#endif
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            base.OnStartup(e);
            // First time app is launched
            SplashScreen splashScreen = new SplashScreen("/images/splashscreen1.png");
            splashScreen.Show(true);
            app = new App();
            app.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            app.Activate();
        }
    }


}
