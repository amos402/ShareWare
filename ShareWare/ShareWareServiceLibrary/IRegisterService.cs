using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ServiceLibrary
{
    [ServiceContract]
    public interface IRegisterService
    {
        [OperationContract]
        bool Register(UserInfo userInfo);
    }
}
