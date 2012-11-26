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

        [OperationContract(IsOneWay = true)]
        void NewUser(int id, string name);

        [OperationContract(IsOneWay = true)]
        void UserLeave(string name);

        [OperationContract(IsOneWay = true)]
        void RefreshUserList(List<string> userList);
    }

}
