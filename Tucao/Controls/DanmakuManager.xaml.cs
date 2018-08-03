using System;
using System.Collections;
using System.Collections.Generic;
using Tucao;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Controls
{
    public sealed partial class DanmakuManager : UserControl
    {
        //弹幕字号为1的字体大小
        double fontSize = 25;
        //弹幕速度为1时的速度
        double speed = 0.25;
        /// <summary>
        /// 弹幕字号
        /// </summary>
        private double danmakuSize = 25;
        public double SizeRatio
        {
            get
            {
                return danmakuSize / fontSize;
            }
            set
            {
                danmakuSize = value * fontSize;
                ComputeLines();
            }
        }
        private double danmakuSpeed = 0.15;
        public double SpeedRatio
        {
            get
            {
                return danmakuSpeed / speed;
            }
            set
            {
                danmakuSpeed = value * speed;
            }
        }
        //是否显示弹幕
        #region
        /// <summary>
        /// 获取或设置一个值,表示是否显示弹幕
        /// </summary>
        public bool IsShowDanmaku
        {
            get
            {
                return Container.Visibility == Visibility.Visible;
            }
            set
            {
                Container.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private bool isShowScrollableDanmaku;
        /// <summary>
        /// 获取或设置一个值,表示是否显示滚动弹幕
        /// </summary>
        public bool IsShowScrollableDanmaku
        {
            get
            {
                return isShowScrollableDanmaku;
            }
            set
            {
                isShowScrollableDanmaku = value;
                ScrollableDanmakuList.ForEach(a => a.Visibility = value ? Visibility.Visible : Visibility.Collapsed);
            }
        }
        private bool isShowTopDanmaku;
        /// <summary>
        /// 获取或设置一个值,表示是否显示顶部弹幕
        /// </summary>
        public bool IsShowTopDanmaku
        {
            get
            {
                return isShowTopDanmaku;
            }
            set
            {
                isShowTopDanmaku = value;
                TopDanmakuList.ForEach(a => a.Visibility = value ? Visibility.Visible : Visibility.Collapsed);
            }
        }
        private bool isShowBottomDanmaku;
        /// <summary>
        /// 获取或设置一个值,表示是否显示底部弹幕
        /// </summary>
        public bool IsShowBottomDanmaku
        {
            get
            {
                return isShowBottomDanmaku;
            }
            set
            {
                isShowBottomDanmaku = value;
                BottomDanmakuList.ForEach(a => a.Visibility = value ? Visibility.Visible : Visibility.Collapsed);
            }
        }
        #endregion

        /// <summary>
        /// 获取一个值,表示弹幕是否是暂停状态
        /// </summary>
        public bool IsPaused { get; private set; }

        //每条轨道的高度
        public int LineHeight
        {
            get { return (int)(danmakuSize + 10); }
        }

        //弹幕轨道数,在控件改变大小时会重新计算
        int lines;
        //滚动弹幕的轨道是否被占用
        bool[] isOccupied_Scrollable;
        //底部弹幕的轨道是否被占用
        bool[] isOccupied_Bottom;
        //顶部弹幕的轨道是否被占用
        bool[] isOccupied_Top;
        //正在进行的动画
        List<Storyboard> Storyboards = new List<Storyboard>();
        // 已绘制的弹幕列表
        List<Grid> ScrollableDanmakuList = new List<Grid>();
        List<Grid> TopDanmakuList = new List<Grid>();
        List<Grid> BottomDanmakuList = new List<Grid>();
        List<DispatcherTimer> timers = new List<DispatcherTimer>();




        public DanmakuManager()
        {
            this.SizeChanged += ThisControl_SizeChanged;
            this.InitializeComponent();
        }
        /// <summary>
        /// 高度变化重新计算轨道数目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThisControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Height != e.NewSize.Height)
                ComputeLines();
        }
        /// <summary>
        /// 计算轨道数
        /// </summary>
        void ComputeLines()
        {
            int temp = (int)(this.RenderSize.Height / LineHeight);
            if (lines != temp)
            {
                lines = temp;
                isOccupied_Scrollable = new bool[lines];
                isOccupied_Bottom = new bool[lines];
                isOccupied_Top = new bool[lines];
            }
        }
        /// <summary>
        /// 为弹幕获取一条轨道
        /// </summary>
        /// <param name="type">弹幕类型</param>
        /// <returns>轨道编号</returns>
        int GetLine(DanmakuType type)
        {
            switch (type)
            {
                case DanmakuType.Scrollable:
                    for (int i = 0; i < lines; i++)
                    {
                        if (!isOccupied_Scrollable[i])
                        {
                            //没有被占用时占用这条轨道
                            isOccupied_Scrollable[i] = true;
                            return i;
                        }
                    }
                    break;
                case DanmakuType.Bottom:
                    for (int i = 0; i < lines; i++)
                    {
                        if (!isOccupied_Bottom[i])
                        {
                            //没有被占用时占用这条轨道
                            isOccupied_Bottom[i] = true;
                            return i;
                        }
                    }
                    break;
                case DanmakuType.Top:
                    for (int i = 0; i < lines; i++)
                    {
                        if (!isOccupied_Top[i])
                        {
                            //没有被占用时占用这条轨道
                            isOccupied_Top[i] = true;
                            return i;
                        }
                    }
                    break;
            }
            //所有轨道都被占用时不绘制此弹幕
            return -1;
        }
        /// <summary>
        /// 计算发射弹幕的纵坐标
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        double GetY_axis(int line)
        {
            return line * LineHeight;
        }
        /// <summary>
        /// 计算弹幕阴影颜色
        /// </summary>
        /// <param name="color">字体颜色</param>
        /// <returns>阴影颜色</returns>
        Color GetShadowColor(Color color)
        {
            int sum = 0;
            sum += color.R;
            sum += color.G;
            sum += color.B;
            if (sum < 383) return Color.FromArgb(color.A, 255, 255, 255);
            else return Color.FromArgb(color.A, 0, 0, 0);
        }
        /// <summary>
        /// 弹幕类型的枚举值
        /// </summary>
        public enum DanmakuType
        {
            Scrollable = 1,
            Bottom = 4,
            Top = 5
        }
        /// <summary>
        /// 添加弹幕
        /// </summary>
        /// <param name="content">弹幕内容</param>
        /// <param name="foreground">颜色</param>
        /// <param name="type">弹幕类型</param>
        public void AddDanmaku(string content, Color foreground, DanmakuType type,int borderThickness=0)
        {
            switch (type)
            {
                case DanmakuType.Scrollable: AddScrollableDanmaku(content, foreground, borderThickness); break;
                case DanmakuType.Bottom: AddBottomDanmaku(content, foreground, borderThickness); break;
                case DanmakuType.Top: AddTopDanmaku(content, foreground, borderThickness); break;
                default: return;
            }
        }
        /// <summary>
        /// 在屏幕中中添加一条滚动弹幕
        /// </summary>
        /// <param name="content">弹幕内容</param>
        /// <param name="foreground">弹幕颜色</param>
        /// <param name="borderThickness">边框粗细</param>
        void AddScrollableDanmaku(string content, Color foreground, int borderThickness)
        {
            if (!(IsShowScrollableDanmaku && IsShowDanmaku)) return;
            //获取轨道
            int line = GetLine(DanmakuType.Scrollable);
            //没有轨道就不绘制弹幕
            if (line == -1) return;
            //创建弹幕块
            Grid item = new Grid() { Margin = new Thickness(0, GetY_axis(line), 0, 0), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x66)), BorderThickness = new Thickness(borderThickness) };
            TextBlock textBlock = new TextBlock() { Text = content, Foreground = new SolidColorBrush(foreground), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            TextBlock shadow = new TextBlock() { Text = content, Margin = new Thickness(2, 1, 0, 0), Foreground = new SolidColorBrush(GetShadowColor(foreground)), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            item.Children.Add(shadow);
            item.Children.Add(textBlock);
            //将弹幕添加到容器中
            Container.Children.Add(item);
            ScrollableDanmakuList.Add(item);
            //设置动画效果
            var animation = new DoubleAnimation() { From = Container.ActualWidth, To = -(textBlock.FontSize * content.Length), SpeedRatio = danmakuSpeed };
            //动画完成后移除弹幕
            animation.Completed += ((object sender, object e) =>
              {
                  if (Container.Children.Contains(item))
                      Container.Children.Remove(item);
                  if (ScrollableDanmakuList.Contains(item))
                      ScrollableDanmakuList.Remove(item);

              });
            //弹幕移动一定距离后使这条轨道变为空闲
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += ((sener, e) =>
            {
                Point relativePoint = item.TransformToVisual(Container).TransformPoint(new Point(0, 0));
                if (relativePoint.X < Container.ActualWidth - item.DesiredSize.Width - 30)
                {
                    timer.Stop();
                    if (line < lines && line != -1)
                    {
                        isOccupied_Scrollable[line] = false;
                    }
                }
            });
            //设置动画
            TranslateTransform trans = new TranslateTransform();
            item.RenderTransform = trans;
            Storyboard.SetTarget(animation, trans);
            Storyboard.SetTargetProperty(animation, "(TranslateTransform.X)");
            Storyboard sb = new Storyboard();
            Storyboards.Add(sb);
            sb.Children.Add(animation);
            //开始动画
            timer.Start();
            sb.Begin();
            //暂停时自动暂停新的弹幕
            if (IsPaused)
            {
                sb.Pause();
            }
            //播放完成后移除动画
            sb.Completed += (sender, e) =>
            {
                if (Storyboards.Contains(sb))
                {
                    Storyboards.Remove(sb);
                }
            };
        }
        void AddBottomDanmaku(string content, Color foreground, int borderThickness)
        {
            if (!(IsShowBottomDanmaku && IsShowDanmaku)) return;
            //获取轨道
            int line = GetLine(DanmakuType.Bottom);
            //没有轨道就不绘制弹幕
            if (line == -1) return;
            //创建弹幕块
            Grid item = new Grid() { Margin = new Thickness(0, Container.ActualHeight - GetY_axis(line) - LineHeight, 0, 0), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x66)), BorderThickness = new Thickness(borderThickness) };
            TextBlock textBlock = new TextBlock() { Text = content, Foreground = new SolidColorBrush(foreground), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            TextBlock shadow = new TextBlock() { Text = content, Margin = new Thickness(2, 1, 0, 0), Foreground = new SolidColorBrush(GetShadowColor(foreground)), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            item.Children.Add(shadow);
            item.Children.Add(textBlock);
            //将弹幕添加到容器中
            Container.Children.Add(item);
            BottomDanmakuList.Add(item);
            //定时使固定弹幕消失
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            int count = 0;
            timer.Tick += ((sener, e) =>
            {
                if (++count < 10) return;
                if (Container.Children.Contains(item))
                    Container.Children.Remove(item);
                if (BottomDanmakuList.Contains(item))
                    BottomDanmakuList.Remove(item);
                if (timers.Contains(timer))
                    timers.Remove(timer);
                isOccupied_Bottom[line] = false;
                timer.Stop();
            });
            timers.Add(timer);
            timer.Start();
        }
        void AddTopDanmaku(string content, Color foreground, int borderThickness)
        {
            if (!(IsShowTopDanmaku && IsShowDanmaku)) return;
            //获取轨道
            int line = GetLine(DanmakuType.Top);
            //没有轨道就不绘制弹幕
            if (line == -1) return;
            //创建弹幕块
            Grid item = new Grid() { Margin = new Thickness(0, GetY_axis(line), 0, 0), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x66)), BorderThickness = new Thickness(borderThickness) };
            TextBlock textBlock = new TextBlock() { Text = content, Foreground = new SolidColorBrush(foreground), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            TextBlock shadow = new TextBlock() { Text = content, Margin = new Thickness(2, 1, 0, 0), Foreground = new SolidColorBrush(GetShadowColor(foreground)), FontSize = danmakuSize, FontWeight = Windows.UI.Text.FontWeights.Bold };
            item.Children.Add(shadow);
            item.Children.Add(textBlock);
            //将弹幕添加到容器中
            Container.Children.Add(item);
            TopDanmakuList.Add(item);
            //定时使固定弹幕消失
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            int count = 0;
            timer.Tick += ((sener, e) =>
            {
                if (++count < 10) return;
                if (Container.Children.Contains(item))
                    Container.Children.Remove(item);
                if (TopDanmakuList.Contains(item))
                    TopDanmakuList.Remove(item);
                if (timers.Contains(timer))
                    timers.Remove(timer);
                isOccupied_Top[line] = false;
                timer.Stop();
            });
            timers.Add(timer);
            timer.Start();
        }
        /// <summary>
        /// 暂停弹幕
        /// </summary>
        public void Pause()
        {
            Storyboards.ForEach(s => s.Pause());
            timers.ForEach(t => t.Stop());
            IsPaused = true;
        }
        /// <summary>
        /// 使暂停的弹幕恢复
        /// </summary>
        public void Resume()
        {
            Storyboards.ForEach(s => s.Resume());
            timers.ForEach(t => t.Start());
            IsPaused = false;
        }
        /// <summary>
        /// 清空弹幕池
        /// </summary>
        public void Clear()
        {
            timers.ForEach(t => t.Stop());
            timers.Clear();
            Container.Children.Clear();
            ScrollableDanmakuList.Clear();
            BottomDanmakuList.Clear();
            TopDanmakuList.Clear();
            Storyboards.ForEach(s => s.Stop());
            Storyboards.Clear();
            isOccupied_Scrollable = new bool[lines];
            isOccupied_Bottom = new bool[lines];
            isOccupied_Top = new bool[lines];
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="content"></param>
        /// <param name="color"></param>
        /// <param name="position"></param>
        /// <param name="cid"></param>
        /// <param name="mode"></param>
        public void SendDanmaku(string content,Color color,double position,string cid,DanmakuType mode)
        {
            string url = "http://www.tucao.tv/index.php";
            //message=敌台全是圣光&color=16777215&stime=13.413373&addtime=1533297826&token=demo&cid=11-4077158-1-0&mode=1&size=25&user=test&datatype=send&
            var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("message", content),
                        new KeyValuePair<string,string>("color", color.ToInt().ToString()),
                        new KeyValuePair<string,string>("stime", position.ToString()),
                        new KeyValuePair<string,string>("addtime", Methods.GetUnixTimestamp().ToString()),
                        new KeyValuePair<string,string>("token", "demo"),
                        new KeyValuePair<string,string>("cid", cid),
                        new KeyValuePair<string,string>("mode", ((int)mode).ToString()),
                        new KeyValuePair<string,string>("size", "25"),
                        new KeyValuePair<string,string>("user", "test"),
                        new KeyValuePair<string,string>("datatype", "send")
                    };
            //POST http://www.tucao.tv/index.php?m=mukio&c=index&a=post&playerID=11-4077158-1-0 
            Hashtable queries = new Hashtable();
            {
                queries.Add("m", "mukio");
                queries.Add("c", "index");
                queries.Add("a", "post");
                queries.Add("playerID", cid);
            }
            Methods.HttpPostAsync(url, body, queries);
            AddDanmaku(content, color, mode, 2);
        }
    }
}
