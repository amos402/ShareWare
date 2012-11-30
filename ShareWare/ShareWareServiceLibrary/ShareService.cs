using ShareWare.DAL;
using ShareWare.ShareFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Transactions;

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

        private int GetClientId()
        {
            var client = OperationContext.Current.GetCallbackChannel<IClient>();

            int id = -1;
            try
            {
                var keyPair = _userDict.Single(item => (item.Value == client));
                id = keyPair.Key.UserID;
            }
            catch (Exception)
            {

                throw;
            }

            return id;
        }

        public void BroadcastEvent(ServerEventArgs e, ServerEventHandler handler)
        {

        }

        public bool Register(string userName, string passWord, string mail)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                _context.Users.Add(new Users() { UserName = userName, Password = passWord, Mail = mail });

                try
                {
                    _context.SaveChanges();
                    transaction.Complete();
                }
                catch (Exception)
                {
                    return false;
                }
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

        protected class CompareFileInfo : IEqualityComparer<ShareFile.FileInfoTransfer>
        {
            public bool Equals(ShareFile.FileInfoTransfer x, ShareFile.FileInfoTransfer y)
            {
                return x.Hash == y.Hash && x.Name == y.Name;
            }

            public int GetHashCode(ShareFile.FileInfoTransfer obj)
            {
                return 0;
            }
        }

        protected class HashCompare : IEqualityComparer<ShareFile.FileInfoTransfer>
        {
            public bool Equals(ShareFile.FileInfoTransfer x, ShareFile.FileInfoTransfer y)
            {
                return x.Hash == y.Hash;
            }

            public int GetHashCode(ShareFile.FileInfoTransfer obj)
            {
                return 0;
            }
        }

        public void UploadShareInfo(List<ShareFile.FileInfoTransfer> fileList)
        {
            int userId = GetClientId();

            if (userId < 0)
            {
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var clearList = fileList.Distinct(new HashCompare());

            //var hashList = (from b in clearList
            //                select b.Hash)
            //                .Except
            //                (from c in _context.FileInfo
            //                 select c.Hash);


            //var infoList = (from c in clearList
            //                where (hashList.Contains(c.Hash))
            //                select c);



            //var ownerFile = from c in _context.FileOwner
            //                where (c.UserID == userId)
            //                select c;

            //var ownerExceptList = (from b in fileList.AsEnumerable()
            //                       select new { b.Hash, b.Name })
            //                .Except
            //                (from c in ownerFile
            //                 select new { c.Hash, c.Name });


            //var ownerList = from c in fileList.AsEnumerable()
            //                where ownerExceptList.Contains(new { c.Hash, c.Name })
            //                select c;


            DataTable fileInfo = new DataTable();

            fileInfo.TableName = "FileInfoTemp";
            fileInfo.Columns.AddRange(new DataColumn[]{
                new DataColumn("Hash", typeof(string)) {Unique = true, AllowDBNull = false},
                new DataColumn("Size", typeof(long)) {AllowDBNull = true},
                new DataColumn("IsFolder", typeof(bool)) { AllowDBNull = false},
                new DataColumn("Pass", typeof(bool))
            });


            DataTable fileOwner = new DataTable();
            var shit = from c in _context.FileOwner select c.ID;

            int? nLast = null;
            try
            {
                nLast = shit.Max();
            }
            catch (Exception)
            {
                nLast = nLast ?? 0;
            }


            fileOwner.TableName = "FileOwner";
            fileOwner.Columns.AddRange(new DataColumn[]{
                new DataColumn("ID", typeof(int)) { AutoIncrement = true, AutoIncrementSeed = (long)(nLast +1) },
                new DataColumn("UserID", typeof(int)),
                new DataColumn("Name", typeof(string)),
                new DataColumn("Hash", typeof(string)) {AllowDBNull = true}
            });


            foreach (var item in clearList)
            {
                if (item.Hash != null)
                {
                    try
                    {
                        DataRow r1 = fileInfo.NewRow();
                        r1[0] = item.Hash;
                        if (item.Size == null)
                        {
                            r1[1] = DBNull.Value;
                        }
                        else
                        {
                            r1[1] = item.Size; ;
                        }
                        r1[2] = item.IsFolder;
                        fileInfo.Rows.Add(r1);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                }
            }

            foreach (var item in fileList)
            {
                try
                {
                    DataRow r2 = fileOwner.NewRow();
                    r2[1] = userId;
                    r2[2] = item.Name;
                    r2[3] = item.Hash;
                    fileOwner.Rows.Add(r2);
                }
                catch (Exception)
                {

                    // throw;
                }
            }


            BulkInser(fileInfo);
            try
            {
                int effect = _context.InsertToFileInfo();
            }
            catch (Exception)
            {

                //throw;
            }
            BulkInser(fileOwner);

            sw.Stop();
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
            return;
        }

        private void BulkInser(DataTable table)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)_context.Database.Connection))
            {
                _context.Database.Connection.Open();
                try
                {
                    bulkCopy.DestinationTableName = table.TableName;
                    bulkCopy.BatchSize = table.Rows.Count;
                    bulkCopy.WriteToServer(table);

                }
                catch (Exception e)
                {
                    DataTable dt = table.Clone();
                    int nHalf = table.Rows.Count / 2;

                    if (nHalf == 0)
                    {
                        return;
                    }

                    for (int i = 0; i < nHalf; i++)
                    {
                        dt.ImportRow(table.Rows[i]);
                    }
                    BulkInser(dt);

                    dt.Clear();
                    for (int i = nHalf; i < table.Rows.Count; i++)
                    {
                        dt.ImportRow(table.Rows[i]);
                    }

                    BulkInser(dt);

                }
                bulkCopy.Close();
            }
        }

        public void RemoveOldFile(List<FileInfoTransfer> fileList)
        {
           
            int id = GetClientId();
            foreach (var item in fileList)
            {
                var res = from c in _context.FileOwner
                        where c.UserID == id && c.Hash == item.Hash
                        select c;
                foreach (var file in res)
                {
                    _context.FileOwner.Remove(file);
                }
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception)
                {

                    // throw;
                }

            }

        }

        public List<FileInfoTransfer> DownloadShareInfo()
        {
            List<FileInfoTransfer> list = new List<FileInfoTransfer>();
            int id = GetClientId();
            var result = from c in _context.FileOwner
                         where (c.UserID == id)
                         select c;
            int nCount = 0;
            foreach (var item in result)
            {

                if (item.FileInfo == null)
                {
                    nCount++;
                }

                FileInfoTransfer file = new FileInfoTransfer();
                try
                {
                    file.Hash = item.Hash;
                    file.Name = item.Name;
                    file.Size = item.FileInfo.Size;
                    file.IsFolder = item.FileInfo.IsFolder;
                    list.Add(file);
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            return list;
        }

        public List<FileOwner> SearchFile(string fileName)
        {
            var result = from c in _context.FileOwner
                         where (c.Name.Contains(fileName))
                         select new { c.ID, c.UserID, c.Name, c.Hash, c.FileInfo };
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
                    //FileInfo = item.FileInfo
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


