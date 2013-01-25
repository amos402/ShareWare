using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.ServiceLibrary
{
    class UserValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            using (ShareWareEntities context = new ShareWareEntities())
            {
                context.Users.Single(us => us.UserName == userName && us.Password == password);
            }
            //if (userName != "asd" || password != "asdd")
            //{
            //    throw new System.IdentityModel.Tokens.SecurityTokenException("Unknown Username or Password");
            //    // throw new Exception("sjhiiiidjasdadasdad");
            //}
        }
    }
}
