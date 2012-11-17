using ConsoleApplication1.ServiceReference;
using ShareWare;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
       public static ShareWareEntities _context = new ShareWareEntities();
        public static int Login(string userName, string passWord)
        {
            var user = from c in _context.Users
                       where (c.UserName == userName && c.Password == passWord)
                       select c;
            if (user.Count() == 1)
            {
                string asd = user.GetType().ToString();
                return user.First().UserID;

            }

            return -1;

        }

        static void Main(string[] args)
        {

/*
            #region MyRegion
            int nId = Login("Amos", "asd");

            ShareFiles sh = new ShareFiles();

            try
            {
                foreach (var item in sh.ShareFileDict)
                {
                    foreach (var item1 in item.Value)
                    {
                        var info = from c in _context.FileInfo.Local where (c.Hash == item1.Hash) select c;

                        if (info.Count() == 0)
                        {
                            _context.FileInfo.Add(new ShareWare.FileInfo() { Hash = item1.Hash, Name = item1.File.Name, Size = (int)item1.File.Length });

                        }
                        _context.FileOwner.Add(new FileOwner() { Hash = item1.Hash, UserID = 1, Name = item1.File.Name });

                    }

                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }


            List<CustFileInfo> list = sh.SearchFile("asdasdasdasd");
            FileStream stream = new FileStream(@"R:\person.dat", FileMode.OpenOrCreate);
            BinaryFormatter bFormat = new BinaryFormatter();




            bFormat.Serialize(stream, sh);
            stream.Close();


            FileStream inStream = new FileStream(@"R:\person.dat", FileMode.Open);
            var table = bFormat.Deserialize(inStream);
            //table.GetType()
            Type type = table.GetType(); 
            #endregion
           */

            Thread.Sleep(1000);

            CallBack callBack = new CallBack();
            ShareServiceClient client = new ShareServiceClient(new InstanceContext(callBack));
            


                var state = client.State;
                //int nId1 = client.Login("Amos", "asd");
                CompositeType type = new CompositeType();
                type.BoolValue = true;
                type.StringValue = "dadmamsdasmdasda";

               // CompositeType shit = client.GetDataUsingDataContract(type);

                ShareWareClient swc = new ShareWareClient(ref client);
                bool done = swc.Login("Amos", "asd");
                swc.UploadFileInfo();

                //ShareFiles sh = new ShareFiles();
                //List<ShareWare.ShareFile.FileInfoTransfer> fileList = new List<ShareWare.ShareFile.FileInfoTransfer>();
                //foreach (var item in sh.ShareFileDict)
                //{
                //    foreach (var item1 in item.Value)
                //    {
                //        fileList.Add(new ShareWare.ShareFile.FileInfoTransfer() { Name = item1.File.Name, Hash = item1.Hash, Size = item1.File.Length});
                //    }
                //}

                //swc.UploadFileInfo();
                //Thread.Sleep(60000);
                swc.SearchFile("person");
                Console.WriteLine("Done");
                Console.ReadLine();
            

        }
    }

        

    public class CallBack : IShareServiceCallback
    {

        public void GetFilePath()
        {
            
            throw new NotImplementedException();
        }
    }



}
