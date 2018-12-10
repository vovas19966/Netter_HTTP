using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace Netter_HTTP_Devices
{
    public class Class_HTTPClient : INotifyPropertyChanged
    {
        private string ServerURI { get; set; } //адрес
        private int ConnectionStatus { get; set; } //статус подключения
        private int MaxRepeatRequest { get; set; } //кол-во повторных запросов
        private TimeSpan StatusTimer; //время отправки статуса
        private TimeSpan PauseTimer; //время паузы перед повторным запросом
        private TimeSpan ResponseTimeOut; //время ожидания ответа 


        private readonly Func<int, string, bool> MessageOutput; //вывод сообщения на экран

        public Class_HTTPClient(Func<int, string, bool> messageOutput)
        {
            this.MessageOutput = messageOutput;
            this.ConnectionStatus = 2;
            this.ServerURI = Properties.Settings.Default.DefaultURI;
            this.MaxRepeatRequest = 1;
            //установка таймеров
            this.StatusTimer = Properties.Settings.Default.DefaultStatusTimer;
            this.PauseTimer = Properties.Settings.Default.DefaultRepeatedRequestTimer;
            this.ResponseTimeOut = Properties.Settings.Default.DefaultResponseTimeOut;
        }


        //отправка GET-запроса на сервер
        public async void GetRequest(
            int repeatedRequest //0 - первичный запрос, 1+ - повторный запрос
            , string rawURI //id устройства в формате URI
            , Func<bool, string, bool> responseHandler //функция обработки ответа
            )
        {
            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient())
                try
                {
                    if (rawURI == "" || rawURI.Length < 3 || rawURI[rawURI.Length - 1] != 'p' || rawURI[rawURI.Length - 2] != 'l' || rawURI[rawURI.Length - 3] != '/')
                        client.Timeout = this.ResponseTimeOut;
                    else client.Timeout = new TimeSpan(-10000);

                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.GetAsync(this.ServerURI + rawURI))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {
                        //исключение повторного вывода сообщения
                        if (_ConnectionStatusID != 0) _ConnectionStatusID = 0;

                        string result = await content.ReadAsStringAsync();
                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));
                    //первичный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    if (repeatedRequest == 0) { if (_ConnectionStatusID != 1) _ConnectionStatusID = 1; }
                    //повторный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    else { if (_ConnectionStatusID != 2 && repeatedRequest >= this._MaxRepeatRequest) _ConnectionStatusID = 2; }

                    //выполнение повторного запроса
                    if (repeatedRequest++ < this._MaxRepeatRequest)
                    {
                        this.MessageOutput(1, "Пауза: " + this.PauseTimer + ". Время начала: " + DateTime.Now.ToLongTimeString());
                        await Task.Delay(this.PauseTimer);
                        this.MessageOutput(0, "; Время завершения: " + DateTime.Now.ToLongTimeString());

                        GetRequest(repeatedRequest, rawURI, responseHandler); //повторный запрос
                    }
                    else
                    {
                        string result = "";
                        responseHandler(StatusCode_Handler(503, ref result), result);
                    }
                }
        }

        //отправка PUT-запроса на сервер
        //добавление нового устройства
        public async void PutRequest(
            int repeatedRequest //0 - первичный запрос, 1+ - повторный запрос
            , string rawURI //id устройства в формате URI
            , List<KeyValuePair<string, string>> postMessage //время ожидания запроса
            , Func<bool, string, bool> responseHandler //функция обработки ответа
            )
        {
            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient() { Timeout = this.ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.PutAsync(this.ServerURI + rawURI, new FormUrlEncodedContent(postMessage)))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {
                        //исключение повторного вывода сообщения
                        if (_ConnectionStatusID != 0) _ConnectionStatusID = 0;

                        string result = await content.ReadAsStringAsync();
                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));
                    //первичный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    if (repeatedRequest == 0) { if (_ConnectionStatusID != 1) _ConnectionStatusID = 1; }
                    //повторный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    else { if (_ConnectionStatusID != 2 && repeatedRequest >= this._MaxRepeatRequest) _ConnectionStatusID = 2; }

                    //выполнение повторного запроса
                    if (repeatedRequest++ < this._MaxRepeatRequest)
                    {
                        this.MessageOutput(1, "Пауза: " + this.PauseTimer + ". Время начала: " + DateTime.Now.ToLongTimeString());
                        await Task.Delay(this.PauseTimer);
                        this.MessageOutput(0, "; Время завершения: " + DateTime.Now.ToLongTimeString());

                        PutRequest(repeatedRequest, rawURI, postMessage, responseHandler); //повторный запрос
                    }
                    else
                    {
                        string result = "";
                        responseHandler(StatusCode_Handler(503, ref result), result);
                    }
                }
        }

        //отправка POST-запроса на сервер
        public async void PostRequest(
            int repeatedRequest //0 - первичный запрос, 1+ - повторный запрос
            , string rawURI //id устройства в формате URI
            , List<KeyValuePair<string, string>> postMessage
            , Func<bool, string, bool> responseHandler //функция обработки ответа
            )
        {
            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient() { Timeout = this.ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.PostAsync(this.ServerURI + rawURI, new FormUrlEncodedContent(postMessage)))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {
                        //исключение повторного вывода сообщения
                        if (_ConnectionStatusID != 0) _ConnectionStatusID = 0;

                        string result = await content.ReadAsStringAsync();
                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));
                    //первичный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    if (repeatedRequest == 0) { if (_ConnectionStatusID != 1) _ConnectionStatusID = 1; }
                    //повторный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    else { if (_ConnectionStatusID != 2 && repeatedRequest >= this._MaxRepeatRequest) _ConnectionStatusID = 2; }

                    //выполнение повторного запроса
                    if (repeatedRequest++ < this._MaxRepeatRequest)
                    {
                        this.MessageOutput(1, "Пауза: " + this.PauseTimer + ". Время начала: " + DateTime.Now.ToLongTimeString());
                        await Task.Delay(this.PauseTimer);
                        this.MessageOutput(0, "; Время завершения: " + DateTime.Now.ToLongTimeString());

                        PostRequest(repeatedRequest, rawURI, postMessage, responseHandler); //повторный запрос
                    }
                    else
                    {
                        string result = "";
                        responseHandler(StatusCode_Handler(503, ref result), result);
                    }
                }
        }

        //отправка DELETE-запроса на сервер
        public async void DeleteRequest(
            int repeatedRequest //0 - первичный запрос, 1+ - повторный запрос
            , string rawURI //id устройства в формате URI
            , Func<bool, string, bool> responseHandler //функция обработки ответа
            )
        {
            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient() { Timeout = this.ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.DeleteAsync(this.ServerURI + rawURI))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {
                        //исключение повторного вывода сообщения
                        if (_ConnectionStatusID != 0 && _ConnectionStatusID != 2) _ConnectionStatusID = 0;

                        string result = await content.ReadAsStringAsync();

                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));
                    //первичный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    if (repeatedRequest == 0) { if (_ConnectionStatusID != 1) _ConnectionStatusID = 1; }
                    //повторный запрос
                    //изменение статуса соединения на ОЖИДАНИЕ
                    else { if (_ConnectionStatusID != 2 && repeatedRequest >= this._MaxRepeatRequest) _ConnectionStatusID = 2; }

                    //выполнение повторного запроса
                    if (repeatedRequest++ < this._MaxRepeatRequest)
                    {
                        this.MessageOutput(1, "Пауза: " + this.PauseTimer + ". Время начала: " + DateTime.Now.ToLongTimeString());
                        await Task.Delay(this.PauseTimer);
                        this.MessageOutput(0, "; Время завершения: " + DateTime.Now.ToLongTimeString());

                        DeleteRequest(repeatedRequest, rawURI, responseHandler); //повторный запрос
                    }
                    else
                    {
                        string result = "";
                        responseHandler(StatusCode_Handler(503, ref result), result);
                    }
                }
        }

        //текст сообщения о превышении времени ожидания
        private string RepeatRequestMessage(int num)
        {
            if (num == 0) return Properties.Resources.ErrorMessage_TimeOut;
            else return Properties.Resources.RepeatedRequest + num + ": " + Properties.Resources.ErrorMessage_TimeOut;
        }

        //проверка статус-кода ответа 
        private bool StatusCode_Handler(int code, ref string result)
        {
            switch (code)
            {
                case 200:
                    if (result == "") result = "OK";
                    return true;
                case 201: return true;
                case 400:
                    result = "400: Некорректный запрос";
                    return false;
                case 404:
                    result = "404: Запрашиваемое устройство отсутствует";
                    return false;
                case 500:
                    result = "500: Внутренняя серверная ошибка";
                    return false;
                case 503:
                    result = "503: Сервер не доступен";
                    return false;
            }
            result = "Неизвестная ошибка";
            return false;
        }

        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            // Если кто-то на него подписан, то вызывем его
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //изменение ServerStatus
        public string _ConnectionStatus
        {
            get
            {
                return Properties.Settings.Default.ConnectionStatus[_ConnectionStatusID];
            }
        }
        public int _ConnectionStatusID
        {
            get
            {
                return this.ConnectionStatus;
            }
            set
            {
                this.ConnectionStatus = value;
                switch (_ConnectionStatusID)
                {
                    case 0:
                        this.MessageOutput(2, "Статус подключения: " + Properties.Resources.ConnectionEstablished);
                        break;
                    case 1:
                        this.MessageOutput(2, "Статус подключения: " + Properties.Resources.Connecting);
                        break;
                    case 2:
                        this.MessageOutput(2, "Статус подключения: " + Properties.Resources.ConnectionBroken);
                        break;
                }
                RaisePropertyChanged("_ConnectionStatus");
            }
        }

        //изменение ServerURI
        public string _ServerURI
        {
            get { return this.ServerURI; }
            set
            {
                value = value.Trim();
                if (value[value.Length - 1] != '/') value += "/";
                if (value[value.Length - 2] == '/') value = value.Remove(value.Length - 1);
                value = value.Replace(" ", ""); //удаление пробелов 

                this.MessageOutput(2, ChangeMessage("URI", this._ServerURI, value));

                this.ServerURI = value;
                RaisePropertyChanged("_ServerURI");
            }
        }

        //изменение RepeatRequest
        public int _MaxRepeatRequest
        {
            get { return this.MaxRepeatRequest; }
            set
            {
                if (value < 1) value = 1;
                else if (value > Properties.Settings.Default.MaxRepeatRequest) value = Properties.Settings.Default.MaxRepeatRequest;

                this.MessageOutput(2, ChangeMessage("Кол-во повторных запросов", this._MaxRepeatRequest.ToString(), value.ToString()));
                this.MaxRepeatRequest = value;
                RaisePropertyChanged("_MaxRepeatRequest");
            }
        }

        //изменение StatusTimer
        public TimeSpan _StatusTimer
        {
            get { return this.StatusTimer; }
            set
            {
                try
                {
                    if (value > Properties.Settings.Default.MaxStatusTimer) value = Properties.Settings.Default.MaxStatusTimer;
                    if (value < Properties.Settings.Default.MinStatusTimer) value = Properties.Settings.Default.MinStatusTimer;

                    this.MessageOutput(2, ChangeMessage("Период обновления данных на сервере", this._StatusTimer.ToString(), value.ToString()));

                    this.StatusTimer = value;


                }
                catch
                {
                    this.MessageOutput(2, IncorrectMessage("Период обновления данных на сервере: ", this._StatusTimer.ToString()));
                }
                RaisePropertyChanged("_StatusTimer");

                if (this._ResponseTimeOut > this._StatusTimer) this._ResponseTimeOut = this._StatusTimer;
            }
        }

        //изменение PauseTimer
        public TimeSpan _PauseTimer
        {
            get { return this.PauseTimer; }
            set
            {
                try
                {
                    if (value > Properties.Settings.Default.MaxRepeatedRequestTimer) value = Properties.Settings.Default.MaxRepeatedRequestTimer;

                    this.MessageOutput(2, ChangeMessage("Время паузы для повторного запроса", this._PauseTimer.ToString(), value.ToString()));

                    this.PauseTimer = value;
                }
                catch
                {
                    this.MessageOutput(2, IncorrectMessage("Время паузы для повторного запроса: ", this._PauseTimer.ToString()));
                }
                RaisePropertyChanged("_PauseTimer");
            }
        }

        //изменение ResponseTimeOut
        public TimeSpan _ResponseTimeOut
        {
            get { return this.ResponseTimeOut; }
            set
            {
                try
                {
                    if (value > _StatusTimer) value = _StatusTimer;

                    this.MessageOutput(2, ChangeMessage("Время ожидания ответа", this._ResponseTimeOut.ToString(), value.ToString()));

                    this.ResponseTimeOut = value;
                }
                catch
                {
                    this.MessageOutput(2, IncorrectMessage("Время ожидания ответа: ", this._ResponseTimeOut.ToString()));
                }
                RaisePropertyChanged("_ResponseTimeOut");
            }
        }
        #endregion

        //возвращение значение таймера (ticks)
        public KeyValuePair<string, string> GetRequestOptions_StatusTimer
        {
            get
            {
                return new KeyValuePair<string, string>
                    (Properties.Settings.Default.DeleteTimerTitle
                    , _StatusTimer.Ticks.ToString());
            }
        }

        //конструктор сообщения об изменении параметра
        private string ChangeMessage(string optionName, string oldValue, string newValue)
        {
            if (oldValue != newValue)
                return (Properties.Resources.ChangeOption + optionName + ": " + oldValue + " -> " + newValue);
            return "";
        }
        private string IncorrectMessage(string optionName, string value)
        {
            return (Properties.Resources.IncorrectValue + optionName + ": " + value);
        }
        
        //возвращает список типа Ключ-Значение всех параметров
        public virtual List<KeyValuePair<string, string>> GetOptions
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.HTTPClientOptionsName[0], _ServerURI));
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.HTTPClientOptionsName[1], _StatusTimer.ToString()));
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.HTTPClientOptionsName[2], _ResponseTimeOut.ToString()));
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.HTTPClientOptionsName[3], _PauseTimer.ToString()));
                options.Add(new KeyValuePair<string, string>(Properties.Settings.Default.HTTPClientOptionsName[4], _MaxRepeatRequest.ToString()));
                return options;
            }
        }
        
        //обработка параметор, полученных от пользователя
        public void OptionsProcessing(List<KeyValuePair<string, string>> options)
        {
            foreach (KeyValuePair<string, string> option in options)
            {
                switch (option.Key)
                {
                    case "uri":
                        _ServerURI = option.Value;
                        break;
                    case "statustimer":
                        try
                        {
                            _StatusTimer = TimeSpan.Parse(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Период обновления данных на сервере", option.Value));
                        }
                        break;
                    case "requesttimer":
                        try
                        {
                            _ResponseTimeOut = TimeSpan.Parse(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Время ожидания ответа", option.Value));
                        }
                        break;
                    case "repeat":
                        try
                        {
                            _MaxRepeatRequest = Convert.ToInt32(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Кол-во повторных запросов", option.Value));
                        }
                        break;
                    case "pausetimer":
                        try
                        {
                            _PauseTimer = TimeSpan.Parse(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Время паузы для повторного запроса", option.Value));
                        }
                        break;
                }
            }
        }
    }
}
