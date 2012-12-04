using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using ShareWare.ShareFile;
using Socket_Library;

namespace ShareMetro
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel(Dispatcher);
            InitializeComponent();
            var t = new DispatcherTimer();
            t.Tick += Tick;
            t.Interval = new TimeSpan(0, 0, 0, 1);
            t.Start();
            var shit = new Flyout();
            shit.Header = "fuck";
            Flyouts.Add(shit);
        }

        void Tick(object sender, EventArgs e)
        {
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            // var x = pivot.Items;

            Flyouts[0].IsOpen = !Flyouts[0].IsOpen;
            Flyouts[2].IsOpen = !Flyouts[2].IsOpen;

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

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.DataContext != null)
            {
                FileInfoDataList model = item.SelectedItem as FileInfoDataList;
                if (model.Type == "文件夹") (this.DataContext as MainWindowViewModel).LoadItems(model.Name);
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

    

        
    }
}
