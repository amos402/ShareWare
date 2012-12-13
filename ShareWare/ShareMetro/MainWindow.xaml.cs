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
    public partial class MainWindow
    {
        System.Windows.Forms.NotifyIcon notifyIcon;

        public void IconShow()
        {
            StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(@"images\sharewareIcon.ico", UriKind.Relative));

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.BalloonTipText = "Hello, NotifyIcon!";
            this.notifyIcon.Text = "Hello, NotifyIcon!";
            this.notifyIcon.Icon = new System.Drawing.Icon(resourceInfo.Stream);
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // this.notifyIcon.ShowBalloonTip(500);
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
           WindowState = System.Windows.WindowState.Normal;
           this.Visibility = System.Windows.Visibility.Visible;
           // this.sho
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    this.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case WindowState.Normal:
                    this.Visibility = System.Windows.Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        public MainWindow()
        {
            DataContext = new MainWindowViewModel(Dispatcher);
            InitializeComponent();
            var t = new DispatcherTimer();
            t.Tick += Tick;
            t.Interval = new TimeSpan(0, 0, 0, 1);
            t.Start();
            this.StateChanged += MainWindow_StateChanged;
            this.Closing += ((sender1, e1) => notifyIcon.Visible = false);
            IconShow();
        }



        void Tick(object sender, EventArgs e)
        {
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            // var x = pivot.Items;

            //Flyouts[0].IsOpen = !Flyouts[0].IsOpen;
            //Flyouts[2].IsOpen = !Flyouts[2].IsOpen;

        }

        private void MiLightRed(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Red"), Theme.Light);
        }

        private void MiDarkRed(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Red"), Theme.Dark);
        }

        private void MiLightGreen(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Green"), Theme.Light);
        }

        private void MiDarkGreen(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Green"), Theme.Dark);
        }

        private void MiLightBlue(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Blue"), Theme.Light);
        }

        private void MiDarkBlue(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Blue"), Theme.Dark);
        }

        private void MiLightPurple(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Purple"), Theme.Light);
        }

        private void MiDarkPurple(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Purple"), Theme.Dark);
        }

        private void BtnPanoramaClick(object sender, RoutedEventArgs e)
        {
            //new ChildWindow().ShowDialog();

            //new Windows1().Show();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //pivot.GoToItem(pi3);
            //((MainWindowViewModel) this.DataContext).SelectedIndex = 2;
        }

        private void BtnVSClick(object sender, RoutedEventArgs e)
        {
            //new VSDemo().Show();
        }

        private void MiDarkOrange(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Orange"), Theme.Dark);
        }

        private void MiLightOrange(object sender, RoutedEventArgs e)
        {
            ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Orange"), Theme.Light);
        }

        private void IgnoreTaskbarOnMaximizedClick(object sender, RoutedEventArgs e)
        {
            this.IgnoreTaskbarOnMaximize = !this.IgnoreTaskbarOnMaximize;
        }


        #region ACE写的
        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                if (model.Type == "文件夹") (this.DataContext as MainWindowViewModel).LoadItems(name);
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
                    (this.DataContext as MainWindowViewModel).SeleteInfo = model;
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
                    (this.DataContext as MainWindowViewModel).Down_list_Selete = model;
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
                (this.DataContext as MainWindowViewModel).Directory = "";
                (this.DataContext as MainWindowViewModel).CallLoad(model.UserName);
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
                    (this.DataContext as MainWindowViewModel).SeleteManInfo = model;
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
                if (model.Type == "文件夹") (this.DataContext as MainWindowViewModel).Show_ShareFile(model.Name, (this.DataContext as MainWindowViewModel).ShareLoact);
            }
        }
        #endregion

    }
}
