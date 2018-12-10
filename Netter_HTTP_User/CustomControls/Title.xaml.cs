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

namespace Netter_HTTP_User.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для Title.xaml
    /// </summary>
    public partial class Title : UserControl
    {
        public Title()
        {
            InitializeComponent();
        }

        #region Label
        //Заголовок программы
        public string MainTitle
        {
            get { return (string)GetValue(MainTitleProperty); }
            set { SetValue(MainTitleProperty, value); }
        }

        public static readonly DependencyProperty MainTitleProperty =
            DependencyProperty.Register("MainTitle", typeof(string), typeof(Title), new PropertyMetadata(""));
        //Группа устройств
        public string GroupTitle
        {
            get { return (string)GetValue(GroupTitleProperty); }
            set { SetValue(GroupTitleProperty, value); }
        }

        public static readonly DependencyProperty GroupTitleProperty =
            DependencyProperty.Register("GroupTitle", typeof(string), typeof(Title), new PropertyMetadata(""));
        //Тип устройств
        public string TypeTitle
        {
            get { return (string)GetValue(TypeTitleProperty); }
            set { SetValue(TypeTitleProperty, value); }
        }

        public static readonly DependencyProperty TypeTitleProperty =
            DependencyProperty.Register("TypeTitle", typeof(string), typeof(Title), new PropertyMetadata(""));
        //ID устройтсва
        public string IDTitle
        {
            get { return (string)GetValue(IDTitleProperty); }
            set { SetValue(IDTitleProperty, value); }
        }

        public static readonly DependencyProperty IDTitleProperty =
            DependencyProperty.Register("IDTitle", typeof(string), typeof(Title), new PropertyMetadata(""));
        #endregion

        #region Color
        //стартовый цвет для градиента
        public Color First_Color
        {
            get { return (Color)GetValue(First_Color_Property); }
            set { SetValue(First_Color_Property, value); }
        }

        public static readonly DependencyProperty First_Color_Property =
            DependencyProperty.Register("First_Color", typeof(Color), typeof(Title), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Second_Color
        {
            get { return (Color)GetValue(Second_Color_Property); }
            set { SetValue(Second_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_Color_Property =
            DependencyProperty.Register("Second_Color", typeof(Color), typeof(Title), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));
        //стартовый цвет для градиента
        public Color Third_Color
        {
            get { return (Color)GetValue(Third_Color_Property); }
            set { SetValue(Third_Color_Property, value); }
        }

        public static readonly DependencyProperty Third_Color_Property =
            DependencyProperty.Register("Third_Color", typeof(Color), typeof(Title), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Fourth_Color
        {
            get { return (Color)GetValue(Fourth_Color_Property); }
            set { SetValue(Fourth_Color_Property, value); }
        }

        public static readonly DependencyProperty Fourth_Color_Property =
            DependencyProperty.Register("Fourth_Color", typeof(Color), typeof(Title), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));
        #endregion
    }
}
