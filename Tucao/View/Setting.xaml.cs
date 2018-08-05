using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Tucao.Helpers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Page
    {
        public Setting()
        {
            string appVersion = string.Format("Ver {0}.{1}.{2}.{3}",
                       Package.Current.Id.Version.Major,
                       Package.Current.Id.Version.Minor,
                       Package.Current.Id.Version.Build,
                       Package.Current.Id.Version.Revision);
            this.InitializeComponent();
            Version.Text = appVersion;
            DanmakuSizeSlider.Value = (double?)SettingHelper.GetValue("DanmakuSize") ?? 0.7;
            DanmakuSpeedSlider.Value = (double?)SettingHelper.GetValue("DanmakuSpeed") ?? 0.6;
            DanmakuOpacitySlider.Value = (double?)SettingHelper.GetValue("DanmakuOpacity") ?? 1;
            ShadowTest.FontSize = 25 * DanmakuSizeSlider.Value;
            DanmakuTest.FontSize = 25 * DanmakuSizeSlider.Value;
            DanmakuGrid.Opacity = DanmakuOpacitySlider.Value;
            DanmakuSizeSlider.Loaded += ((sender, e) => DanmakuSizeSlider.ValueChanged += DanmakuSizeSlider_ValueChanged);
            DanmakuSpeedSlider.Loaded += ((sender, e) => DanmakuSpeedSlider.ValueChanged += DanmakuSpeedSlider_ValueChanged);
            DanmakuOpacitySlider.Loaded += ((sender, e) => DanmakuOpacitySlider.ValueChanged += DanmakuOpacitySlider_ValueChanged);
        }
        //设置这些值
        private void DanmakuOpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuOpacity", e.NewValue);
            DanmakuGrid.Opacity = e.NewValue;
        }
        private void DanmakuSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuSpeed", e.NewValue);
        }
        private void DanmakuSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuSize", e.NewValue);
            ShadowTest.FontSize = 25 * e.NewValue;
            DanmakuTest.FontSize = 25 * e.NewValue;
        }
        //点击弹幕的恢复默认
        private void ResetDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmakuSizeSlider.Value = 0.7;
            DanmakuSpeedSlider.Value = 0.6;
            DanmakuOpacitySlider.Value = 1;
        }
        private void Reward_Click(object sender, RoutedEventArgs e)
        {
            RewardImage.ShowAt(sender as HyperlinkButton);
        }
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            var task=ComposeEmail();
        }
        private async Task ComposeEmail()
        {
            var emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("w953934508@outlook.com", "三黑"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }


    }
}
