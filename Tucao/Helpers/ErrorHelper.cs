using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
