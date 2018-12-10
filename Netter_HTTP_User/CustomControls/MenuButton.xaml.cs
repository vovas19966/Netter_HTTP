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
    /// Логика взаимодействия для MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {
        public MenuButton()
        {
            InitializeComponent();
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }


        //переключатель кнопки (true - нажата)
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(MenuButton), new PropertyMetadata(false));

        //Label Text
        public string TitleContent
        {
            get { return (string)GetValue(TitleContentProperty); }
            set { SetValue(TitleContentProperty, value); }
        }

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent", typeof(string), typeof(MenuButton), new PropertyMetadata(""));

        //Content
        public object PathContent
        {
            get { return (object)GetValue(PathContentProperty); }
            set { SetValue(PathContentProperty, value); }
        }

        public static readonly DependencyProperty PathContentProperty =
            DependencyProperty.Register("PathContent", typeof(object), typeof(MenuButton), new PropertyMetadata(""));

        #region Color
        //стартовый цвет для градиента
        public Color First_Active_Color
        {
            get { return (Color)GetValue(First_Active_Color_Property); }
            set { SetValue(First_Active_Color_Property, value); }
        }

        public static readonly DependencyProperty First_Active_Color_Property =
            DependencyProperty.Register("First_Active_Color", typeof(Color), typeof(MenuButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Second_Active_Color
        {
            get { return (Color)GetValue(Second_Active_Color_Property); }
            set { SetValue(Second_Active_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_Active_Color_Property =
            DependencyProperty.Register("Second_Active_Color", typeof(Color), typeof(MenuButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));
        //стартовый цвет для градиента
        public Color First_InActive_Color
        {
            get { return (Color)GetValue(First_InActive_Color_Property); }
            set { SetValue(First_InActive_Color_Property, value); }
        }

        public static readonly DependencyProperty First_InActive_Color_Property =
            DependencyProperty.Register("First_InActive_Color", typeof(Color), typeof(MenuButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Second_InActive_Color
        {
            get { return (Color)GetValue(Second_InActive_Color_Property); }
            set { SetValue(Second_InActive_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_InActive_Color_Property =
            DependencyProperty.Register("Second_InActive_Color", typeof(Color), typeof(MenuButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));
        #endregion

        #region Click
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(MenuButton.ClickEvent);
            RaiseEvent(newEventArgs);
        }
        private void OnClick()
        {
            RaiseClickEvent();
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuButton));
        #endregion
    }
}
