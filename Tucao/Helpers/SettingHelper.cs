using Windows.Storage;
using Windows.System.Display;

namespace Tucao.Helpers
{
    class SettingHelper
    {
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        static DisplayRequest displayRequest = new DisplayRequest();
        /// <summary>
        /// 设置屏幕常亮
        /// </summary>
        public static bool IsScreenAlwaysOn
        {
            set
            {
                if (value == true)
                {
                    try
                    {
                        displayRequest.RequestActive();
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        displayRequest.RequestRelease();
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            return null;
        }

        /// <summary>
        /// 设置指定键的值
        /// </summary>
        public static void SetValue(string key, object value)
        {
            container.Values[key] = value;
        }

        /// <summary>
        /// 是否存在某键
        /// </summary>
        public static bool ContainsKey(string key)
        {
            return container.Values[key] != null;
        }
    }
}
