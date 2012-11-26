﻿using ConsoleApplication1.ServiceReference;
using ShareWare;
using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    class Program
    {


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

            //Thread.Sleep(1000);

            //CallBack callBack = new CallBack();
            //ShareServiceClient client = new ShareServiceClient(new InstanceContext(callBack));



            //    var state = client.State;
            //    //int nId1 = client.Login("Amos", "asd");
            //    CompositeType type = new CompositeType();
            //    type.BoolValue = true;
            //    type.StringValue = "dadmamsdasmdasda";

            //   // CompositeType shit = client.GetDataUsingDataContract(type);

            //    ShareWareClient swc = new ShareWareClient(ref client);
            //    bool done = swc.Login("Amos", "asd");
            //    swc.UploadFileInfo();

            //    //ShareFiles sh = new ShareFiles();
            //    //List<ShareWare.ShareFile.FileInfoTransfer> fileList = new List<ShareWare.ShareFile.FileInfoTransfer>();
            //    //foreach (var item in sh.ShareFileDict)
            //    //{
            //    //    foreach (var item1 in item.Value)
            //    //    {
            //    //        fileList.Add(new ShareWare.ShareFile.FileInfoTransfer() { Name = item1.File.Name, Hash = item1.Hash, Size = item1.File.Length});
            //    //    }
            //    //}

            //    //swc.UploadFileInfo();
            //    //Thread.Sleep(60000);
            //    swc.SearchFile("person");
            //    Console.WriteLine("Done");
            //string[] hash = HashHelper.ComputeSHA1ByParts(@"R:\temp.torrent");

            //int asd = hash[0].Count();
            //var fileName = @"R:\DataTriggerDemo.rar";
            //var hash = HashHelper.ComputeMD5(fileName);
            //var hashList = HashHelper.ComputeSHA1ByParts(fileName, 16);
            Thread.Sleep(1000);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            ShareFiles sh = new ShareFiles();

            sh.AddSharePath(@"asd", @"R:\");

            //sh.AddSharePath("asd", @"D:\asd");
            //sh.OnePathComplete += ((sender, e) => Console.WriteLine("asdasdad"));
            //sh.OnePathComplete += ((asd, e) => Console.WriteLine(asd));
            //Thread t = sh.ListFile();
            //t.Join();
            sw.Stop();


            //sh.Serialize(@"config\known.met");

            //var asd = ShareFiles.Deserialize(@"config\known.met");

            CallBack callBack = new CallBack();
            ShareServiceClient client = new ShareServiceClient(new InstanceContext(callBack));

            //bool res = client.Register("shit", "asdd", "asdasd@ad.com");
            int id = client.Login("Amos", "asd", GetFirstMac());

            sh = ShareFiles.Deserialize(@"R:\shit.damn.fuck");

            client.UploadShareInfo(sh.FileList, id);

            Console.WriteLine("Lonin ID : {0}", id);

            //client.UploadShareInfo(fileList, id);
            Console.ReadKey();
            var file = client.SearchFile("Data");
            
            
            client.DownloadRequest(file[0], 5000);

            Console.ReadLine();


        }

        public static string GetFirstMac()
        {
            string mac = null;
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection queryCollection = query.Get();
            foreach (ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {
                    mac = mo["MacAddress"].ToString();
                    break;
                }
            }
            return (mac);
        }


    }



    public class CallBack : IShareServiceCallback
    {


        public void DownloadPerformance(string szHash, string szIp, int nPort)
        {
            Console.WriteLine("send to {0} {1} {2}", szHash, szIp, nPort);
        }


        public void NewUser(int id, string name)
        {
            Console.WriteLine("New user : {0}  {1}", id, name);
        }

        public void RefreshUserList(List<string> userList)
        {
            Console.WriteLine("Online users :");
            foreach (var item in userList)
            {
                Console.WriteLine(item);
            }
        }

        public void UserLeave(string name)
        {
            Console.WriteLine("{0} Leave", name);
        }
    }



}
