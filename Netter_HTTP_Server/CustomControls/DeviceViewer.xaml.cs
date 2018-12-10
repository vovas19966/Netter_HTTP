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
    /// Логика взаимодействия для DeviceViewer.xaml
    /// </summary>
    public partial class DeviceViewer : UserControl
    {
        public DeviceViewer()
        {
            InitializeComponent();
        }

        #region DependencyProperty
        //информация об устройстве
        public List<KeyValue> Options
        {
            get { return (List< KeyValue>)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register("Options", typeof(List<KeyValue>), typeof(DeviceViewer), new PropertyMetadata(null));

        //Label Text
        public string IDContent
        {
            get { return (string)GetValue(IDContentProperty); }
            set { SetValue(IDContentProperty, value); }
        }

        public static readonly DependencyProperty IDContentProperty =
            DependencyProperty.Register("IDContent", typeof(string), typeof(DeviceViewer), new PropertyMetadata(null));

        public string TimeContent
        {
            get { return (string)GetValue(TimeContentProperty); }
            set { SetValue(TimeContentProperty, value); }
        }

        public static readonly DependencyProperty TimeContentProperty =
            DependencyProperty.Register("TimeContent", typeof(string), typeof(DeviceViewer), new PropertyMetadata(""));

        public TimeSpan DeleteTimerTicks
        {
            get { return (TimeSpan)GetValue(DeleteTimerTicksProperty); }
            set { SetValue(DeleteTimerTicksProperty, value); }
        }

        public static readonly DependencyProperty DeleteTimerTicksProperty =
            DependencyProperty.Register("DeleteTimerTicks", typeof(TimeSpan), typeof(DeviceViewer), new PropertyMetadata(new TimeSpan(0)));
        #endregion


        public void DataUpdate()
        {
            ItemsControl_DeviceList.Items.Refresh();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataUpdate();
        }
    }
}
