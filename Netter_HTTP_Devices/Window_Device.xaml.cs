using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Netter_HTTP_Devices
{
    /// <summary>
    /// Логика взаимодействия для Window_Device.xaml
    /// </summary>
    public partial class Window_Device : Window
    {
        private DispatcherTimer SendingDataTimer; //таймер для связи с сервером
        private Class_Device DeviceInfo; //информация об устройстве
        private Class_HTTPClient HTTPClient; //http client
        private List<KeyValuePair<string, string>> LastOutputOptions; //список параметров устройства, актуальных для сервера

        private bool LastMessageIsChangeMessage; //тип последнего выведенного сообщения (true - Изменение:б false - прочее). Необходим для группировки сообщений на изменение

        public Window_Device(string groupTitle, string typeTitle, Page devicePage, Class_Device deviceInfo, Color firstColor, Color secondColor)
        {
            InitializeComponent();

            this.LastMessageIsChangeMessage = false;

            this.LastOutputOptions = new List<KeyValuePair<string, string>>();

            //настройка таймера
            this.SendingDataTimer = new DispatcherTimer();
            this.SendingDataTimer.Tick += SendingDataTimer_Tick;

            //вывод названий
            Title.MainTitle = Properties.Settings.Default.ProjectName.ToUpper();
            Title.GroupTitle = groupTitle.ToUpper();
            Title.TypeTitle = typeTitle.ToUpper();
            Title.First_Color = firstColor;
            Title.Second_Color = secondColor;

            //подключение страницы устройства
            Frame_Options.Navigate(devicePage);

            //информация об устройстве
            this.DeviceInfo = deviceInfo;
            this.DeviceInfo.MessageOutput = MessageOutput;
            //http клиент
            this.HTTPClient = new Class_HTTPClient(MessageOutput);

            //вывод параметров на экран
            DataContext = new { this.DeviceInfo, this.HTTPClient };

            (ScrollViewer_Message.MainContent as TextBox).Text = "";
        }
        

        #region Window
        //перемещение окна
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (this.WindowState != WindowState.Normal) this.WindowState = WindowState.Normal;
        }
        #endregion

        #region Button
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
            this.HTTPClient._ConnectionStatusID = 1;
            MessageOutput(1, "URI сервера: " + this.HTTPClient._ServerURI);

            ONOFF_VisualDisplay(true); //инициализация

            //проверка соединения с сервером
            ConnectionCheck();
            //дальнейшая обработка выполняется только после получения ответа 
            //-> FirstConnection_ResponseHandler
        }
        private void Button_OFF_Click(object sender, RoutedEventArgs e)
        {
            //отключение от устройства
            //если уже имеется активное соединение с сервером (status = 0)
            //и не выполняется отправка информации об устройстве по таймеру
            if (this.HTTPClient._ConnectionStatusID == 0 && this.SendingDataTimer.IsEnabled)
                LastConnection();
        }

        //визуальное отображение работы с сервером
        private void ONOFF_VisualDisplay(bool onoff)
        {
            if (onoff)
            {
                Button_ONOFF.IsActive = true;

                //установка периода таймера
                this.SendingDataTimer.Interval = this.HTTPClient._StatusTimer;

                //блокировка изменения полей для адреса и таймера
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[3] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[5] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[7] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[9] as Border).Child as TextBox).IsReadOnly = true;
                (((ScrollViewer_ServerInfo.MainContent as Grid).Children[11] as Border).Child as TextBox).IsReadOnly = true;

                //изменение метода
                Button_ONOFF.Click -= Button_ON_Click;
                Button_ONOFF.Click += Button_OFF_Click;
            }
            else
            {
                if (this.HTTPClient._ConnectionStatusID == 2)
                {
                    //обнуление списка параметров устройства, актуальных дял сервера
                    this.LastOutputOptions = null;

                    this.SendingDataTimer.Stop(); //отключение таймера
                    if (this.DeviceInfo._DeviceID != 0) this.DeviceInfo._DeviceID = 0;

                    Button_ONOFF.IsActive = false;

                    //разблокировка изменения полей для адреса и таймера
                    (((ScrollViewer_ServerInfo.MainContent as Grid).Children[3] as Border).Child as TextBox).IsReadOnly = false;
                    (((ScrollViewer_ServerInfo.MainContent as Grid).Children[5] as Border).Child as TextBox).IsReadOnly = false;
                    (((ScrollViewer_ServerInfo.MainContent as Grid).Children[7] as Border).Child as TextBox).IsReadOnly = false;
                    (((ScrollViewer_ServerInfo.MainContent as Grid).Children[9] as Border).Child as TextBox).IsReadOnly = false;
                    (((ScrollViewer_ServerInfo.MainContent as Grid).Children[11] as Border).Child as TextBox).IsReadOnly = false;

                    //изменение метода
                    Button_ONOFF.Click -= Button_OFF_Click;
                    Button_ONOFF.Click += Button_ON_Click;
                }
            }
        }
        #endregion
        #endregion

        #region TextBox
        //проверка вводимого текста
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length > Properties.Settings.Default.LengthLong) //ограничение по длине
                e.Handled = true;
        }
        private void URI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthShot) //ограничение по длине
            {
                for (int i =0; i<e.Text.Length && e.Handled != true; i++)
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

        #region Frame
        //Изменение frame
        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Frame frame = (sender as Frame);

            if (frame.NavigationService.RemoveBackEntry() != null)
            {
                frame.RemoveBackEntry();
                frame.NavigationService.RemoveBackEntry();
            }
        }
        #endregion
        
        //постоянное (по таймеру) информирование сервера о статусе устройства
        private void SendingDataTimer_Tick(object sender, System.EventArgs e)
        {
            this.SendingDataTimer.Stop(); //остановка таймера
            StatusConnection(); //передача информации об устройстве
            //запуск таймера после получения ответа на запрос (StatusConnection_ResponseHandler
        }

        #region Connection Checked
        //проверка соединения (GET)
        private void ConnectionCheck()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.ConnectionCheckRequest + GetNowTime);
            this.HTTPClient.GetRequest(0, "", ConnectionCheck_ResponseHandler);
        }

        private bool ConnectionCheck_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.ConnectionCheckRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                FirstConnection();
            }
            else
            {
                ONOFF_VisualDisplay(false); //отключение соединения
            }

            return true;
        }
        #endregion

        #region First Coneection
        //первое подключение к серверу (PUT)
        private void FirstConnection()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.ONRequest + GetNowTime);

            List<KeyValuePair<string, string>> putMessage = new List<KeyValuePair<string, string>>();
            putMessage.Add(this.HTTPClient.GetRequestOptions_StatusTimer);
            
            //первый запрос: получение IDустойства
            this.HTTPClient.PutRequest(0, this.DeviceInfo.GetID_URI, putMessage, FirstConnection_ResponseHandler);
        }

        //обработчик ответа Первого запроса
        private bool FirstConnection_ResponseHandler(bool code, string response)
        {
            //ответ: id=001
            int id = ID_Check(response.Split('='));
            MessageOutput(1, Properties.Resources.ONRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);

            if (code && id != -1)
            {
                this.DeviceInfo._DeviceID = id;

                //создание пустого списка параметров устройства, актуальных для сервера
                this.LastOutputOptions = new List<KeyValuePair<string, string>>();

                //Long-Polling
                LongPollingConnection();
                //отправка информации об устростве на сервер
                StatusConnection();
            }
            else
            {
                //MessageOutput(true, response);
                ONOFF_VisualDisplay(false); //отключение соединения
            }
            return true;
        }
        #endregion

        #region Status Connection
        //передача информации об устройстве
        private void StatusConnection()
        {
            this.SendingDataTimer.Stop(); //остановка таймера
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.StatusRequest + GetNowTime);

            List<KeyValuePair<string, string>> postMessage = new List<KeyValuePair<string, string>>();
            //список параметров устройства
            postMessage.AddRange(this.DeviceInfo.GetOptions);
            postMessage.AddRange(this.HTTPClient.GetOptions);

            OptionsCheck(postMessage);
            //обозначение выполняемой операции (get)
            postMessage.Add(new KeyValuePair<string, string>(Properties.Settings.Default.PostRequest_Operation, Properties.Settings.Default.PostRequest_OperationValue[2]));

            if (postMessage != null)
            //первый запрос: получение IDустойства
            this.HTTPClient.PostRequest(0, this.DeviceInfo.GetID_URI, postMessage, StatusConnection_ResponseHandler);
        }

        //обработчик ответа
        private bool StatusConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.StatusRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);

            if (code)
            {
                this.SendingDataTimer.Interval = this.HTTPClient._StatusTimer; //остановка таймера
                this.SendingDataTimer.Start(); //запуск таймера
            }
            else
            {
                //MessageOutput(true, response);
                ONOFF_VisualDisplay(false); //отключение соединения
            }
            return true;
        }
        #endregion

        #region Long-Polling Connection
        //Long-Polling
        //запрос на сервер с отложенным ответом
        //для моментального реагирования на запросы от пользователя
        private void LongPollingConnection()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.LongPollingRequest + GetNowTime);
            this.HTTPClient.GetRequest(0, this.DeviceInfo.GetID_URI + "/lp", LongPollingConnection_ResponseHandler);
        }

        //обработчик ответа Последнего запроса
        private bool LongPollingConnection_ResponseHandler(bool code, string response)
        {
            this.SendingDataTimer.Stop(); //остановка таймера
            MessageOutput(2, Properties.Resources.LongPollingRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);

            string operationValue = "";
            List<KeyValuePair<string, string>> options = null;

            if (response != "")
            {
                try
                {
                    options = stringToKeyValueList(response);
                    KeyValuePair<string, string> operation = options.First(a => a.Key == Properties.Settings.Default.PostRequest_Operation);
                    options.Remove(operation);
                    operationValue = operation.Value;
                }
                catch { }
            }

            if (code)
            {
                switch(operationValue)
                {
                    case "": //перезагрузка Long-Polling-запроса
                        LongPollingConnection(); //перезагрузка Long-Polling запроса 
                        this.SendingDataTimer.Start(); //запуск таймера
                        break;
                    case "get": //запрос списка параметров
                        LongPollingConnection(); //перезагрузка Long-Polling запроса 
                        StatusConnection(); //отправка списка параметров
                        break;
                    case "set":
                        //обработка полученных от пользователя параметров
                        this.DeviceInfo.OptionsProcessing(options);
                        this.HTTPClient.OptionsProcessing(options);

                        LongPollingConnection(); //перезагрузка Long-Polling запроса 
                        StatusConnection(); //отправка списка параметров
                        break;
                    case "off":
                        LastConnection_ResponseHandler(code, "OK");
                        break;
                }
            }
            else
            {
                ONOFF_VisualDisplay(false);
            }
            return true;
        }
        #endregion

        #region Last Connection
        //отключение устройства от сервера
        private void LastConnection()
        {
            this.SendingDataTimer.Stop(); //остановка таймера
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.OFFRequest + GetNowTime);

            this.HTTPClient.DeleteRequest(0, this.DeviceInfo.GetID_URI, LastConnection_ResponseHandler);
        }

        //обработчик ответа Последнего запроса
        private bool LastConnection_ResponseHandler(bool code, string response)
        {
            if (this.HTTPClient._ConnectionStatusID != 2)
            {
                MessageOutput(1, Properties.Resources.OFFRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
                this.HTTPClient._ConnectionStatusID = 2;

                ONOFF_VisualDisplay(false);
            }

            return true;
        }
        #endregion

        //проверка корректновти ID
        private int ID_Check(string[] keyValue)
        {
            if (keyValue[0] == "id")
            {
                try
                {
                    int id = Convert.ToInt32(keyValue[1]);
                    return id;
                }
                catch
                {
                    return -1;
                }
            }
            return -1;
        }

        //вывод сообщения
        private bool MessageOutput(int separator, string str)
        {
            //проверка длины поля сообщений 
            if ((ScrollViewer_Message.MainContent as TextBox).LineCount > Properties.Settings.Default.MaxCountMessageLine) Button_MessageTextBoxClear_Click(new object(), new RoutedEventArgs());
            //выводить только в случаи наличия сообщения
            if (str != "")
            {
                //является ли текущее сообщение сообщение на Изменение
                bool thisMessageIsChangeMessage = str.Contains(Properties.Resources.ChangeOption);

                //если имеются уже выведенные на экран сообщения               
                if ((ScrollViewer_Message.MainContent as TextBox).Text != "")
                {
                    //если последнее и текущее сообщения не являются сообщениями на Изменение
                    if (!this.LastMessageIsChangeMessage || !thisMessageIsChangeMessage)
                        //вывод заданного числа enter'оф
                        for (int i = 0; i < separator; i++)
                            (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
                    //простой переход в следующую строку
                    else (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
                }
                //вывод сообщения
                (ScrollViewer_Message.MainContent as TextBox).Text += (str);

                //перевод scrollBar в нижнее положение
                ScrollViewer_Message.ScrollToEnd();

                this.LastMessageIsChangeMessage = thisMessageIsChangeMessage;
            }
            return true;
        }
        //возвращение текущего времени
        private string GetNowTime
        {
            get { return ("Время: " + DateTime.Now.ToLongTimeString()); }
        }

        //проверка параметров устройства на изменение перед отправкой на сервер
        private void OptionsCheck(List<KeyValuePair<string, string>> options)
        {
            try
            {
                List<KeyValuePair<string, string>> newLastOutputOptions = new List<KeyValuePair<string, string>>();
                newLastOutputOptions.AddRange(options);

                //былил ли параметры уже отправлены
                if (this.LastOutputOptions.Count != 0)
                {
                    foreach (KeyValuePair<string,string> option in this.LastOutputOptions)
                    {
                        //определение индекса, исходя из имени параметра
                        //index=-1 подразумевает неизвестную ошибку, т.к. this.LastOutputOptions и options по значению KEY не могут отличаться
                        int index = options.FindIndex(a => a.Key == option.Key);
                        if (options[index].Value == option.Value) options.RemoveAt(index);
                    }
                }
                this.LastOutputOptions = newLastOutputOptions;
            }
            catch
            {
                options = null;
                ONOFF_VisualDisplay(false);
            }
        }
        
        //перевод строки в список ключ-значение
        private List<KeyValuePair<string, string>> stringToKeyValueList(string str)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            string[] units = str.Split('&');
            foreach (string unit in units)
            {
                string[] keyValue = unit.Split('=');
                list.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
            }
            return list;
        }
    }
}
