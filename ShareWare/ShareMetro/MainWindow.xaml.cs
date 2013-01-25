using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using ShareWare.ShareFile;
using Socket_Library;
using System.Windows.Resources;

namespace ShareMetro
{
    public partial class MainWindow : MetroWindow
    {
        System.Windows.Forms.NotifyIcon notifyIcon;
        private bool _willClose = false;
        private bool _isShowed = true;

        public void IconShow()
        {
            StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\sharewareIcon.ico", UriKind.Relative));
            System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
            System.Windows.Forms.ToolStripMenuItem item1 = new System.Windows.Forms.ToolStripMenuItem();
            item1.Click += item1_Click;
            item1.Text = "显示主面板";
            System.Windows.Forms.ToolStripMenuItem item2 = new System.Windows.Forms.ToolStripMenuItem();
            item2.Text = "退出";
            item2.Click += item2_Click;
            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);

            this.Closing += MainWindow_Closing;

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.ContextMenuStrip = contextMenu;

            this.notifyIcon.BalloonTipText = "Hello, ShareWare!";
            this.notifyIcon.Text = "Hello, ShareWare!";
            this.notifyIcon.Icon = new System.Drawing.Icon(resourceInfo.Stream);
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            this.notifyIcon.ShowBalloonTip(500);
        }

        void item1_Click(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        void item2_Click(object sender, EventArgs e)
        {
            _willClose = true;
            this.Close();
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            return;
#else

            if (_willClose)
            {
                notifyIcon.Visible = false;
            }
            else
            {
                e.Cancel = true;
                this.Hide();
                _isShowed = false;
            }
#endif
        }

        public void ShowMainWindow()
        {
            this.Show();
            _isShowed = true;
            WindowState = System.Windows.WindowState.Normal;
            this.Activate();
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_isShowed)
            {
                this.Hide();
                _isShowed = false;
            }
            else
            {
                ShowMainWindow();
            }
        }

        private MainWindowViewModel _mainVM;
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            _mainVM = (this.DataContext as MainWindowViewModel);
            InitializeComponent();
            IconShow();
        }


        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                if (model == null) return;
                if (!_mainVM.Islist) return;
                string name = "";
                //if (model.Hash == null) 
                name = model.Name;
                //else name = model.Hash;
                if (model.Type == "文件夹") _mainVM.LoadItems(name);
            }
        }

        private void ListViewItem_SelectionClick(object sender, SelectionChangedEventArgs e)
        {
            if (e.RoutedEvent.Name == "SelectionChanged")
            {
                ListView item = sender as ListView;
                if (item.DataContext != null)
                {
                    FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                    _mainVM.SeleteInfo = model;
                }
            }
        }

        private void SelectedItem(object sender, SelectionChangedEventArgs e)
        {
            if (e.RoutedEvent.Name == "SelectionChanged")
            {
                ListView item = sender as ListView;
                if (item.DataContext != null)
                {
                    DownListInfo model = item.SelectedItem as DownListInfo;
                    _mainVM.Down_list_Selete = model;
                }
            }
        }

        private void Down_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem(sender, e);
        }

        private void History_listveiw_SelectinChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem(sender, e);
        }

        private void Garbage_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem(sender, e);
        }

        private void ListBox_Mouse_Double_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBox item = sender as ListBox;
            if (item.DataContext != null)
            {
                OnlineUserData model = item.SelectedItem as OnlineUserData;
                if (model == null) return;
                _mainVM.Islist = true;
                _mainVM.Directory = "";
                _mainVM.CallLoad(model.UserName);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RoutedEvent.Name == "SelectionChanged")
            {
                ListBox item = sender as ListBox;
                if (item.DataContext != null)
                {
                    OnlineUserData model = item.SelectedItem as OnlineUserData;
                    _mainVM.SeleteManInfo = model;
                }
            }
        }

        private void Share_DoubleCilk(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                if (model == null) return;
                if (model.Type == "文件夹") _mainVM.Show_ShareFile(model.Name, _mainVM.ShareLoact);
            }
        }


        private void Get_ListViewItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                if (model == null) return;
                string name = "";
                //if (model.Hash == null) 
                name = model.Name;
                //else name = model.Hash;
                _mainVM.SeleteInfo = model;
            }
        }

        private void Men_Down(object sender, RoutedEventArgs e)
        {
            _mainVM.CreatDowndLoad(null);
        }

        private void Men_OpenD(object sender, RoutedEventArgs e)
        {
            if (_mainVM.SeleteInfo.Type == "文件夹")
                _mainVM.LoadItems(_mainVM.SeleteInfo.Name);
        }


        private void Get_ListBox_Item(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBox item = sender as ListBox;
            if (item.DataContext != null)
            {
                OnlineUserData model = item.SelectedItem as OnlineUserData;
                if (model == null) return;
                _mainVM.SeleteManInfo = model;
            }
        }

        private void Men_OpenShareFile(object sender, RoutedEventArgs e)
        {
            _mainVM.Islist = true;
            _mainVM.Directory = "";
            _mainVM.CallLoad(_mainVM.SeleteManInfo.UserName);
        }

        private void Men_Talk(object sender, RoutedEventArgs e)
        {
            ListBox item = e.Source as ListBox;
            if (item.DataContext != null)
            {
                OnlineUserData model = item.SelectedItem as OnlineUserData;
                if (model == null) return;
                _mainVM.SeleteManInfo = model;
                _mainVM.Talk(model);
            }
        }

        private void Get_Down_ListView(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = e.Source as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                _mainVM.SeleteInfo = model;
            }
        }

        private void Nem_Com_Down(object sender, RoutedEventArgs e)
        {
            _mainVM.Go_DowndLoad(null);
        }

        private void Men_Stop_Down(object sender, RoutedEventArgs e)
        {

            _mainVM.Stop_DowndLoad(null);
        }

        private void Men_Dele_Down(object sender, RoutedEventArgs e)
        {

            _mainVM.Detelet_DownListViewInfo(null);
        }

        private void Get_He_ListView(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = e.Source as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                _mainVM.SeleteInfo = model;
            }
        }

        private void Men_Dele_He(object sender, RoutedEventArgs e)
        {

            _mainVM.Detelet_HistoryListViewInfo(null);

        }

        private void Men_Open_He(object sender, RoutedEventArgs e)
        {
            _mainVM.Open_File(null);

        }

        private void Get_Ge_ListvView(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = e.Source as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                _mainVM.SeleteInfo = model;
                _mainVM.Reduction_GarbageInfo(null);
            }
        }

        public void Men_Re_Ge(object sender, RoutedEventArgs e)
        {

            _mainVM.Reduction_GarbageInfo(null);

        }

        private void Men_Dele_Ge(object sender, RoutedEventArgs e)
        {
            _mainVM.Detelet_File(null);

        }

        private void ShareFile_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                if (model == null) return;
                if (model.Type == "文件夹") _mainVM.Selection_ShareFile = model;
            }
        }

        private void Get_ShareFile_Item(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ;
        }

        private void Men_ShareFile_Dele(object sender, RoutedEventArgs e)
        {
            if (_mainVM.Selection_ShareFile == null) return;
            _mainVM.Delete_ShareFile(null);
        }

        private void Men_ShareFile_Open(object sender, RoutedEventArgs e)
        {
            if (_mainVM.Selection_ShareFile == null) return;
            if (_mainVM.Selection_ShareFile.Type == "文件夹")
                _mainVM.Open_shareFile(null);
        }
    }
}
