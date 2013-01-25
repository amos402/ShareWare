using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShareMetro
{
    /// <summary>
    /// ThirdLogin.xaml 的交互逻辑
    /// </summary>
    public partial class ThirdLogin : MetroWindow
    {
        public ThirdLogin()
        {
            InitializeComponent();

        }

        private void WebBrowser_Navigated_1(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://amos402.host-ed.me/?"))
            {
                string result = GetElementTextById("Result");
                if (result == "Success")
                {
                    MainWindowViewModel model = (MainWindowViewModel)Owner.DataContext;
                    model.UserName = GetElementTextById("NickName");
                    Close();
                }

            }
        }

        private string GetElementTextById(string id)
        {
            try
            {
                mshtml.IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)webBrowser1.Document;
                mshtml.IHTMLElement element = doc2.all.item(id, 0);
                return element.getAttribute("InnerText");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
