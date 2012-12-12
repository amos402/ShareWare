using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ServiceLibrary
{
    [ServiceContract]
    public interface IRegisterService
    {
        [OperationContract]
        RegError Register(UserInfo userInfo);
    }

    public enum RegError
    {
        NoError,
        UserExist,
        Ohter
    }

}
