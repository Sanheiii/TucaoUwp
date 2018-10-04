using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MUXC=Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// 由于NavigationView有bug,所以将Frame放到外面,随着里面Grid的大小变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = sender as Grid;
            MainFrame.Height = grid.RenderSize.Height;
            MainFrame.Width = grid.RenderSize.Width;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void NavView_ItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
        {

        }

        private void NavView_DisplayModeChanged(MUXC.NavigationView sender, MUXC.NavigationViewDisplayModeChangedEventArgs args)
        {
            //switch(args.DisplayMode)
            //{
            //    case MUXC.NavigationViewDisplayMode.Expanded:
            //        {
            //            sender.IsPaneToggleButtonVisible = false;
            //            break;
            //        }
            //    default:
            //        {
            //            sender.IsPaneToggleButtonVisible = true;
            //            break;
            //        }
            //}
        }
    }
}
