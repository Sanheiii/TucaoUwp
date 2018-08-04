using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        }

        private void Reward_Click(object sender, RoutedEventArgs e)
        {
            RewardImage.ShowAt(sender as HyperlinkButton);
        }
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            ComposeEmail();
        }
        private async Task ComposeEmail()
        {
            var emailMessage = new EmailMessage();
            emailMessage.To.Add(new EmailRecipient("w953934508@outlook.com", "三黑"));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }
    }
}
