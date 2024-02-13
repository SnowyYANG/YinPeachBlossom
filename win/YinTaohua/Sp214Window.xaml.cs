using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YinTaohua
{
    /// <summary>
    /// Sp214Window.xaml 的交互逻辑
    /// </summary>
    public partial class Sp214Window : Window
    {
        public Sp214Window()
        {
            InitializeComponent();
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var section = (Section)Resources["v" + ((Hyperlink)sender).Tag];
            doc.Blocks.Remove(doc.Blocks.FirstBlock);
            doc.Blocks.InsertBefore(back, section);
            menu.Visibility = Visibility.Collapsed;
            docViewer.Visibility = Visibility.Visible;
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            menu.Visibility = Visibility.Visible;
            docViewer.Visibility = Visibility.Collapsed;
        }
    }
}
