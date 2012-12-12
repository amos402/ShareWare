using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ServiceLibrary
{
    public interface IClient
    {
        [OperationContract(IsOneWay = true)]
        void DownloadPerformance(string szHash, string szIp, int nPort);

        [OperationContract(IsOneWay = true)]
        void NewUser(OnlineUserInfo user);

        [OperationContract(IsOneWay = true)]
        void UserLeave(string name);

        [OperationContract(IsOneWay = true)]
        void RefreshUserList(List<OnlineUserInfo> userList);

        [OperationContract(IsOneWay = true)]
        void ConversationPerformance(string remoteIp, int remotePort);

        [OperationContract(IsOneWay = true)]
        void OpenShareFolderPerfromance(string nickName, string remoteIp, int remotePort);

        [OperationContract(IsOneWay = true)]
        void ReceiveChatRoomMessage(string msg, string userName, string nickName);

        [OperationContract(IsOneWay = true)]
        void NewChatRoomMessage();
    }

}
