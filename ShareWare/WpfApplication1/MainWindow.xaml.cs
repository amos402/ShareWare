using ShareWare.ShareFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //ShareFiles sh = new ShareFiles();
            //sh.AddSystemSharePath();
            //var shit = sh.ShareName.ToList();
            //listView1.ItemsSource = shit;
            //var damn = listView1.DataContext as ShareFiles;
            //var shit = listView1.ItemsSource as Dictionary<string, string>;
            //shit.AddSystemSharePath();
            //damn.AddSystemSharePath();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var damn = this.DataContext as VM;
            damn.FilePath.AddSystemSharePath();
            listView1.ItemsSource = damn.SharePath;
        }

        //public List< MyProperty { get; set; }
    }
}
