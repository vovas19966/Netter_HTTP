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
    /// Логика взаимодействия для Page_Lighting_Lighting.xaml
    /// </summary>
    public partial class Page_Lighting_Lighting : Page
    {
        private int GroupIndex = 2; //номер группы
        private int TypeIndex = 0; //номер типа
        private string ID; //id устройства

        private List<KeyValuePair<string, string>> LastUpdateOptions; //список параметров устройства (полученных с сервера|измененных)

        private readonly Action<int, string> MessageOutput; //метод для вывод сообщений

        public Page_Lighting_Lighting(string id, Action<int, string> messageOutput)
        {
            InitializeComponent();

            if (MainWindow.IsAdmin)
            {
                for (int i = 6; i < (ScrollViewer_Options.MainContent as Grid).Children.Count; i++)
                {
                    (ScrollViewer_Options.MainContent as Grid).Children[i].Visibility = Visibility.Visible;
                }
            }

            this.MessageOutput = messageOutput;

            this.ID = id.Replace('.', '/');

            //проверка соединения с сервером
            ConnectionCheck();

            ScrollViewer_Options.TitleContent = Properties.Settings.Default.GroupTitles[this.GroupIndex] + ". " + Properties.Settings.Default.TypeTitles_of_LightingGroup[this.TypeIndex];
        }

        #region Button
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender as CustomControls.ControlButton).IsActive)
            {
                (sender as CustomControls.ControlButton).IsActive = true;
                SetOptionsConnection();
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender as CustomControls.ControlButton).IsActive)
            {
                (sender as CustomControls.ControlButton).IsActive = true;
                DeviceOffConnection();
            }
        }
        #endregion

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
        private void URI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthShot) //ограничение по длине
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
                DeviceOptionsConnection();
            }
            else
            {
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region DeviceOptions Connection
        //запрос кол-ва подключенных к серверу устройств
        private void DeviceOptionsConnection()
        {
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.MainDeviceInfoRequest + GetNowTime);

            string rawURI = (MainWindow.IsAdmin ? Properties.Settings.Default.RawURI_Admin : "")
                + this.ID;

            MainWindow.HTTPClient.GetRequest(0, rawURI, DeviceOptionsConnection_ResponseHandler);
        }

        private bool DeviceOptionsConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.MainDeviceInfoRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                OptionDecoder(response);
            }
            else
            {
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region SetOptions Connection
        //запрос кол-ва подключенных к серверу устройств
        private void SetOptionsConnection()
        {
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.SaveOptions + GetNowTime);

            string rawURI = (MainWindow.IsAdmin ? Properties.Settings.Default.RawURI_Admin : "")
                + this.ID;

            List<KeyValuePair<string, string>> options = GetOptions;

            //если параметры введены корректно
            if (options != null)
                MainWindow.HTTPClient.PostRequest(0, rawURI, options, SetOptionsConnection_ResponseHandler);
            else
            {
                MessageBox.Show(Properties.Resources.OptionsError + ". " + GetNowTime);
                (((ScrollViewer_Options.MainContent as Grid).Children[16] as Grid).Children[0] as CustomControls.ControlButton).IsActive = false;
            }
        }

        private bool SetOptionsConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.SaveOptions + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (!code)
                MessageBox.Show(Properties.Resources.SaveError + ". " + GetNowTime);

            (((ScrollViewer_Options.MainContent as Grid).Children[16] as Grid).Children[0] as CustomControls.ControlButton).IsActive = false;
            return true;
        }
        #endregion

        #region DeviceOff Connection
        //запрос кол-ва подключенных к серверу устройств
        private void DeviceOffConnection()
        {
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.SaveOptions + GetNowTime);

            string rawURI = (MainWindow.IsAdmin ? Properties.Settings.Default.RawURI_Admin : "")
                + this.ID;

            MainWindow.HTTPClient.DeleteRequest(0, rawURI, DeviceOffConnection_ResponseHandler);
        }

        private bool DeviceOffConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.DeviceOFF + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (!code)
                MessageBox.Show(Properties.Resources.SaveError + ". " + GetNowTime);

            (((ScrollViewer_Options.MainContent as Grid).Children[16] as Grid).Children[1] as CustomControls.ControlButton).IsActive = false;
            return true;
        }
        #endregion

        //разбор сообщения. Определение и вывод параметров
        private void OptionDecoder(string response)
        {
            string[] options = response.Split('&');

            foreach (string option in options)
            {
                string[] optionKeyValue = option.Split('=');

                switch (optionKeyValue[0])
                {
                    case "id":
                        ScrollViewer_Options.TitleContent += ". id=" + optionKeyValue[1];
                        break;
                    case "name":
                        (((ScrollViewer_Options.MainContent as Grid).Children[1] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "location":
                        (((ScrollViewer_Options.MainContent as Grid).Children[3] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "brightness":
                        ((ScrollViewer_Options.MainContent as Grid).Children[5] as Slider).Value = Convert.ToInt32(optionKeyValue[1]);
                        break;
                    case "uri":
                        (((ScrollViewer_Options.MainContent as Grid).Children[7] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "statustimer":
                        (((ScrollViewer_Options.MainContent as Grid).Children[9] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "responsetimer":
                        (((ScrollViewer_Options.MainContent as Grid).Children[11] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "repeat":
                        (((ScrollViewer_Options.MainContent as Grid).Children[15] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                    case "pausetimer":
                        (((ScrollViewer_Options.MainContent as Grid).Children[13] as Border).Child as TextBox).Text = optionKeyValue[1];
                        break;
                }
            }
        }

        //возвращение текущего времени
        private string GetNowTime
        {
            get { return ("Время: " + DateTime.Now.ToLongTimeString()); }
        }

        //список параметор устройства (для выгрузки на сервер)
        private List<KeyValuePair<string, string>> GetOptions
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();

                //выполняемая операция (oprt=set)
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.PostRequest_Operation, Properties.Settings.Default.PostRequest_OperationValue[3]));

                options.Add(new KeyValuePair<string, string>("name", (((ScrollViewer_Options.MainContent as Grid).Children[1] as Border).Child as TextBox).Text));
                options.Add(new KeyValuePair<string, string>("location", (((ScrollViewer_Options.MainContent as Grid).Children[3] as Border).Child as TextBox).Text));
                options.Add(new KeyValuePair<string, string>("brightness", ((ScrollViewer_Options.MainContent as Grid).Children[5] as Slider).Value.ToString()));
                //если пользователь администратор
                if (MainWindow.IsAdmin)
                {
                    options.Add(new KeyValuePair<string, string>("uri", (((ScrollViewer_Options.MainContent as Grid).Children[7] as Border).Child as TextBox).Text));
                    try
                    {
                        options.Add(new KeyValuePair<string, string>("statustimer", TimeSpan.Parse((((ScrollViewer_Options.MainContent as Grid).Children[9] as Border).Child as TextBox).Text).ToString()));
                        options.Add(new KeyValuePair<string, string>("responsetimer", TimeSpan.Parse((((ScrollViewer_Options.MainContent as Grid).Children[11] as Border).Child as TextBox).Text).ToString()));
                        options.Add(new KeyValuePair<string, string>("pausetimer", TimeSpan.Parse((((ScrollViewer_Options.MainContent as Grid).Children[13] as Border).Child as TextBox).Text).ToString()));
                        options.Add(new KeyValuePair<string, string>("repeat", Convert.ToInt32((((ScrollViewer_Options.MainContent as Grid).Children[15] as Border).Child as TextBox).Text).ToString()));
                    }
                    catch
                    {
                        return null;
                    }
                    //ID администратора
                    options.Add(new KeyValuePair<string, string>("id", MainWindow.AdminID.ToString()));
                }

                return options;
            }
        }
    }
}
