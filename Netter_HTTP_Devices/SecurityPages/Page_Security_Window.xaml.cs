using System;
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

namespace Netter_HTTP_Devices
{
    /// <summary>
    /// Логика взаимодействия для Page_Security_Window.xaml
    /// </summary>
    public partial class Page_Security_Window : Page
    {
        public Page_Security_Window()
        {
            InitializeComponent();

            //загрузка comboBox
            ((ScrollViewer_DeviceInfo.MainContent as Grid).Children[5] as ComboBox).ItemsSource = Properties.Settings.Default.Window_Position;
        }

        //создание класса для хранения информации об устройстве
        //привязка полей к переменным
        public object InfoCreate(int groupID, int typeID)
        {
            Class_Device_Security_Window deviceInfo = new Class_Device_Security_Window(groupID, typeID);
            DataContext = deviceInfo;
            return deviceInfo;
        }

        #region TextBox
        //проверка вводимого текста
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthLong) //ограничение по длине
            {
                for (int i = 0; i < e.Text.Length && e.Handled != true; i++)
                    if (e.Text[i] == 43 || e.Text[i] == 47 || e.Text[i] == 61 || (e.Text[i] >= 'а' && e.Text[i] <= 'я') || (e.Text[i] >= 'А' && e.Text[i] <= 'Я')) //запрет на ввод символов + = /
                        e.Handled = true;
            }
            else e.Handled = true;
        }
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthShot) //ограничение по длине
            {
                for (int i = 0; i < e.Text.Length && e.Handled != true; i++)
                    if (e.Text[i] < 48 || e.Text[i] > 57) //запрет символов, отличных от цифр
                        e.Handled = true;
            }
            else e.Handled = true;
        }
        #endregion
    }
}
