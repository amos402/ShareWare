﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ShareFile
{
    public static class HashHelper
    {
        private static string ComputeFileMd5Base(Stream fs, out List<string> hashList, bool bParts = true)
        {
            MD5CryptoServiceProvider sha1 = new MD5CryptoServiceProvider();
            MD5CryptoServiceProvider sha2 = new MD5CryptoServiceProvider();

            byte[] buffer = new Byte[512 * 1024];
            long offset = 0;
            int len = fs.Read(buffer, 0, buffer.Length);

            hashList = bParts ? new List<string>() : null;

            while (fs.Position < fs.Length)
            {
                if (bParts == true)
                {
                    if (fs.Position - offset < 20971520)
                    {
                        sha2.TransformBlock(buffer, 0, len, buffer, 0);
                    }
                    else
                    {
                        sha2.TransformFinalBlock(buffer, 0, len);
                        hashList.Add(BytesToStr(sha2.Hash));
                        sha2.Dispose();
                        sha2 = new MD5CryptoServiceProvider();
                        offset = fs.Position;
                    }
                }
                sha1.TransformBlock(buffer, 0, len, buffer, 0);
                len = fs.Read(buffer, 0, buffer.Length);
            }
            if (bParts == true)
            {
                sha2.TransformFinalBlock(buffer, 0, len);
                hashList.Add(BytesToStr(sha2.Hash));
            }
            sha1.TransformFinalBlock(buffer, 0, len);
            return BytesToStr(sha1.Hash);
        }

        public static string ComputeFileMd5(string file, out List<string> hashList)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeFileMd5Base(fs, out hashList);
            }
        }

        public static string ComputeFileMd5(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                List<string> hashList;
                return ComputeFileMd5Base(fs, out hashList, false);
            }
        }

        public static string ComputeFileMd5(this Stream fs, out List<string> hashList)
        {
            return ComputeFileMd5Base(fs, out hashList);
        }

        public static string ComputeFileMd5(this Stream fs)
        {
            List<string> hashList;
            return ComputeFileMd5Base(fs, out hashList, false);
        }

        public static string BytesToStr(byte[] bytes)
        {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
                str.AppendFormat("{0:X2}", bytes[i]);

            return str.ToString();
        }

        public static string ComputeStringMd5(string str)
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

        //public static string ComputeFileMd5(string path)
        //{
        //    try
        //    {
        //        MD5 md5 = MD5.Create();
        //        FileStream fs = File.OpenRead(path);
        //        byte[] data = md5.ComputeHash(fs);
        //        fs.Close();
        //        StringBuilder hash = new StringBuilder();
        //        foreach (var item in data)
        //        {
        //            hash.Append(item.ToString("x2"));
        //        }
        //        return hash.ToString();
        //    }
        //    catch (Exception)
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}
