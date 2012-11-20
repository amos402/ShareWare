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

        private string GetClientIp()
        {
            //提供方法执行的上下文环境
            OperationContext context = OperationContext.Current;
            //获取传进的消息属性
            MessageProperties properties = context.IncomingMessageProperties;
            //获取消息发送的远程终结点IP和端口
            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            return endpoint.Address;
        }

        public int Login(string userName, string passWord)
        {
            var user = from c in _context.Users
                       where (c.UserName == userName && c.Password == passWord)
                       select c;
            if (user.Count() == 1)
            {

                user.First().UserIP = GetClientIp();


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

            DataTable dt1 = new DataTable();
            dt1.Columns.AddRange(new DataColumn[]{
                new DataColumn("Hash",typeof(string)),  
                new DataColumn("Name",typeof(string)),  
                new DataColumn("Size",typeof(long))});


            DataTable dt2 = new DataTable();
            dt2.Columns.AddRange(new DataColumn[]{
                new DataColumn("ID", typeof(int)) { AutoIncrement = true},
                new DataColumn("UserID",typeof(int)),
                new DataColumn("Name",typeof(string)),
                new DataColumn("Hash",typeof(string)) {AllowDBNull = true}
            });


            foreach (var item in fileList)
            {
                DataRow r1 = dt1.NewRow();
                r1[0] = item.Hash;
                r1[1] = item.Name;
                r1[2] = item.Size;
                dt1.Rows.Add(r1);

                DataRow r2 = dt2.NewRow();
                r2[1] = userId;
                r2[2] = item.Name;
                r2[3] = item.Hash;
                dt2.Rows.Add(r2);

            }

            var conn = _context.Database.Connection;
            conn.Open();
            SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)_context.Database.Connection);
            try
            {

                bulkCopy.DestinationTableName = "FileInfo";
                bulkCopy.BatchSize = dt1.Rows.Count;

                bulkCopy.WriteToServer(dt1);
            }
            catch (Exception e)
            {

                // Console.WriteLine(e.Message);
            }


            try
            {
                bulkCopy.DestinationTableName = "FileOwner";
                bulkCopy.BatchSize = dt2.Rows.Count;

                bulkCopy.WriteToServer(dt2);

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
            var result = from c in _context.FileOwner where (c.Name.Contains(fileName)) select c;
            var list = result.ToList();
            var newList = new List<FileOwner>();
            foreach (var item in list)
            {
                newList.Add(new FileOwner(){
                    ID = item.ID,
                    UserID = item.UserID,
                    Hash = item.Hash,
                     Name = item.Name
                });
            }

            return newList;

        }


        public void DownloadRequest(FileOwner fileOnwer, int nPort)
        {
            var users = from c in _context.Users
                        where (fileOnwer.UserID == c.UserID)
                        select c;
            string ip = GetClientIp();
            foreach (var item in users)
            {
                _userDict[item].DownloadPerformance(fileOnwer.Hash, ip, nPort);
            }
        }
    }
}


