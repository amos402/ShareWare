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
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Policy;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Transactions;

namespace ShareWare.ServiceLibrary
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ShareService : IShareService
    {
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

        public static string ImagePath { get { return ConfigurationManager.AppSettings["ImagePath"].ToString(); } }

        public ShareService()
        {
            OperationContext.Current.Channel.Closing += Channel_Closing;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;
            UserLogin += ShareService_UserLogin;
            UserLeave += ShareService_UserLeave;

        }

        void Channel_Faulted(object sender, EventArgs e)
        {
            ServiceErrorHandler.HandleException(new Exception("Faulted"));
            // string id = GetClientName();
            // throw new NotImplementedException();
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
            try
            {
                Users user = e.User;
                lock (o)
                {
                    foreach (var item in _userDict)
                    {
                        if (item.Key != user)
                        {
                            item.Value.NewUser(new OnlineUserInfo()
                            {
                                UserName = user.UserName,
                                ImageHash = user.ImageHash,
                                NickName = user.NickName
                            });
                            //Thread.Sleep(1000);
                        }
                    }
                }

            }
            catch (InvalidOperationException)
            {
                //throw;
            }
        }


        void ShareService_UserLeave(object sender, ServerEventArgs e)
        {
            lock (o)
            {
                if (e.User.UserName != null)
                {
                    foreach (var item in _userDict)
                    {
                        if (item.Key != e.User)
                        {
                            item.Value.UserLeave(e.User.UserName);
                        }
                    }
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
                    string asd = keyPair.Key.UserName;

                    lock (o)
                    {
                        _userDict.Remove(keyPair.Key);
                    }
                }
                catch (Exception)
                {
                    // Console.WriteLine(ex);
                }
            }
        }

        private string GetClientIp()
        {

            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (endpoint != null)
            {
                if (endpoint.Address == "127.0.0.1")
                {
                    System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    foreach (var item in addressList)
                    {
                        if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return item.ToString();
                        }
                    }

                }
                else
                {
                    return endpoint.Address;
                }

            }

            return EmptyIp;

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
            catch (InvalidOperationException)
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
        private static object o = new object();

       
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
            catch (Exception)
            {
                return -1;
            }

            user.UserIP = GetClientIp();
            user.MAC = mac;
            var client = OperationContext.Current.GetCallbackChannel<IClient>();

            ClientCallbackList.Add(client);

            try
            {
                lock (o)
                {
                    _userDict.Add(user, client);
                }
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


            //BroadcastEvent(this, new ServerEventArgs() { User = user }, UserLogin);
            if (UserLogin != null)
            {
                UserLogin(this, new ServerEventArgs() { User = user });
            }

            var userList = (from c in _userDict.Keys
                            where (c.UserID != user.UserID)
                            select new OnlineUserInfo()
                            {
                                UserName = c.UserName,
                                ImageHash = c.ImageHash,
                                NickName = c.NickName
                            }).ToList();


            client.RefreshUserList(userList);

            return user.UserID;

        }

        public int Login(string mac)
        {
            Users user = null;
            try
            {
                user = _context.Users.Single(us => us.UserName == OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name);
            }
            catch (InvalidOperationException)
            {

                return -1;
            }
            catch (Exception)
            {
                return -1;
            }

            user.UserIP = GetClientIp();
            user.MAC = mac;
            var client = OperationContext.Current.GetCallbackChannel<IClient>();

            ClientCallbackList.Add(client);

            try
            {
                lock (o)
                {
                    _userDict.Add(user, client);
                }
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


            //BroadcastEvent(this, new ServerEventArgs() { User = user }, UserLogin);
            if (UserLogin != null)
            {
                UserLogin(this, new ServerEventArgs() { User = user });
            }

            var userList = (from c in _userDict.Keys
                            where (c.UserID != user.UserID)
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
            //IClient client = OperationContext.Current.GetCallbackChannel<IClient>();

            //Channel_Closing(client, null);
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


        protected class HashAndNameCompare : IEqualityComparer<FileInfoData>
        {
            public bool Equals(FileInfoData x, FileInfoData y)
            {
                return x.Hash == y.Hash && x.Name == y.Name;
            }

            public int GetHashCode(ShareFile.FileInfoData obj)
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

            bool isComplete = false;
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
                    isComplete = true;
                }
                catch (Exception)
                {
                    isComplete = false;
                    //return false;
                    // throw;
                }

            }
            return isComplete;
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
            if (id < 0)
            {
                return;
            }
            using (ShareWareEntities context = new ShareWareEntities())
            {
                foreach (var item in fileList)
                {
                    var res = from c in context.FileOwner
                              where c.UserID == id && c.Hash == item.Hash
                              select c;
                    foreach (var file in res)
                    {
                        context.FileOwner.Remove(file);
                    }
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {

                        // throw;
                    }

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

            foreach (var item in result)
            {

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

        public List<FileInfoData> SearchFile(List<string> nameList, bool mustOnline)
        {
            Debug.Assert(nameList.Count > 0);
            int id = GetClientId();

            if (id < 0)
            {
                return null;
            }

            List<FileInfoData> newList = new List<FileInfoData>();
            List<IQueryable<FileInfoData>> resultList = new List<IQueryable<FileInfoData>>();


            IQueryable<FileInfoData> inter;
            string tempStr = nameList[0];

            inter = (from c in _context.FileOwner
                     where (c.Name.Contains(tempStr))
                     select new FileInfoData()
                     {
                         UserId = c.UserID,
                         Name = c.Name,
                         Hash = c.Hash,
                         Size = c.FileInfo.Size,
                         UserName = c.Users.UserName,
                         IsFolder = c.FileInfo.IsFolder
                     });

            for (int i = 1; i < nameList.Count; i++)
            {
                tempStr = nameList[i];
                inter = (from c in inter
                         where (c.Name.Contains(tempStr))
                         select c);
            }
            var onlineResult = (from b in _userDict.Keys
                                group b.UserID by b into a
                                from c in inter
                                where a.Contains(c.UserId)
                                select c).ToList();

            var offlineResult = inter.ToList().Except(onlineResult).ToList();

            foreach (var item in onlineResult)
            {
                item.IsOnline = true;
            }

            var re = (onlineResult.Concat(offlineResult)).Distinct(new HashAndNameCompare()).ToList();


            return re;
            //return inter.Distinct().ToList();
        }

        public int DownloadRequest(string hash, int nPort)
        {
            //var users = from c in _context.Users
            //            where (fileOnwer.UserID == c.UserID)
            //            select c;
            var users = from c in _context.FileOwner
                        where (c.Hash == hash)
                        select c.Users;

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
            //Tick
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
                string imagePath = ImagePath;
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

            string imagePath = ImagePath;
            string hash = ComputeStringMd5(name);
            string path = ImagePath + hash + ".jpg";
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
                        _userDict[user].OpenShareFolderPerfromance(user.NickName, ip, localPort);
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
                                 UserName = user.UserName,
                                 NickName = user.NickName,
                                 IsMale = user.IsMale,
                                 QQ = user.QQ,
                                 MicroBlog = user.MicroBlog,
                                 Signature = user.Signature,
                                 ImageHash = user.ImageHash
                                 // Password = user.Password
                             };

                return userInfo;
            }
            catch (Exception)
            {

                return null;
            }
        }



        public bool ChangeUserInfo(UserInfo userInfo)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return false;
            }

            using (ShareWareEntities context = new ShareWareEntities())
            {
                try
                {
                    Users user = context.Users.Single(T => T.UserID == id);
                    user.NickName = userInfo.NickName;
                    user.IsMale = userInfo.IsMale;
                    user.QQ = userInfo.QQ;
                    user.MicroBlog = userInfo.MicroBlog;
                    user.Signature = userInfo.Signature;
                    context.SaveChanges();
                }
                catch (Exception)
                {

                    return false;
                }
            }

            return true;
        }


        public bool ChangePassword(string oldPassword, string newPassword)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return false;
            }

            using (ShareWareEntities context = new ShareWareEntities())
            {
                try
                {
                    Users user = context.Users.Single(T => T.UserID == id);

                    if (user.Password == oldPassword)
                    {
                        user.Password = newPassword;
                        context.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }


        public void RemoveNotExistShreFile(string hash)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return;
            }

            try
            {
                using (ShareWareEntities context = new ShareWareEntities())
                {
                    var file = from c in context.FileOwner
                               where (c.UserID == id && c.Hash == hash)
                               select c;
                    foreach (var item in file)
                    {
                        context.FileOwner.Remove(item);
                    }
                    context.SaveChanges();
                }

            }
            catch (Exception)
            {
                return;
            }
        }

        public void RemoveNotExistShreFileList(List<string> hashList)
        {
            int id = GetClientId();
            if (id < 0)
            {
                return;
            }

            try
            {
                using (ShareWareEntities context = new ShareWareEntities())
                {
                    foreach (var hash in hashList)
                    {
                        var file = from c in context.FileOwner
                                   where (c.UserID == id && c.Hash == hash)
                                   select c;
                        foreach (var item in file)
                        {
                            context.FileOwner.Remove(item);
                        }
                    }

                    context.SaveChanges();
                }

            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
