using CopyCat.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CopyCat.Wpf.Views
{
    /// <summary>
    /// HostView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HostView : UserControl
    {
        public HostView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as HostViewModel;
            //viewModel.SetHostView(this, "../../../../Unity.Build/CopyCat.exe");
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = this.DataContext as HostViewModel;
            //viewModel.Refresh();
        }
    }
}
