using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_User
{
    public class Class_HTTPClient : INotifyPropertyChanged
    {
        private string ServerURI { get; set; } //адрес
        private int MaxRepeatRequest { get; set; } //кол-во повторных запросов
        private TimeSpan PauseTimer; //время паузы перед повторным запросом
        private TimeSpan ResponseTimeOut; //время ожидания ответа 
        private TimeSpan SaveResponseTimeOut; //время ожидания ответа 


        private readonly Action<int, string> MessageOutput; //вывод сообщения на экран

        public Class_HTTPClient(Action<int, string> messageOutput)
        {
            this.MessageOutput = messageOutput;
            this.ServerURI = Properties.Settings.Default.DefaultURI;
            this.MaxRepeatRequest = 1;
            //установка таймеров
            this.PauseTimer = Properties.Settings.Default.DefaultRepeatedRequestTimer;
            this.ResponseTimeOut = Properties.Settings.Default.DefaultResponseTimeOut;
            this.SaveResponseTimeOut = Properties.Settings.Default.DefaultSaveResponseTimeOut;
        }

        #region Request

        //отправка GET-запроса на сервер
        public async void GetRequest(
            int repeatedRequest //0 - первичный запрос, 1+ - повторный запрос
            , string rawURI //id устройства в формате URI
            , Func<bool, string, bool> responseHandler //функция обработки ответа
            )
        {
            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient() { Timeout = this._ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.GetAsync(this.ServerURI + rawURI))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {

                        string result = await content.ReadAsStringAsync();

                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));

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
            if (MainWindow.IsAdmin) rawURI = "admin/" + rawURI;

            //создание клиента с заданный периодом ожидания
            using (HttpClient client = new HttpClient() { Timeout = this._ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.PutAsync(this.ServerURI + rawURI, new FormUrlEncodedContent(postMessage)))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {

                        string result = await content.ReadAsStringAsync();
                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));

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
            using (HttpClient client = new HttpClient())
                try
                {
                    //запрос на изменение данных устройства
                    if (postMessage.FirstOrDefault(a => a.Key == Properties.Settings.Default.PostRequest_Operation).Value == Properties.Settings.Default.PostRequest_OperationValue[3])
                        client.Timeout = this._SaveResponseTimeOut;
                    else client.Timeout = this._ResponseTimeOut;

                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.PostAsync(this.ServerURI + rawURI, new FormUrlEncodedContent(postMessage)))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {

                        string result = await content.ReadAsStringAsync();
                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));

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
            using (HttpClient client = new HttpClient() { Timeout = this._ResponseTimeOut })
                try
                {
                    //асинхронный запрос на сервер
                    using (HttpResponseMessage response = await client.DeleteAsync(this.ServerURI + rawURI))
                    //полученный ответ
                    using (HttpContent content = response.Content)
                    {

                        string result = await content.ReadAsStringAsync();

                        responseHandler(StatusCode_Handler((int)response.StatusCode, ref result), result); //возвращение ответа для дальнейшей обработки
                    }
                }
                //обработка исключение: превышен лимит ожидания
                catch
                {
                    this.MessageOutput(2, RepeatRequestMessage(repeatedRequest));

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
#endregion
        
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
                case 202:
                    result = "202: Запрос не обработан (список ожидания переполнен)";
                    return false;
                case 400:
                    result = "400: Некорректный запрос";
                    return false;
                case 403:
                    result = "403: Пользователь не авторизован";
                    return false;
                case 404:
                    result = "404: Запрашиваемое устройство отсутствует";
                    return false;
                case 500:
                    result = "500: Внутренная серверная ошибка";
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

        //изменение PauseTimer
        public TimeSpan _PauseTimer
        {
            get { return this.PauseTimer; }
            set
            {
                    if (value > Properties.Settings.Default.MaxRepeatedRequestTimer) value = Properties.Settings.Default.MaxRepeatedRequestTimer;

                    this.MessageOutput(2, ChangeMessage("Время паузы для повторного запроса", this._PauseTimer.ToString(), value.ToString()));

                    this.PauseTimer = value;
                RaisePropertyChanged("_PauseTimer");
            }
        }


        //изменение ResponseTimeOut
        public TimeSpan _ResponseTimeOut
        {
            get { return this.ResponseTimeOut; }
            set
            {
                    this.MessageOutput(2, ChangeMessage("Время ожидания ответа", this._ResponseTimeOut.ToString(), value.ToString()));
                    this.ResponseTimeOut = value;

                RaisePropertyChanged("_ResponseTimeOut");
            }
        }

        //изменение SaveResponseTimeOut
        public TimeSpan _SaveResponseTimeOut
        {
            get { return this.SaveResponseTimeOut; }
            set
            {
                this.MessageOutput(2, ChangeMessage("Время ожидания ответа сохранения", this._SaveResponseTimeOut.ToString(), value.ToString()));
                this.ResponseTimeOut = value;

                RaisePropertyChanged("_SaveResponseTimeOut");
            }
        }
        #endregion

        //конструктор сообщения об изменении параметра
        private string ChangeMessage(string optionName, string oldValue, string newValue)
        {
            if (oldValue != newValue)
                return (Properties.Resources.ChangeOption + optionName + ": " + oldValue + " -> " + newValue);
            return "";
        }
    }
}