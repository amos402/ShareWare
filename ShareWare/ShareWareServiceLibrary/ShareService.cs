using ShareWare.DAL;
using ShareWare.ShareFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Policy;
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

        private readonly string EmptyIp = "0.0.0.0";

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
            foreach (var item in _userDict)
            {
                if (item.Key != user)
                {
                    item.Value.NewUser(new OnlineUserInfo()
                    {
                        UserName = user.UserName,
                        ImageHash = user.ImageHash
                    });
                }
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
                return EmptyIp;
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

                return -1;
            }

            return id;
        }

        private string GetClientName()
        {
            var client = OperationContext.Current.GetCallbackChannel<IClient>();
            try
            {
                var keyPair = _userDict.Single(item => (item.Value == client));
                string name = keyPair.Key.UserName;
                return name;
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }

        private Users GetClientUser()
        {
            var client = OperationContext.Current.GetCallbackChannel<IClient>();
            try
            {
                var keyPair = _userDict.Single(item => (item.Value == client));

                return keyPair.Key;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public void BroadcastEvent(ServerEventArgs e, ServerEventHandler handler)
        {

        }

        public bool Register(UserInfo userInfo)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                using (ShareWareEntities context = new ShareWareEntities())
                {
                    try
                    {
                        context.Users.Add(new Users()
                        {
                            UserName = userInfo.UserName,
                            Password = userInfo.Password,
                            NickName = userInfo.NickName,
                            IsMale = userInfo.IsMale,
                            QQ = userInfo.QQ,
                            MicroBlog = userInfo.MicroBlog,
                            Signature = userInfo.Signature
                        });
                        context.SaveChanges();

                        transaction.Complete();
                    }
                    catch (Exception)
                    {

                        return false;
                    }
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
                _userDict.Add(user, client);
            }
            catch (Exception)
            {
                if (_userDict.ContainsKey(user))
                {
                    Channel_Closing(_userDict[user], new ServerEventArgs() { Message = "你已经在其他位置登陆", User = user });
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

            var userList = (from c in _userDict.Keys
                            select new OnlineUserInfo()
                            {
                                UserName = c.UserName,
                                ImageHash = c.ImageHash,
                                NickName = c.NickName
                            }).ToList();


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

        public bool UploadShareInfo(List<ShareFile.FileInfoTransfer> fileList)
        {
            int userId = GetClientId();

            if (userId < 0)
            {
                return false;
            }
            var clearList = fileList.Distinct(new HashCompare());
            #region MyRegion

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
            #endregion


            DataTable fileInfo = new DataTable();

            fileInfo.TableName = "FileInfoTemp";
            fileInfo.Columns.AddRange(new DataColumn[]{
                new DataColumn("Hash", typeof(string)) {Unique = true, AllowDBNull = false},
                new DataColumn("Size", typeof(long)) {AllowDBNull = true},
                new DataColumn("IsFolder", typeof(bool)) { AllowDBNull = false},
                new DataColumn("Pass", typeof(bool))
            });


            DataTable fileOwner = new DataTable();
            var dataId = from c in _context.FileOwner select c.ID;

            int? nLast = null;
            try
            {
                nLast = dataId.Max();
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

            using (TransactionScope ts = new TransactionScope())
            {
                try
                {
                    if (BulkInser(fileInfo))
                    {
                        int effect = _context.InsertToFileInfo();
                        BulkInser(fileOwner);
                    }

                    ts.Complete();
                }
                catch (Exception)
                {

                    // throw;
                }

            }
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
            return true;
        }

        private bool BulkInser(DataTable table)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)_context.Database.Connection))
            {
                _context.Database.Connection.Open();
                try
                {
                    bulkCopy.DestinationTableName = table.TableName;
                    bulkCopy.BatchSize = table.Rows.Count;
                    bulkCopy.WriteToServer(table);
                    bulkCopy.Close();
                    return true;
                }
                catch (Exception)
                {
                    DataTable dt = table.Clone();
                    int nHalf = table.Rows.Count / 2;

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

            }
            return false;
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

        public List<FileInfoData> SearchFile(List<string> nameList)
        {
            int id = GetClientId();

            if (id < 0)
            {
                return null;
            }

            List<FileInfoData> newList = new List<FileInfoData>();
            List<IQueryable<FileInfoData>> resultList = new List<IQueryable<FileInfoData>>();
            foreach (var item in nameList)
            {
                var result = (from c in _context.FileOwner
                              where (c.Name.Contains(item))
                              select new FileInfoData()
                              {
                                  UserId = c.UserID,
                                  Name = c.Name,
                                  Hash = c.Hash,
                                  Size = c.FileInfo.Size,
                                  UserName = c.Users.UserName
                              });
                resultList.Add(result);
            }

            var inter = from c in resultList[0]
                        select c;

            for (int i = 1; i < resultList.Count; i++)
            {
                inter = inter.Intersect(resultList[i]);
            }


            return inter.Distinct().ToList();
        }

        public int DownloadRequest(string hash, int nPort)
        {
            //var users = from c in _context.Users
            //            where (fileOnwer.UserID == c.UserID)
            //            select c;
            var users = (from c in _context.FileOwner
                         where (c.Hash == hash)
                         select c.Users).Distinct();

            string ip = GetClientIp();
            foreach (var item in users)
            {
                if (_userDict.ContainsKey(item))
                {
                    _userDict[item].DownloadPerformance(hash, ip, nPort);
                }
            }

            return users.Count();
        }


        public void TickTack()
        {
            int t = 1;

        }

        internal static string ComputeStringMd5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder hash = new StringBuilder();
            foreach (var item in data)
            {
                hash.Append(item.ToString("x2"));
            }
            return hash.ToString();
        }

        internal static string ComputeFileMd5(string path)
        {
            try
            {
                MD5 md5 = MD5.Create();
                FileStream fs = File.OpenRead(path);
                byte[] data = md5.ComputeHash(fs);
                fs.Close();
                StringBuilder hash = new StringBuilder();
                foreach (var item in data)
                {
                    hash.Append(item.ToString("x2"));
                }
                return hash.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public void UploadImage(Bitmap image)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return;
            }

            using (ShareWareEntities context = new ShareWareEntities())
            {
                var user = context.Users.Single(T => T.UserID == id);
                string nameHash = ComputeStringMd5(user.UserName);
                IDictionary section = (IDictionary)ConfigurationManager.GetSection("ImagePath");
                string imagePath = section["Path"].ToString();
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                string filePath = imagePath + nameHash.ToString() + ".jpg";
                image.Save(filePath, ImageFormat.Jpeg);
                string fileHash = ComputeFileMd5(filePath);

                user.ImageHash = fileHash;
                context.SaveChanges();
            }
        }


        public Bitmap DownloadUserImage(string name)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return null;
            }

            IDictionary section = (IDictionary)ConfigurationManager.GetSection("ImagePath");
            string imagePath = section["Path"].ToString();
            string hash = ComputeStringMd5(name);
            string path = imagePath + hash + "jpg";
            if (File.Exists(path))
            {
                try
                {
                    Bitmap image = new Bitmap(path);
                    return image;
                }
                catch (Exception)
                {

                    return null;
                }

            }
            return null;
        }


        public void RequestConversation(string userName, int localPort)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return;
            }

            try
            {
                Users user = _context.Users.Single(T => T.UserName == userName);
                if (_userDict.ContainsKey(user))
                {
                    string ip = GetClientIp();
                    if (ip != EmptyIp)
                    {
                        _userDict[user].ConversationPerformance(ip, localPort);
                    }

                }
            }
            catch (Exception)
            {

                //throw;
            }
        }


        public void RequestOpenShareFolder(string userName, int localPort)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return;
            }

            try
            {
                Users user = _context.Users.Single(T => T.UserName == userName);
                if (_userDict.ContainsKey(user))
                {
                    string ip = GetClientIp();
                    if (ip != EmptyIp)
                    {
                        _userDict[user].OpenShareFolderPerfromance(ip, localPort);
                    }

                }
            }
            catch (Exception)
            {

                //throw;
            }
        }

        public void SendChatRoomMessage(string msg)
        {
            Users user = GetClientUser();

            foreach (var item in _userDict.Values)
            {
                item.ReceiveChatRoomMessage(msg, user.UserName, user.NickName);
            }
        }

        string _chatMsg;
        public string GetChatRoomMessage()
        {

            return _chatMsg;
        }

        public UserInfo DownloadUserInfo(int userId)
        {
            int id = GetClientId();
            if (id != userId)
            {
                return null;
            }

            try
            {
                Users user = _context.Users.Single(T => userId == T.UserID);

                UserInfo userInfo = new UserInfo()
                             {
                                 NickName = user.NickName,
                                 IsMale = user.IsMale,
                                 QQ = user.QQ,
                                 MicroBlog = user.MicroBlog,
                                 Signature = user.Signature,
                                 Password = user.Password
                             };

                return userInfo;
            }
            catch (Exception)
            {

                return null;
            }
        }



        public bool ChangedUserInfo(UserInfo userInfo)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return false;
            }

            using (ShareWareEntities context = new ShareWareEntities())
            {
                Users user = context.Users.Single(T => T.UserID == id);
                user.NickName = userInfo.NickName;
                user.IsMale = userInfo.IsMale;
                user.QQ = userInfo.QQ;
                user.MicroBlog = userInfo.MicroBlog;
                user.Signature = userInfo.Signature;
                user.Password = userInfo.Password;
                context.SaveChanges();
            }

            return true;
        }
    }
}
