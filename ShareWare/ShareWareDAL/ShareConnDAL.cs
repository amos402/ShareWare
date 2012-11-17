using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareWare.DAL
{
    public class NewUser
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }


    public class UserDAL
    {
        private SqlConnection sqlCn = null;

        #region Open / Close methods
        public void OpenConnection(string connectionString)
        {
            sqlCn = new SqlConnection();
            sqlCn.ConnectionString = connectionString;
            sqlCn.Open();
        }

        public void CloseConnection()
        {
            sqlCn.Close();
        }
        #endregion

        #region Select methods
        public DataTable GetAllInventoryAsDataTable()
        {
            // This will hold the records.
            DataTable inv = new DataTable();

            // Prep command object.
            string sql = "Select * From Inventory";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                // Fill the DataTable with data from the reader and clean up.
                inv.Load(dr);
                dr.Close();
            }
            return inv;
        }
        public List<NewUser> GetAllUserAsList()
        {
            // This will hold the records.
            List<NewUser> inv = new List<NewUser>();

            // Prep command object.
            string sql = "Select * From Users";
            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    inv.Add(new NewUser
                    {
                        UserID = (int)dr["UserID"],
                        UserName = (string)dr["UserName"],
                        PassWord = (string)dr["PassWord"]
                    });
                }
                dr.Close();
            }
            return inv;
        }
        #endregion

        public bool CheckUser(string userName, string passWord)
        {
            string sql = string.Format("Select * From Users where UserName='{0}' and PassWord='{1}'",
                userName, passWord);

            using (SqlCommand cmd = new SqlCommand(sql, this.sqlCn))
            {
                SqlDataReader dr = cmd.ExecuteReader();
                bool bConfirm = dr.Read();
                dr.Close();
                if (bConfirm)
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
