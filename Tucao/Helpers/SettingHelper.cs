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
        private static object GetValue(string key)
        {
            return container.Values[key];
        }

        /// <summary>
        /// 设置指定键的值
        /// </summary>
        private static void SetValue(string key, object value)
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
        public class Values
        {
            /// <summary>
            /// 上一次打开app的版本
            /// </summary>
            public static string Version
            {
                get
                {
                    return (string)(GetValue("Version") ?? "");
                }
                set
                {
                    SetValue("Version", value);
                }
            }
            /// <summary>
            /// 播放视频时的默认音量
            /// </summary>
            public static double Volume
            {
                get
                {
                    return (double)(GetValue("Volume") ?? 100.0);
                }
                set
                {
                    SetValue("Volume", value);
                }
            }
            /// <summary>
            /// 是否自动转屏
            /// </summary>
            public static bool IsAutoRotate
            {
                get
                {
                    return (bool)(GetValue("IsAutoRotate") ?? true);
                }
                set
                {
                    SetValue("IsAutoRotate", value);
                }
            }
            /// <summary>
            /// 是否显示弹幕
            /// </summary>
            public static bool IsShowDanmaku
            {
                get
                {
                    return (bool)(GetValue("IsShowDanmaku") ?? true);
                }
                set
                {
                    SetValue("IsShowDanmaku", value);
                }
            }
            /// <summary>
            /// 是否显示滚动弹幕
            /// </summary>
            public static bool IsShowScrollableDanmaku
            {
                get
                {
                    return (bool)(GetValue("IsShowScrollableDanmaku") ?? true);
                }
                set
                {
                    SetValue("IsShowScrollableDanmaku", value);
                }
            }
            /// <summary>
            /// 是否显示顶部弹幕
            /// </summary>
            public static bool IsShowTopDanmaku
            {
                get
                {
                    return (bool)(GetValue("IsShowTopDanmaku") ?? true);
                }
                set
                {
                    SetValue("IsShowTopDanmaku", value);
                }
            }
            /// <summary>
            /// 是否显示底部弹幕
            /// </summary>
            public static bool IsShowBottomDanmaku
            {
                get
                {
                    return (bool)(GetValue("IsShowBottomDanmaku") ?? true);
                }
                set
                {
                    SetValue("IsShowBottomDanmaku", value);
                }
            }
            /// <summary>
            /// 弹幕的大小倍率
            /// </summary>
            public static double DanmakuSize
            {
                get
                {
                    return (double)(GetValue("DanmakuSize") ?? 0.7);
                }
                set
                {
                    SetValue("DanmakuSize", value);
                }
            }
            /// <summary>
            /// 滚动弹幕的速度倍率
            /// </summary>
            public static double DanmakuSpeed
            {
                get
                {
                    return (double)(GetValue("DanmakuSpeed") ?? 0.6);
                }
                set
                {
                    SetValue("DanmakuSpeed", value);
                }
            }
            /// <summary>
            /// 弹幕的不透明度
            /// </summary>
            public static double DanmakuOpacity
            {
                get
                {
                    return (double)(GetValue("DanmakuOpacity") ?? 1.0);
                }
                set
                {
                    SetValue("DanmakuOpacity", value);
                }
            }
        }
    }
}
