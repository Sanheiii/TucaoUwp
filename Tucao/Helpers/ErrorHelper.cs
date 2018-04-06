using System;

namespace Tucao.Helpers
{
    class ErrorHelper
    {
        public static async void PopUp(string message,string title)
        {
            var msgDialog = new Windows.UI.Popups.MessageDialog(message,title);
            await msgDialog.ShowAsync();
        }
        public static void PopUp(string message)
        {
            PopUp(message, "");
        }
    }
}
