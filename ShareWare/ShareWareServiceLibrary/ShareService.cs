using ShareWare.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
    //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ShareService : IShareService
    {
        // UserDAL user = new UserDAL();
        //string m_cnStr = ConfigurationManager.ConnectionStrings["ShareWareSqlProvider"].ConnectionString;
        static ShareWareEntities _context = new ShareWareEntities();
        static List<Users> _userList = new List<Users>();
        static Dictionary<Users, IClient> _userDict = new Dictionary<Users, IClient>();

        public delegate void ServerEventHandler(object sender, ServerEventArgs e);
        public event ServerEventHandler UserLogin;
        public event ServerEventHandler UserLeave;

        private static List<IClient> _clientCallbackList = new List<IClient>();

        public static List<IClient> ClientCallbackList
        {
            get { return ShareService._clientCallbackList; }
            set { ShareService._clientCallbackList = value; }
        }


        public ShareService()
        {
            OperationContext.Current.Channel.Closing += Channel_Closing;
            UserLogin += ShareService_UserLogin;
            UserLeave += ShareService_UserLeave;

        }

        void BroadcastEvent(object sender, ServerEventArgs e, ServerEventHandler handler)
        {
            foreach (ServerEventHandler item in handler.GetInvocationList())
            {
                item.BeginInvoke(sender, e, null, null);
            }
        }

        void ShareService_UserLogin(object sender, ServerEventArgs e)
        {
            Users user = e.User;
            foreach (var item in _userDict.Values)
            {
                item.NewUser(user.UserID, user.UserName);
            }
        }

        void ShareService_UserLeave(object sender, ServerEventArgs e)
        {
            if (e.User.UserName != null)
            {
                foreach (var item in _userDict.Values)
                {
                    item.UserLeave(e.User.UserName);
                }
            }
        }


        private void Channel_Closing(object sender, EventArgs e)
        {
            var client = sender as IClient;
            if (client != null)
            {
                try
                {
                    var keyPair = _userDict.Single(item => (item.Value == client));
                    ServerEventArgs sea = new ServerEventArgs();
                    sea.User = keyPair.Key;
                    BroadcastEvent(sender, sea, UserLeave);
                    _userDict.Remove(keyPair.Key);

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex);
                }
            }
        }



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

        private string GetClientIp()
        {
            //提供方法执行的上下文环境
            OperationContext context = OperationContext.Current;
            //获取传进的消息属性
            MessageProperties properties = context.IncomingMessageProperties;
            //获取消息发送的远程终结点IP和端口
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (endpoint != null)
            {
                return endpoint.Address;
            }
            else
            {
                return "0.0.0.0";
            }
        }

        public void BroadcastEvent(ServerEventArgs e, ServerEventHandler handler)
        {

        }

        public bool Register(string userName, string passWord, string mail)
        {
            _context.Users.Add(new Users() { UserName = userName, Password = passWord, Mail = mail });

            try
            {
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public int Login(string userName, string passWord, string mac)
        {
            Users user = null;
            try
            {
                user = _context.Users.Single(us => us.UserName == userName && us.Password == passWord);
            }
            catch (InvalidOperationException)
            {

                return -1;
            }

            user.UserIP = GetClientIp();
            user.MAC = mac;
            var client = OperationContext.Current.GetCallbackChannel<IClient>();
            ClientCallbackList.Add(client);

            try
            {
                lock (_userDict)
                {
                    _userDict.Add(user, client);
                }

            }
            catch (Exception)
            {
                if (_userDict.ContainsKey(user))
                {
                    Channel_Closing(_userDict[user], new ServerEventArgs() { Message = "You have lognin at another location.", User = user });
                    _userDict[user] = client;
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch (Exception)
            {

                //throw;
            }


            BroadcastEvent(this, new ServerEventArgs() { User = user }, UserLogin);

            List<string> userList = (from c in _userDict.Keys select c.UserName).ToList();
            client.RefreshUserList(userList);

            return user.UserID;

        }

        public void Logout()
        {
            IClient client = OperationContext.Current.GetCallbackChannel<IClient>();
            Channel_Closing(client, null);
        }

        protected class CompareFileInfo : IEqualityComparer<ShareWare.FileInfo>
        {
            public bool Equals(ShareWare.FileInfo x, ShareWare.FileInfo y)
            {
                return x.Hash == y.Hash && x.Size == y.Size;
            }

            public int GetHashCode(FileInfo obj)
            {
                return 0;
            }
        }


        public int UploadShareInfo(List<ShareFile.FileInfoTransfer> fileList, int userId)
        {

            var asd = (from b in fileList.AsEnumerable() select b.Hash);
            var res = (from c in _context.FileInfo select c.Hash);

            var shit = new ArrayList();
            foreach (var item in res)
            {
                if (!asd.Contains(item))
                {
                    shit.Add(item);
                }
            }


            DataTable fileInfo = new DataTable();
            fileInfo.Columns.AddRange(new DataColumn[]{
                new DataColumn("Hash", typeof(string)) {Unique = true},  
                new DataColumn("Name", typeof(string)),  
                new DataColumn("Size", typeof(long))}
                );

            
            DataTable fileOwner = new DataTable();
            fileOwner.Columns.AddRange(new DataColumn[]{
                new DataColumn("ID", typeof(int)) { AutoIncrement = true},
                new DataColumn("UserID", typeof(int)),
                new DataColumn("Name", typeof(string)),
                new DataColumn("Hash", typeof(string)) {AllowDBNull = true},
                new DataColumn("Path", typeof(string)) {AllowDBNull = true},
                new DataColumn("IsFolder", typeof(bool))
            });

          
            foreach (var item in fileList)
            {
                string guid = null;
                try
                {
                    DataRow r1 = fileInfo.NewRow();
                    if (item.Hash == null && item.IsFolder == true)
                    {
                        guid = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        guid = item.Hash;
                    }
                    r1[0] = guid;
                    r1[1] = item.Size;
                    fileInfo.Rows.Add(r1);
                }
                catch (Exception)
                {

                    //throw;
                }

               
                try
                {
                    DataRow r2 = fileOwner.NewRow();
                    r2[1] = userId;
                    r2[2] = item.Name;
                    r2[3] = guid;
                    r2[4] = item.Path;
                    r2[5] = item.IsFolder;
                    fileOwner.Rows.Add(r2);
                }
                catch (Exception)
                {

                    // throw;
                }

            }

            var conn = _context.Database.Connection;
            conn.Open();
            SqlBulkCopy bulkCopy = new SqlBulkCopy(_context.Database.Connection.ConnectionString, SqlBulkCopyOptions.KeepIdentity);
            try
            {

                bulkCopy.DestinationTableName = "FileInfo";
                bulkCopy.BatchSize = fileInfo.Rows.Count;

                bulkCopy.WriteToServer(fileInfo);
            }
            catch (Exception e)
            {

                //Console.WriteLine(e.Message);
            }


            try
            {
                bulkCopy.DestinationTableName = "FileOwner";
                bulkCopy.BatchSize = fileOwner.Rows.Count;

                bulkCopy.WriteToServer(fileOwner);

            }
            catch (Exception e)
            {

                //Console.WriteLine(e.Message);
            }
            conn.Close();


            #region MyRegion
            //try
            //{

            //    foreach (var item in fileList)
            //    {
            //        var info = from c in _context.FileInfo.Local where (c.Hash == item.Hash) select c;

            //        if (info.Count() == 0)
            //        {
            //            _context.FileInfo.Local.Add(new ShareWare.FileInfo() { Hash = item.Hash, Name = item.Name, Size = (int)item.Size });

            //        }

            //        var info1 = from c in _context.FileOwner.Local where (c.Hash == item.Hash && c.Name == item.Name) select c;
            //        if (info1.Count() == 0)
            //        {
            //            _context.FileOwner.Local.Add(new FileOwner() { Hash = item.Hash, UserID = userId, Name = item.Name });
            //        }


            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.InnerException.Message);
            //}

            //try
            //{
            //    _context.SaveChanges();
            //}
            //catch (Exception e)
            //{

            //    Console.WriteLine(e.InnerException.Message);
            //}

            #endregion
            return 0;
        }


        public List<FileOwner> SearchFile(string fileName)
        {
            var result = from c in _context.FileOwner
                         where (c.Name.Contains(fileName))
                         select new { c.ID, c.UserID, c.Name, c.Hash, c.Path, c.IsFolder };
            var list = result.ToList();
            var newList = new List<FileOwner>();
            foreach (var item in list)
            {
                newList.Add(new FileOwner()
                {
                    ID = item.ID,
                    UserID = item.UserID,
                    Hash = item.Hash,
                    Name = item.Name,
                    IsFolder = item.IsFolder,
                    Path = item.Path

                });
            }

            return newList;

        }


        public int DownloadRequest(FileOwner fileOnwer, int nPort)
        {
            var users = from c in _context.Users
                        where (fileOnwer.UserID == c.UserID)
                        select c;
            string ip = GetClientIp();
            foreach (var item in users)
            {
                _userDict[item].DownloadPerformance(fileOnwer.Hash, ip, nPort);
            }

            return users.Count();
        }
    }
}


