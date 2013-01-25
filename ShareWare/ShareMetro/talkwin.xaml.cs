using System;
using System.Collections.Generic;
using System.Text;
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
    /// talkwin.xaml 的交互逻辑
    /// </summary>
    public partial class talkwin
    {
        public talkwin()
        {
            this.Closing += talkwin_Closing;
            this.InitializeComponent();
            // 在此点之下插入创建对象所需的代码。
        }

        void talkwin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //throw new NotImplementedException();
            ((talkwinMVVMcs)this.DataContext).Close_Talk();
            ((talkwinMVVMcs)this.DataContext).talk.CloseTalk();
        }

        private void DownList_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView m = sender as ListView;
            if (m.DataContext != null)
            {
                sendFileLsitClass model = m.SelectedItem as sendFileLsitClass;
                (this.DataContext as talkwinMVVMcs).SelectionChange = model;
            }
        }
    }
}