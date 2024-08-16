using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace YinTaohua
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow I { get; private set; }
        public MainWindow()
        {
            I = this;
            SteamUserStats.RequestCurrentStats();

            bool night = false;
            bool hsc = true;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings["night"] == "1") night = true;
                if (appSettings["hallu"] == "0") hsc = false;
            }
            catch (ConfigurationErrorsException)
            {
            }

            InitializeComponent();
            {
                SteamUserStats.GetStat("sp214", out int stat);
                if (stat == 1 || DateTime.Now.Month == 2 && DateTime.Now.Day == 14)
                {
                    sp214.Visibility = Visibility.Visible;
                    if (stat == 0)
                    {
                        SteamUserStats.SetStat("sp214", 1);
                        SteamUserStats.StoreStats();
                    }
                }
                if (DateTime.Now.Month == 4 && ((DateTime.Now.Year <= 2022 || DateTime.Now.Year > 2023) && DateTime.Now.Day >=4 && DateTime.Now.Day <= 6 || DateTime.Now.Year == 2023 && DateTime.Now.Day == 5)) sp4.Visibility = Visibility.Visible;
            }

            if (night)
            {
                this.night.IsChecked = true;
                night_Click(null, null);
            }
            if (hsc) halluSwitch.IsChecked = true;

            autoScrollAnimation.Tick += AutoScrollAnimation_Tick;
            halluTimer.Tick += HalluTimer_Tick;
            Story.Doc = doc.Blocks;
            Story.End1 = end1;
            Story.End2 = end2;
            Story.MainParagraph = main.Inlines;
            Story.ChoiceMenu = choiceMenu.Inlines;
            Story.ParseFrom(new StreamReader(Application.GetResourceStream(new Uri("/YinTaohua.ink", UriKind.Relative)).Stream));
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            Story.Restart();
        }

        Point mouseDownPoint;
        private void doc_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPoint = Mouse.GetPosition(doc);
        }

        ScrollViewer _scrollViewer;
        ScrollViewer ScrollViewer
        {
            get
            {
                if (_scrollViewer == null) _scrollViewer = GetScrollViewer(docViewer) as ScrollViewer;
                return _scrollViewer;
            }
        }
        DispatcherTimer autoScrollAnimation = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(33) };
        double endScroll;
        public bool BottomScrolled => ScrollViewer.VerticalOffset >= ScrollViewer.ScrollableHeight - 20d;
        public void BeginAutoScroll()
        {
            autoScrollAnimation.Stop();
            var beginScroll = ScrollViewer.VerticalOffset;
            endScroll = ScrollViewer.ExtentHeight;
            if (endScroll - beginScroll > ScrollViewer.ActualHeight - 20d) endScroll = beginScroll + ScrollViewer.ActualHeight - 20d;
            autoScrollAnimation.Start();
        }
        private void AutoScrollAnimation_Tick(object sender, EventArgs e)
        {
            var offset = ScrollViewer.VerticalOffset + 20d;
            if (offset > endScroll)
            {
                offset = endScroll;
                autoScrollAnimation.Stop();
            }
            ScrollViewer.ScrollToVerticalOffset(offset);
        }
        private void docViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mouseUpPoint = Mouse.GetPosition(doc);
            if ((mouseUpPoint - mouseDownPoint).Length < 20) Story.Doc_Click(null, null);
            else autoScrollAnimation.Stop();
        }

        private void FlowDocumentScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            autoScrollAnimation.Stop();
            if (((FlowDocumentScrollViewer)sender).ActualWidth > 500)
            {
                var padding = (((FlowDocumentScrollViewer)sender).ActualWidth - 500) / 2;
                ((FlowDocumentScrollViewer)sender).Document.PagePadding = new Thickness(padding, 0, padding, 0);
            }
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }

        private void FlowDocumentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            DependencyObject scrollHost = sender as DependencyObject;
            var scrollViewer = GetScrollViewer(scrollHost) as ScrollViewer;
            autoScrollAnimation.Stop();
            if (scrollViewer != null)
            {
                double offset = scrollViewer.VerticalOffset - (e.Delta * 5d / 6);
                if (offset < 0)
                {
                    scrollViewer.ScrollToVerticalOffset(0);
                }
                else if (offset > scrollViewer.ScrollableHeight)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(offset);
                }

                e.Handled = true;
            }
        }

        UIElement lastUi;
        private void mainmenu_Click(object sender, RoutedEventArgs e)
        {
            EndHallu();
            lastUi.Visibility = Visibility.Collapsed;
            cover.Visibility = Visibility.Visible;
        }

        private void game_Click(object sender, RoutedEventArgs e)
        {
            game.Visibility = Visibility.Visible;
            cover.Visibility = Visibility.Collapsed;
            lastUi = game;
            Story.Restart();
        }

        private void sp214_Click(object sender, RoutedEventArgs e)
        {
            if (SteamUserStats.GetAchievement("te", out bool a) && a) new Sp214Window().Show();
            else MessageBox.Show("通关某个结局后解锁");
        }

        private void sp4_Click(object sender, RoutedEventArgs e)
        {
            if (SteamUserStats.GetAchievement("be", out bool a) && a) new Sp4Window().Show();
            else MessageBox.Show("通关某个结局后解锁");
        }

        private void snowy_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://snowyyang.com");
        }

        private void night_Click(object sender, RoutedEventArgs e)
        {
            if (night.IsChecked == true)
            {
                Foreground = Brushes.White;
                Background = Brushes.Black;
            }
            else
            {
                Foreground = Brushes.Black;
                Background = Brushes.White;
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                var key = "night";
                var value = night.IsChecked == true ? "1" : "0";
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                key = "hallu";
                value = halluSwitch.IsChecked == true ? "1" : "0";
                if (settings[key] == null) settings.Add(key, value);
                else settings[key].Value = value;
                configFile.Save(ConfigurationSaveMode.Modified);
                //ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
            base.OnClosing(e);
        }

        DispatcherTimer halluTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(33) };
        Random random = new Random();
        public void BeginHallu()
        {
            halluSwitch.Visibility = Visibility.Visible;
            if (halluSwitch.IsChecked == true)
            {
                halluTimer.Start();
                hallu.Visibility = Visibility.Visible;
            }
        }
        public void EndHallu()
        {
            halluTimer.Stop();
            halluSwitch.Visibility = hallu.Visibility = Visibility.Collapsed;
            hallu.Children.Clear();
        }

        private void hallu_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                hallu.Visibility = Visibility.Visible;
                halluTimer.Start();
            }
            else
            {
                hallu.Visibility = Visibility.Collapsed;
                halluTimer.Stop();
            }
        }

        private void hallu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var m = hallu.Children.Count * (e.PreviousSize.Height + 100d) * (e.PreviousSize.Width + 100d) / (e.NewSize.Height + 100d) / (e.NewSize.Width + 100d);
            if (m < hallu.Children.Count)
            {
                while (hallu.Children.Count >= m) hallu.Children.RemoveAt(random.Next(hallu.Children.Count));
                foreach (TextBlock t in hallu.Children)
                {
                    Canvas.SetLeft(t, random.NextDouble()*(e.NewSize.Width + 100d) - 50d);
                    Canvas.SetTop(t, random.NextDouble()*(e.NewSize.Height+100d) - 50d);
                }
            }
            else if (m > hallu.Children.Count)
            {
                var s = "幻听";
                var htn = random.Next(2);
                for(int ii = 0; ii < htn; ii++)
                {
                    var hn = random.Next(2)+1;
                    for (int j = 0; j < hn; j++) s += "幻";
                    var tn = random.Next(2)+1;
                    for (int j = 0; j < tn; j++) s += "听";
                }
                var i = 0;
                while (i < random.Next(5) && hallu.Children.Count < m) hallu.Children.Add(new TextBlock() { Text = s, Opacity = random.NextDouble() });
                foreach (TextBlock t in hallu.Children)
                {
                    Canvas.SetLeft(t, random.NextDouble()*(e.NewSize.Width + 100d) - 50d);
                    Canvas.SetTop(t, random.NextDouble()*(e.NewSize.Height+100d) - 50d);
                }
            }
        }

        private void HalluTimer_Tick(object sender, EventArgs e)
        {
            if (random.NextDouble() > hallu.Children.Count / (hallu.ActualHeight + 100) / (hallu.ActualWidth + 100) * 3000)
            {
                var s = "幻听";
                var htn = random.Next(2);
                for (int ii = 0; ii < htn; ii++)
                {
                    var hn = random.Next(2) + 1;
                    for (int j = 0; j < hn; j++) s += "幻";
                    var tn = random.Next(2) + 1;
                    for (int j = 0; j < tn; j++) s += "听";
                }
                var t = new TextBlock() { Text = s, Opacity = random.NextDouble() };
                hallu.Children.Add(t);
                Canvas.SetLeft(t, random.NextDouble() * (hallu.ActualWidth + 100d) - 50d);
                Canvas.SetTop(t, random.NextDouble() * (hallu.ActualHeight + 100d) - 50d);
            }
            foreach (TextBlock t in hallu.Children)
            {
                var d = random.NextDouble();
                if (d > 0.97)
                    if (d > 0.985)
                    {
                        if (t.Text.Length < 6) t.Text += "幻";
                    }
                    else
                    {
                        if (t.Text.Length < 6) t.Text += "听";
                    }
                else if (d < 0.03)
                    if (d < 0.015)
                    {
                        if (t.Text.Length > 1) t.Text = t.Text.Substring(1);
                    }
                    else if (t.Text.Length > 1) t.Text = t.Text.Substring(0, t.Text.Length - 1);
                d = random.NextDouble();
                if (d > 0.97) Canvas.SetLeft(t, Canvas.GetLeft(t) + (d - 0.7) * 20);
                else if (d < 0.03) Canvas.SetLeft(t, Canvas.GetLeft(t) + (d - 0.3) * 20);
                if (d > 0.97) Canvas.SetTop(t, Canvas.GetTop(t) + (d - 0.7) * 20);
                else if (d < 0.03) Canvas.SetTop(t, Canvas.GetTop(t) + (d - 0.3) * 20);
                d = random.NextDouble();
                if (d > 0.8) t.Opacity = t.Opacity * Math.Pow((d + 0.5), 0.25);
            }
        }

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                sp214.Visibility = Visibility.Visible;
                sp4.Visibility = Visibility.Visible;
            }
        }

        private void meng_Click(object sender, RoutedEventArgs e)
        {
            new MengTaohua().Show();
        }
        private void yan_Click(object sender, RoutedEventArgs e)
        {
            if (SteamUserStats.GetAchievement("persuade", out bool a) && a) new YanTaohua().Show();
            else MessageBox.Show("通关某个结局后解锁");
        }
    }
}
