using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare
{
    public interface IClient
    {
        [OperationContract(IsOneWay = true)]
        void DownloadPerformance(string szHash, string szIp, int nPort);
    }

}
