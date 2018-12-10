using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

namespace Netter_HTTP_Server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DeviceList_Users_Admin ConnectionList; //список подключенных устройств
        public HTTPServer Server; //сервер

        public MainWindow()
        {
            InitializeComponent();

            Title.MainTitle = Properties.Settings.Default.ProjectName.ToUpper();
            Title.IDTitle = "Server".ToUpper();
            //инициализация классов
            ConnectionList = new DeviceList_Users_Admin(MessageOutput);
            Server = new HTTPServer(MessageOutput, GetRequestMathodHandler, PutRequestMathodHandler, PostRequestMathodHandler, DeleteRequestMathodHandler);
            DeviceListUpdate();
        }

        #region Window
        //перемещение окна
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
        }
        #endregion

        #region Button CLick
        //закрыть окно
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Button_OFF_Click(sender, e);
            Close();
        }

        //очистка поля сообщений 
        private void Button_MessageTextBoxClear_Click(object sender, RoutedEventArgs e)
        {
            (ScrollViewer_Message.MainContent as TextBox).Text = "";
        }

        #region ON OFF
        //ВКЛ/ВЫКЛ
        private void Button_ON_Click(object sender, RoutedEventArgs e)
        {
            ONOFF_VisualDisplay();

            if (this.Server.Start())
                DeviceListUpdate();
            else ONOFF_VisualDisplay();


        }
        private void Button_OFF_Click(object sender, RoutedEventArgs e)
        {

            this.Server.Stop();
            ONOFF_VisualDisplay();
        }
        #endregion
        #endregion

        #region TextBox
        //проверка длины текста
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

        //визуальное отображение работы с сервером
        private void ONOFF_VisualDisplay()
        {
            Button_ONOFF.IsActive ^= true;
            if (Button_ONOFF.IsActive)
            {
                //блокировка изменения полей для адреса и таймера
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[3] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[5] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[7] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[9] as Border).Child as TextBox).IsReadOnly = true;

                //изменение метода
                Button_ONOFF.Click -= Button_ON_Click;
                Button_ONOFF.Click += Button_OFF_Click;
            }
            else
            {
                //разблокировка изменения полей для адреса и таймера
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[3] as Border).Child as TextBox).IsReadOnly = false;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[5] as Border).Child as TextBox).IsReadOnly = false;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[7] as Border).Child as TextBox).IsReadOnly = false;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[9] as Border).Child as TextBox).IsReadOnly = false;

                //изменение метода
                Button_ONOFF.Click -= Button_OFF_Click;
                Button_ONOFF.Click += Button_ON_Click;
            }
        }

        //обновление списка устройств
        private void DeviceListUpdate()
        {
            DataContext = new { Door = ConnectionList._Door, Window = ConnectionList._Window, Lighting=ConnectionList._Lighting, Server, Other = this.ConnectionList };

            foreach (object group in (ScrollViewer_DeviceList.MainContent as StackPanel).Children)
            {
                foreach (object type in ((group as CustomControls.GroupBox).MainContent as StackPanel).Children)
                {
                    try
                    {
                        ((type as CustomControls.GroupBox).MainContent as ItemsControl).Items.Refresh();
                    }
                    catch{ };
                }
            }
        }
        
        //вывод сообщения
        private bool MessageOutput(bool separator, string str)
        {
            try
            {
                //проверка длины поля сообщений 
                if ((ScrollViewer_Message.MainContent as TextBox).LineCount > Properties.Settings.Default.MaxCountMessageLine) Button_MessageTextBoxClear_Click(new object(), new RoutedEventArgs());
            }
            catch { }
            if ((ScrollViewer_Message.MainContent as TextBox).Text != "")
            {
                (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
                if (separator) (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
            }
            (ScrollViewer_Message.MainContent as TextBox).Text += (str);

            //перевод scrollBar в нижнее положение
            ScrollViewer_Message.ScrollToEnd();

            return true;
        }

        #region Обработчики запросов
        //обработчик GET-запроса (добавление устройства)
        private string GetRequestMathodHandler(string s_id, HttpListenerResponse response)
        {
            //получение ID устройства (группа, тип, id) из RawURL
            Request request = new Request(s_id);
            //если не было проблем с расшифровкой индекса
            if (request.ID != null)
            {
                if (request.ID.Count() == 2) //запрос списка устройств
                {
                    if (request.DevicePage == -1) //запрос кол-ва устройств
                        return "200" + this.ConnectionList.DeviceList.Where(a => a.GroupID == request.ID[0] && a.TypeID == request.ID[1]).Count();
                    //запрос конкретного устройства по номеру в списке
                    else return "200" + this.ConnectionList.GetDevice(request.ID[0], request.ID[1], request.DevicePage).GetMainDeviceInfo;
                }
                else
                {         
                    //поиск устройства в списке устройств
                    Device thisDevice = ConnectionList.DeviceList.FirstOrDefault(a => a.GroupID == request.ID[0] && a.TypeID == request.ID[1] && a.DeviceID == request.ID[2]);
                    //если устройство найдено
                    if (thisDevice != null)
                    {
                        //Long-Polling-запрос
                        if (request.IsLongPollingRequest) 
                        {
                            thisDevice.LongPollingResponse = response;
                            MessageOutput(false, Properties.Resources.LongPollingRequest + thisDevice._ID);
                            return "";
                        }
                        else
                        //запрос информации об устройстве от пользователя
                        {
                            //запрос от имени администратора
                            if (request.IsAdminRequest) return "200" + thisDevice.GetAdminDeviceInfo;
                            //запрос от имени простого пользователя
                            else return "200" + thisDevice.GetDeviceInfo;
                        }
                    }
                    else //устройство не существует
                        return "404";
                }
            }
            else //проблемы с запросом
                return "400";
            return "500";
        }

        //обработчик PUT-запроса
        private string PutRequestMathodHandler(string s_id, string requestBody)
        {
            //получение ID устройства (группа, тип, id) из RawURL
            Request request = new Request(s_id);
            //если не было проблем с расшифровкой индекса
            if (request.ID != null && request.ID.Count == 3) //доьавление устройства
            {
                //список параметров, рашифровка параметров -> Class_KeyValue
                List<KeyValue> options = PostRequestDecoder(requestBody);
                //время до автоматического отключения устройства
                KeyValue timeout = options.FirstOrDefault(a => a._Key == Properties.Settings.Default.DeleteTimerTitle);

                TimeSpan timerTicks = Properties.Settings.Default.DefaultDeleteTimer; //значение таймера по умолчанию
                if (timeout != null) timerTicks = new TimeSpan(Properties.Settings.Default.TimeOutMultiplication * Convert.ToInt32(timeout._Value)); //значение таймера с устройства
                                                                                                                                                     //добавление нового устройства
                return "201" + NewDeviceConnection(request.ID, timerTicks);
            }
            else if (request.IsAdminRequest)
            {
                if (this.ConnectionList._AdminID == 0)
                {
                    //список параметров, рашифровка параметров -> Class_KeyValue
                    List<KeyValue> options = PostRequestDecoder(requestBody);

                    if (options.FirstOrDefault(a => a._Key == "login") != null && options.FirstOrDefault(a => a._Key == "password") != null)
                    {
                        //если логин и паролль совпадают
                        if (this.ConnectionList.AdminOn(options.FirstOrDefault(a => a._Key == "login")._Value, options.FirstOrDefault(a => a._Key == "password")._Value))
                            return "201" + this.ConnectionList._AdminID_str; //админ авторизован (выдан id)
                        else return "403"; //неправильный логин или пароль
                    }
                    else return "403"; //отсутствует логин или пароль
                }
                else return "403"; //администратор уже зарегистрирован
            }
            else //проблемы с запросом
                return "400";
        }

        //обработчик POST-запроса (обновление информации об устройстве)
        private string PostRequestMathodHandler(string s_id, string requestBody, HttpListenerResponse response)
        {
            //получение ID устройства (группа, тип, id) из RawURL
            Request request = new Request(s_id);
            //если не было проблем с расшифровкой индекса
            if (request.ID != null && request.ID.Count == 3)
            {
                //список параметров, рашифровка параметров -> Class_KeyValue
                List<KeyValue> options = PostRequestDecoder(requestBody);
                //выполняемая операция
                string operationValue = GetOperationNumber(options);
                //если не указана выполняемая операция
                if (operationValue != "")
                {
                    //поиск устройства в списке устройств
                    Device thisDevice = ConnectionList.DeviceList.FirstOrDefault(a => a.GroupID == request.ID[0] && a.TypeID == request.ID[1] && a.DeviceID == request.ID[2]);
                    //если устройство найдено
                    if (thisDevice != null)
                    {
                        //обработка данной операции
                        return OperationValue(operationValue, request.IsAdminRequest, options, thisDevice, response);                         
                    }
                    else //устройство не существует
                        return "404";
                    ;
                }
                else //нет выполняемой операции
                    return "400";
            }
            else //проблемы с запросом
                return "400";

            return "500";
        }

        //обработчик DELETE-запроса (удаление устройства)
        private string DeleteRequestMathodHandler(string s_id, string requestBody)
        {
            //получение ID устройства (группа, тип, id) из RawURL
            Request request = new Request(s_id);
            //если не было проблем с расшифровкой индекса
            if (request.ID != null && request.ID.Count == 3)
            {
                //поиск устройства в списке устройств
                Device thisDevice = ConnectionList.DeviceList.FirstOrDefault(a => a.GroupID == request.ID[0] && a.TypeID == request.ID[1] && a.DeviceID == request.ID[2]);
                //если устройство найдено
                if (thisDevice != null)
                {
                    //отправка ответа на отключение устройства
                    MessageOutput(false, Properties.Resources.LongPollingResponse);
                    this.Server.ResponceOutput("200" + Properties.Settings.Default.PostRequest_Operation + "=" + Properties.Settings.Default.PostRequest_OperationValue[1], thisDevice.LongPollingResponse);
                    DeviceDisconnection(thisDevice); //отключение устройства
                    return "200";
                }
                else //устройство не существует
                    return "404";
            } 
            //отключение администратора
            else if (request.IsAdminRequest)
            {
                this.ConnectionList.AdminOff();
                return "200";
            }
            else //проблемы с запросом
                return "400";
            return "500";
        }
        #endregion

        //разбор запроса на добавление нового устройства
        private string NewDeviceConnection(List<int> id, TimeSpan timerTicks)
        {
            try
            {
                //список всех устройств данного типа
                List<Device> thisDevices = ConnectionList.DeviceList.Where(a => a.GroupID == id[0] && a.TypeID == id[1]).ToList();
                
                //добавление нового устройства к списку устройств
                Device thisDevice = new Device(id[0], id[1], this.ConnectionList.AllDeviceNumber, timerTicks, DeviceDisconnection);
                this.ConnectionList.Add(thisDevice);
                DeviceListUpdate(); //обновление списка

                //отправка нового ID
                return "id=" + thisDevice.DeviceID;
            }
            catch
            {
                return "500"; //непредвиденная ошибка
            }
        }

        //обработка операции
        private string OperationValue(string operation, bool isAdmin, List<KeyValue> options, Device device, HttpListenerResponse response)
        {
            switch (operation)
            {
                case "get": //обновление параметров устройства
                    device.OptionsUpdate(options); //обновление данных об устройстве
                    //ответ пользователям
                    foreach (HttpListenerResponse userResponse in device.UserResponses)
                    {
                        this.Server.ResponceOutput("200", userResponse);
                    }
                    device.UserResponses = new List<HttpListenerResponse>();
                    return "200";
                    break;
                case "set": //запрос от пользователя на изменение параметров устройства
                    //если кол-во пользователей, ожидающих ответов, не большое
                    if (device.UserResponses.Count < this.Server._MaxNumberOfWaitingUsers)
                    {
                        //проверка на наличие adminID в запросе
                        if (isAdmin)
                        {
                            KeyValue admin = options.FirstOrDefault(a => a._Key == "id");
                            if (admin != null)
                            {
                                if (Convert.ToInt32(admin._Value) == this.ConnectionList._AdminID) isAdmin = true;
                                else return "403"; //неавторизованный администратор
                            }
                        }

                        string responseText = "200" + Properties.Settings.Default.PostRequest_Operation + "=" + Properties.Settings.Default.PostRequest_OperationValue[3];
                        //создание ответа (исключение параметров, на изменение которых у пользователя нет разрешения)
                        foreach (KeyValue option in options)
                        {
                            //проверка параметров на "доступно только для администратора"
                            bool isAdminOption = false;
                            for (int i=0; !isAdminOption && i< Properties.Settings.Default.AdminDeviceOptions.Count;i++)
                                if (Properties.Settings.Default.AdminDeviceOptions[i] == option._Key) isAdminOption = true;

                            //выборка параметров (по правам доступа)
                            if (isAdmin || !isAdminOption)
                                responseText += "&" + option.ToString;
                        }
                        //ответ на устройство по Long-Polling
                        this.Server.ResponceOutput(responseText, device.LongPollingResponse);
                        //добавление ответа пользователю в список на ожидание
                        device.UserResponses.Add(response);
                        return "";
                    }
                    else
                    {
                        return "202";
                    }
                    break;
            }
            //нет выполняемой операции
            return "400";
        }

        //отключение устройства
        private bool DeviceDisconnection(Device device)
        {   
            this.ConnectionList.Remove(device); //удаление данного устройства
            DeviceListUpdate(); //обновление списка устройств
            return true;
        }
        
        //расшифровка (считывание) post запроса. определение парамеров
        private List<KeyValue> PostRequestDecoder(string requestBody)
        {
            try
            {
                List<KeyValue> options = new List<KeyValue>();

                //деление строки на куски по &
                string[] s = requestBody.Split('&');
                foreach (string param in s)
                {
                    //деление строки на куски по =
                    string[] keyValue = param.Split('=');
                    keyValue[1] = keyValue[1].Replace("%3A", ":").Replace("%2F", "/"); //замена %3A на : |  %3A на /

                    //замена + на пробел
                    KeyValue option = new KeyValue(keyValue[0], keyValue[1].Replace('+', ' '));
                    MessageOutput(false, option._Key + " = " + option._Value);
                    options.Add(option);
                }
                return options;
            }
            catch
            {
                return null; //если возникли проблемы при разшифровке
            }
        }

        //возвращение типа выполняемой операции
        private string GetOperationNumber(List<KeyValue> options)
        {
            //поиск параметра, обозначающего выполняемую операцию
            KeyValue option = options.FirstOrDefault(a => a._Key == Properties.Settings.Default.PostRequest_Operation);
            options.Remove(option); //удаление элемента (status)
            return option != null ? option._Value : "";
        }
    }
}
