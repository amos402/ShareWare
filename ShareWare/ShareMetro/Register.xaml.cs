﻿using System;
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
	/// Window2.xaml 的交互逻辑
	/// </summary>
	public partial class Register
	{
		public Register()
		{
			this.InitializeComponent();

            RegisterVM vm = DataContext as RegisterVM;
            if (vm != null)
            {
                vm.RequestClose += ((sender, e) => this.Close());
            }
			// 在此点之下插入创建对象所需的代码。
		}
	}
}