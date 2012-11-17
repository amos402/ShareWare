﻿using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
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
       [OperationContract(IsOneWay = false)]
        int Login(string userName, string passWord);

        [OperationContract]
        bool SendShareFile(List<List<FileInfo>> list);

        [OperationContract]
        bool SendClientInfo();

        [OperationContract]
        int UploadShareInfo(List<FileInfoTransfer> fileList, int userId);

        [OperationContract(IsOneWay = false)]
        void SearchFile(string fileName);

    }


    public interface IClient
    {
        [OperationContract(IsOneWay = true)]
        void GetFilePath();
    }
    // 使用下面示例中说明的数据约定将复合类型添加到服务操作。
    // 可以将 XSD 文件添加到项目中。在生成项目后，可以通过命名空间“ShareWareServiceLibrary.ContractType”直接使用其中定义的数据类型。
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
