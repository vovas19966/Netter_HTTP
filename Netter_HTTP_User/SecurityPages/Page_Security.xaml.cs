using Netter_HTTP_User.CustomControls;
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

namespace Netter_HTTP_User
{
    /// <summary>
    /// Логика взаимодействия для Page_Security.xaml
    /// </summary>
    public partial class Page_Security : Page
    {
        private int GroupIndex = 0; //номер группы
        private int TypeIndex = 0; //номер типа
        private int DeviceNumber = 0; //кол-во устройств, подключенных к серверу
        private Color ThisColor;

        private readonly Action<int, string> MessageOutput; //метод для вывод сообщений

        public Page_Security(Action<int, string> messageOutput)
        {
            InitializeComponent();

            this.ThisColor = (Color)this.FindResource("Blue_First_Color");

            this.MessageOutput = messageOutput;

            MenuReset();

            for (int i = 0; i < (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
            {
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.TypeTitles_of_SecurityGroup[i].ToUpper();
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).Click += Button_Click;
            }
        }


        #region Button CLick
        //нажатие на кнопку (выбор группы устройств)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DeviceNumber = 0;

            //если выбранная кнопка активна
            if ((sender as MenuButton).IsActive)
            {
                //изменение статуса выбранной кнопки
                (sender as MenuButton).IsActive ^= true;
                //обнуление frame
                Frame_DeviceList.Navigate(null);
            }
            else
            {
                //сброс меню
                MenuReset();

                ConnectionCheck();

                //активация выбранной кнопки
                (sender as MenuButton).IsActive = true;
            }
        }


        private void Button_Door_Click(object sender, RoutedEventArgs e)
        {
            this.TypeIndex = 0;
        }
        private void Device_Door_Click(CustomControls.DeviceViewer device)
        {
            MainWindow._MainFrame.Navigate(new Page_Security_Door(device.FirstContent, this.MessageOutput));
        }

        private void Button_Window_Click(object sender, RoutedEventArgs e)
        {
            this.TypeIndex = 1;
        }
        private void Device_Window_Click(CustomControls.DeviceViewer device)
        {
            MainWindow._MainFrame.Navigate(new Page_Security_Window(device.FirstContent, this.MessageOutput));
        }

        private void Button_Floor_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion


        #region Connection Checked
        //проверка соединения (GET)
        private void ConnectionCheck()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.ConnectionCheckRequest + GetNowTime);
            MainWindow.HTTPClient.GetRequest(0, "", ConnectionCheck_ResponseHandler);
        }

        private bool ConnectionCheck_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.ConnectionCheckRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                DeviceNumberConnection();
            }
            else
            {
                //сброс меню
                MenuReset();
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region DeviceList Connection
        //запрос кол-ва подключенных к серверу устройств
        private void DeviceNumberConnection()
        {
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.DeviceListRequest + GetNowTime);

            string rawURI = (MainWindow.IsAdmin ? Properties.Settings.Default.RawURI_Admin : "")
                + Properties.Settings.Default.GroupIDs[this.GroupIndex].ToString("D2") 
                + "/" + Properties.Settings.Default.TypeIDs_of_SecurityGroup[this.TypeIndex].ToString("D2") + "/";

            MainWindow.HTTPClient.GetRequest(0, rawURI, DeviceNumberConnection_ResponseHandler);
        }

        private bool DeviceNumberConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.DeviceListRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                this.DeviceNumber = Convert.ToInt32(response);
                Frame_DeviceList.Navigate(TypeMenuOpen());
            }
            else
            {
                //сброс меню
                MenuReset();
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region Управление Frame_Type
        //сброс меню
        private void MenuReset()
        {
            //обнуление нажатых кнопок
            foreach (object button in (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children)
                (button as MenuButton).IsActive = false;
        }

        //выбор и отобращение Page типов для соответствующей группы
        private Page TypeMenuOpen()
        {
            Action<CustomControls.DeviceViewer> click = null;
            if (this.TypeIndex == 0) click = Device_Door_Click;
            else if (this.TypeIndex == 1) click = Device_Window_Click;

            return new PageDeviceList(this.GroupIndex, this.TypeIndex, this.DeviceNumber, click, this.MessageOutput, this.ThisColor);
        }

        //Изменение frame
        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Frame frame = (sender as Frame);

            if (frame.NavigationService.RemoveBackEntry() != null)
            {
                //frame.RemoveBackEntry();
                frame.NavigationService.RemoveBackEntry();
            }
        }
        #endregion
        
        //возвращение текущего времени
        private string GetNowTime
        {
            get { return ("Время: " + DateTime.Now.ToLongTimeString()); }
        }
    }
}
