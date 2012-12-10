using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ShareWare.ServiceLibrary
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class RegisterService : IRegisterService
    {
        public RegError Register(UserInfo userInfo)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                using (ShareWareEntities context = new ShareWareEntities())
                {
                    try
                    {
                        var result = from c in context.Users
                                     where c.UserName == userInfo.UserName
                                     select c;
                        if (result.Count() > 0)
                        {
                            return RegError.UserExist;
                        }

                        Users newUser = new Users()
                        {
                            UserName = userInfo.UserName,
                            Password = userInfo.Password,
                            NickName = userInfo.NickName,
                            IsMale = userInfo.IsMale,
                            QQ = userInfo.QQ,
                            MicroBlog = userInfo.MicroBlog,
                            Signature = userInfo.Signature
                        };
                        context.Users.Add(newUser);

                        context.SaveChanges();
                       
                        
                        var user = context.Users.Single(T => T.UserName == newUser.UserName);
                        string nameHash = ShareService.ComputeStringMd5(user.UserName);
                        IDictionary section = (IDictionary)ConfigurationManager.GetSection("ImagePath");
                        string imagePath = section["Path"].ToString();
                        if (!Directory.Exists(imagePath))
                        {
                            Directory.CreateDirectory(imagePath);
                        }
                        if (userInfo.Image != null)
                        {

                            string filePath = imagePath + nameHash.ToString() + ".jpg";
                            userInfo.Image.Save(filePath, ImageFormat.Jpeg);
                            string fileHash = ShareService.ComputeFileMd5(filePath);

                            user.ImageHash = fileHash;
                            context.SaveChanges(); 
                        }

                        transaction.Complete();
                    }
                    catch (Exception)
                    {

                        return RegError.Ohter;
                    }
                }

            }

            return RegError.NoError;
        }

    }


}
