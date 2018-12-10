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
    /// Логика взаимодействия для PageDeviceList.xaml
    /// </summary>
    public partial class PageDeviceList : Page
    {
        private List<object> DeviceList; //список устройств
        private int GroupIndex = 0; //номер группы
        private int TypeIndex = 0; //номер типа
        private int DeviceNumber = 0; //кол-во устройств, подключенных к серверу
        private int DeviceNum = 0; //номер запрошиваемого устройтсва
        private Action<CustomControls.DeviceViewer> DevicePage_Open;
        private readonly Action<int, string> MessageOutput; //метод для вывод сообщений

        public PageDeviceList(int groupIndex, int typeIndex, int deviceNumber //номера группы и типа, кол-во устройств
            , Action<CustomControls.DeviceViewer> devicePage_Open //метод вызова окна устройства
            , Action<int, string> messageOutput //метод вывода сообщений
            , Color borderColor //цвет границы поля
            )
        {
            InitializeComponent();
            ScrollViewer_DeviceList.Second_Color = borderColor;

            this.DeviceList = new List<object>();

            this.GroupIndex = groupIndex;
            this.TypeIndex = typeIndex;
            this.DeviceNumber = deviceNumber;
            this.DeviceNum = 0;

            this.DevicePage_Open = devicePage_Open;
            this.MessageOutput = messageOutput;

            if (this.DeviceNumber > 0)
                DeviceListConnection();

            DataContext = this.DeviceList;
        }

        #region DeviceList Connection
        //запрос списка устройств (по штуке)
        private void DeviceListConnection()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.MainDeviceInfoRequest + GetNowTime);
            string rawURI = (MainWindow.IsAdmin ? Properties.Settings.Default.RawURI_Admin : "")
                + Properties.Settings.Default.GroupIDs[this.GroupIndex].ToString("D2")
                + "/" + Properties.Settings.Default.TypeIDs_of_SecurityGroup[this.TypeIndex].ToString("D2")
                + "/" + "pg=" + (++this.DeviceNum);

            MainWindow.HTTPClient.GetRequest(0, rawURI, DeviceListConnection_ResponseHandler);
        }

        private bool DeviceListConnection_ResponseHandler(bool code, string response)
        {
            //вывод сообщения
            MessageOutput(1, Properties.Resources.MainDeviceInfoRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                AddDevice(response);
                //запрос следующего устройства
                if (this.DeviceNum < this.DeviceNumber) DeviceListConnection();
            }
            else
            {
                //ONOFF_VisualDisplay(false); //отключение соединения
            }
            return true;
        }
        #endregion


        //добавление устройства к списку устройств
        public void AddDevice(string device)
        {
            this.DeviceList.Add(DeviceInfoDecoder(device));
            DataContext = this.DeviceList;
            DeviceListUpdate();
        }
        
        //обновление списка устройств (вывод на экран)
        private void DeviceListUpdate()
        {
            (ScrollViewer_DeviceList.MainContent as ItemsControl).Items.Refresh();
        }        

        //расшифровка ID устройства из строки RawURL
        private object DeviceInfoDecoder(string str)
        {
            string[] result = new string[3];

            //удаление лишних символов
            while (str[0] == '&')
                str = str.Remove(0, 1);
            while (str[str.Length - 1] == '&')
                str = str.Remove(str.Length - 1, 1);

            //ID: 0-group, 1-type, 2-device
            string[] options = str.Split('&');
            //если в строке более трех сегментов, разделенных /
            //получение id
            try
            {
                for (int i=0; i< options.Length; i++)
                { 
                    string[] keyValue = options[i].Split('=');
                    result[i] = keyValue[1];
                }
                return result;
            }
            catch { return null; } //если возникли проблемы при разшифровке
        }

        //открытие панели с описанием устройства
        private void Device_Click(object sender, RoutedEventArgs e)
        {
            this.DevicePage_Open(sender as CustomControls.DeviceViewer);
        }

        //возвращение текущего времени
        private string GetNowTime
        {
            get { return ("Время: " + DateTime.Now.ToLongTimeString()); }
        }
    }
}
