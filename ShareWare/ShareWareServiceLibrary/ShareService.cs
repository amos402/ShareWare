using ShareWare.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace ShareWare.ServiceLibrary
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“Service1”。
   // [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ShareService : IShareService
    {

        UserDAL user = new UserDAL();
        //string m_cnStr = ConfigurationManager.ConnectionStrings["ShareWareSqlProvider"].ConnectionString;
        static ShareWareEntities _context = new ShareWareEntities();
        static List<Users> _userList = new List<Users>();
        static Dictionary<Users, IClient> _userDict = new Dictionary<Users, IClient>();


        private static List<IClient> _clientCallbackList = new List<IClient>();

        public static List<IClient> ClientCallbackList
        {
            get { return ShareService._clientCallbackList; }
            set { ShareService._clientCallbackList = value; }
        }

        List<string> userList = new List<string>();

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }


        //public int Login(string userName, string passWord)
        //{

        //    bool bHave = user.CheckUser(userName, passWord);
        //    if (bHave)
        //    {
        //        userList.Add(userName);
        //    }
        //    return bHave ? true : false;

        //}


        public bool SendShareFile(List<List<FileInfo>> list)
        {
            FileInfo info = (FileInfo)list[0][0];
            //throw new NotImplementedException();
            return true;
        }



        public bool SendClientInfo()
        {
            //提供方法执行的上下文环境
            OperationContext context = OperationContext.Current;
            //获取传进的消息属性
            MessageProperties properties = context.IncomingMessageProperties;
            //获取消息发送的远程终结点IP和端口
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            Console.WriteLine(string.Format("Hello ,You are  from {0}:{1}", endpoint.Address, endpoint.Port));
            return true;
        }


        public int Login(string userName, string passWord)
        {
            var user = from c in _context.Users
                       where (c.UserName == userName && c.Password == passWord)
                       select c;
            if (user.Count() == 1)
            {
                //提供方法执行的上下文环境
                OperationContext context = OperationContext.Current;
                //获取传进的消息属性
                MessageProperties properties = context.IncomingMessageProperties;
                //获取消息发送的远程终结点IP和端口
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                user.First().UserIP = endpoint.Address;


                var client = OperationContext.Current.GetCallbackChannel<IClient>();
                ClientCallbackList.Add(client);

                _userDict.Add(user.First(), client);

                _userList.Add(user.First());

                return user.First().UserID;
            }

            return -1;

        }

        public int UploadShareInfo(List<ShareFile.FileInfoTransfer> fileList, int userId)
        {
            try
            {

                foreach (var item in fileList)
                {
                    var info = from c in _context.FileInfo.Local where (c.Hash == item.Hash) select c;

                    if (info.Count() == 0)
                    {
                        _context.FileInfo.Add(new ShareWare.FileInfo() { Hash = item.Hash, Name = item.Name, Size = (int)item.Size });

                    }
                    _context.FileOwner.Add(new FileOwner() { Hash = item.Hash, UserID = userId, Name = item.Name });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            return 0;
        }


        public void SearchFile(string fileName)
        {
            var result = from c in _context.FileOwner.Local where (c.Name.Contains(fileName)) select c;
            var list = result.ToList();
            

        }
    }
}


