﻿using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ShareWare.ServiceLibrary
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract(CallbackContract = typeof(IClient))]
    public interface IShareService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: 在此添加您的服务操作
        [OperationContract(IsOneWay = true)]
        void TickTack();

        [OperationContract(IsOneWay = false)]
        bool Register(UserInfo userInfo);

        [OperationContract(IsOneWay = false)]
        int Login(string userName, string passWord, string mac);

        [OperationContract(IsOneWay = true)]
        void Logout();

        [OperationContract(IsOneWay = false)]
        bool UploadShareInfo(List<FileInfoTransfer> fileList);

        [OperationContract]
        List<FileInfoTransfer> DownloadShareInfo();

        [OperationContract(IsOneWay = true)]
        void RemoveOldFile(List<ShareFile.FileInfoTransfer> fileList);
        [OperationContract]
        List<FileInfoData> SearchFile(List<string> nameList);

        [OperationContract]
        int DownloadRequest(string hash, int nPort);

        [OperationContract(IsOneWay = true)]
        void UploadImage(Bitmap image);

        [OperationContract]
        Bitmap DownloadUserImage(string name);

        [OperationContract(IsOneWay = true)]
        void RequestConversation(string userName, int localPort);

        [OperationContract]
        void RequestOpenShareFolder(string userName, int localPort);

        [OperationContract(IsOneWay = true)]
        void SendChatRoomMessage(string msg);

        [OperationContract]
        UserInfo DownloadUserInfo(int userId);

        [OperationContract]
        bool ChangedUserInfo(UserInfo userInfo);
    }


    // 使用下面示例中说明的数据约定将复合类型添加到服务操作。
    // 可以将 XSD 文件添加到项目中。在生成项目后，可以通过命名空间“ShareWareServiceLibrary.ContractType”直接使用其中定义的数据类型。
    public class ServerEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Users User { get; set; }
    }


    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
