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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Netter_HTTP_Server.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для GroupBox.xaml
    /// </summary>
    public partial class GroupBox : UserControl
    {
        public GroupBox()
        {
            InitializeComponent();
        }

        //
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(GroupBox), new PropertyMetadata(true));

        //Radius
        public int Radius
        {
            get { return (int)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(int), typeof(GroupBox), new PropertyMetadata(0));

        //Label Text
        public string TitleContent
        {
            get { return (string)GetValue(TitleContentProperty); }
            set { SetValue(TitleContentProperty, value); }
        }

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent", typeof(string), typeof(GroupBox), new PropertyMetadata(null));

        //Content
        public object MainContent
        {
            get { return (object)GetValue(PathContentProperty); }
            set { SetValue(PathContentProperty, value); }
        }

        public static readonly DependencyProperty PathContentProperty =
            DependencyProperty.Register("MainContent", typeof(object), typeof(GroupBox), new PropertyMetadata(""));

        #region Color
        //стартовый цвет для градиента
        public Color First_Color
        {
            get { return (Color)GetValue(First_Color_Property); }
            set { SetValue(First_Color_Property, value); }
        }

        public static readonly DependencyProperty First_Color_Property =
            DependencyProperty.Register("First_Color", typeof(Color), typeof(GroupBox), new PropertyMetadata(Color.FromArgb(255, 255, 255, 255)));

        //конечный цвет для градиента
        public Color Second_Color
        {
            get { return (Color)GetValue(Second_Color_Property); }
            set { SetValue(Second_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_Color_Property =
            DependencyProperty.Register("Second_Color", typeof(Color), typeof(GroupBox), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0)));
        #endregion

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsOpen ^= true;
        }
    }
}
