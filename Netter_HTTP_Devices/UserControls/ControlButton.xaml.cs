using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Netter_HTTP_Devices.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ControlButton.xaml
    /// </summary>
    public partial class ControlButton : UserControl
    {
        public ControlButton()
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
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ControlButton), new PropertyMetadata(false));
        
        //Content
        public object PathContent
        {
            get { return (object)GetValue(PathContentProperty); }
            set { SetValue(PathContentProperty, value); }
        }

        public static readonly DependencyProperty PathContentProperty =
            DependencyProperty.Register("PathContent", typeof(object), typeof(ControlButton), new PropertyMetadata(""));
        
        #region Color
        //стартовый цвет для градиента
        public Color First_Active_Color
        {
            get { return (Color)GetValue(First_Active_Color_Property); }
            set { SetValue(First_Active_Color_Property, value); }
        }

        public static readonly DependencyProperty First_Active_Color_Property =
            DependencyProperty.Register("First_Active_Color", typeof(Color), typeof(ControlButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Second_Active_Color
        {
            get { return (Color)GetValue(Second_Active_Color_Property); }
            set { SetValue(Second_Active_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_Active_Color_Property =
            DependencyProperty.Register("Second_Active_Color", typeof(Color), typeof(ControlButton), new PropertyMetadata(Color.FromArgb(255,225,225,225)));
        //стартовый цвет для градиента
        public Color First_InActive_Color
        {
            get { return (Color)GetValue(First_InActive_Color_Property); }
            set { SetValue(First_InActive_Color_Property, value); }
        }

        public static readonly DependencyProperty First_InActive_Color_Property =
            DependencyProperty.Register("First_InActive_Color", typeof(Color), typeof(ControlButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));

        //конечный цвет для градиента
        public Color Second_InActive_Color
        {
            get { return (Color)GetValue(Second_InActive_Color_Property); }
            set { SetValue(Second_InActive_Color_Property, value); }
        }

        public static readonly DependencyProperty Second_InActive_Color_Property =
            DependencyProperty.Register("Second_InActive_Color", typeof(Color), typeof(ControlButton), new PropertyMetadata(Color.FromArgb(255, 225, 225, 225)));
        #endregion

        #region Click
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ControlButton.ClickEvent);
            RaiseEvent(newEventArgs);
        }
        private void OnClick()
        {
            RaiseClickEvent();
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlButton));
        #endregion
    }
}
