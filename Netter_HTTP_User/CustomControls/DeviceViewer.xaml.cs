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
    /// Логика взаимодействия для DeviceViewer.xaml
    /// </summary>
    public partial class DeviceViewer : UserControl
    {
        public DeviceViewer()
        {
            InitializeComponent();
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }

        #region DependencyProperty

        //Label Text
        public string FirstContent
        {
            get { return (string)GetValue(FirstContentProperty); }
            set { SetValue(FirstContentProperty, value); }
        }

        public static readonly DependencyProperty FirstContentProperty =
            DependencyProperty.Register("FirstContent", typeof(string), typeof(DeviceViewer), new PropertyMetadata(""));
        public string SecondContent
        {
            get { return (string)GetValue(SecondContentProperty); }
            set { SetValue(SecondContentProperty, value); }
        }

        public static readonly DependencyProperty SecondContentProperty =
            DependencyProperty.Register("SecondContent", typeof(string), typeof(DeviceViewer), new PropertyMetadata(""));
        public string ThirdContent
        {
            get { return (string)GetValue(ThirdContentProperty); }
            set { SetValue(ThirdContentProperty, value); }
        }

        public static readonly DependencyProperty ThirdContentProperty =
            DependencyProperty.Register("ThirdContent", typeof(string), typeof(DeviceViewer), new PropertyMetadata(""));
        #endregion


        #region Click
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DeviceViewer.ClickEvent);
            RaiseEvent(newEventArgs);
        }
        private void OnClick()
        {
            RaiseClickEvent();
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DeviceViewer));
        #endregion
    }
}
