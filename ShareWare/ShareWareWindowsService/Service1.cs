using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareWareWindowsService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        private ServiceHost _shareHost;
        private ServiceHost _regHost;

        protected override void OnStart(string[] args)
        {
             _shareHost = new ServiceHost(typeof(ShareWare.ServiceLibrary.ShareService));
             _regHost = new ServiceHost(typeof(ShareWare.ServiceLibrary.RegisterService));
             _shareHost.Open();
             _regHost.Open();
        }

        protected override void OnStop()
        {
            _shareHost.Close();
            _regHost.Close();
        }
    }
}
