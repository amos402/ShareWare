using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost _shareHost;
            ServiceHost _regHost;
            _shareHost = new ServiceHost(typeof(ShareWare.ServiceLibrary.ShareService));
            _regHost = new ServiceHost(typeof(ShareWare.ServiceLibrary.RegisterService));

            _shareHost.Opened += (sender, e) => Console.WriteLine(_shareHost.Description.Name);
            _regHost.Opened += (sender, e) => Console.WriteLine(_regHost.Description.Name);

            _shareHost.Open();
            _regHost.Open();

            Console.ReadLine();
            _shareHost.Close();
            _regHost.Close();
        }

    }
}
