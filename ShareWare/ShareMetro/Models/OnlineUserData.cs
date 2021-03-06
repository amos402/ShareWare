﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ShareMetro
{
    public class OnlineUserData
    {
        public OnlineUserData()
        {
            IsVisible = true;
        }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public BitmapImage Image { get; set; }
        public bool IsVisible { get; set; }
    }
}
