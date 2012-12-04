using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Socket_Library;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            //SendShareFileInfo d = new SendShareFileInfo();
            //d.CreatSocket("192.168.163.79",5000);
            //d.Local_ID = "asd";
            //string s = @"D:\Potplayer";
            //string s = @"D:\C++\C#\E6_MyExplorer\E6_MyExplorer\bin\Debug\E6_MyExplorer.exe";
            //string s = @"D:\C++\C#\e5_6_1DrawLine\e5_6_1DrawLine\bin\Debug\e5_6_1DrawLine.exe";
            // string s = @"D:\QQ\QQ2012Beta1.exe";
            string s = @"D:\DriverGenius2012";
            //string s = @"D:\goagent";
            int typt = 1;
            string ip = "192.168.163.79";
            int port = 6000;
            Upload u = new Upload();
            u.CreatUpload(s, ip, typt, port);
            Console.ReadLine();
        }
    }
}
