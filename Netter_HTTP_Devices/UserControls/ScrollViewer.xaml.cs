using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Netter_HTTP_Devices.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ScrollViewer.xaml
    /// </summary>
    public partial class ScrollViewer : UserControl
    {
        public ScrollViewer()
        {
            InitializeComponent();
        }

        //положение ScrollBar (true - слева, false - справа)
        public bool LeftScrollBar
        {
            get { return (bool)GetValue(LeftScrollBarProperty); }
            set { SetValue(LeftScrollBarProperty, value); }
        }

        public static readonly DependencyProperty LeftScrollBarProperty =
            DependencyProperty.Register("LeftScrollBar", typeof(bool), typeof(ScrollViewer), new PropertyMetadata(false));

        //Label Text
        public string TitleContent
        {
            get { return (string)GetValue(TitleContentProperty); }
            set { SetValue(TitleContentProperty, value); }
        }

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent", typeof(string), typeof(ScrollViewer), new PropertyMetadata(""));

        //Content
        public object MainContent
        {
            get { return (object)GetValue(PathContentProperty); }
            set { SetValue(PathContentProperty, value); }
        }

        public static readonly DependencyProperty PathContentProperty =
            DependencyProperty.Register("MainContent", typeof(object), typeof(ScrollViewer), new PropertyMetadata(""));

        #region Color
        //стартовый цвет для градиента
        public Color First_Color
        {
            get { return (Color)GetValue(First_Color_Property); }
            set { SetValue(First_Color_Property, value); }
        }

        public static readonly DependencyProperty First_Color_Property =
            DependencyProperty.Register("First_Color", typeof(Color), typeof(ScrollViewer), new PropertyMetadata(Color.FromArgb(255, 255, 255, 255)));

        //конечный цвет для градиента
        public Color Second_Color
        {
            get { return (Color)GetValue(Second_Color_Property); }
            set { SetValue(Second_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_Color_Property =
            DependencyProperty.Register("Second_Color", typeof(Color), typeof(ScrollViewer), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0)));
        #endregion

        //перевод scrollbar в нижнее положение 
        public void ScrollToEnd()
        {
            if (MainScrollViewer.VerticalOffset == MainScrollViewer.ScrollableHeight)
                MainScrollViewer.ScrollToEnd();
        }
    }
}
