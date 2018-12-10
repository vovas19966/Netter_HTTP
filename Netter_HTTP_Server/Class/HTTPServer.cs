using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace Netter_HTTP_Server
{
    public class HTTPServer : INotifyPropertyChanged
    {
        private string URI { get; set; } //адрес сервера
        private int ServerStatus { get; set; } //статус сервера
        private int MaxNumberOfWaitingUsers { get; set; } //максимальное количество ожидающих ответа пользователей

        private DateTime StartTime; //время запуска сервера
        private HttpListener Listener;

        private readonly Func<bool, string, bool> MessageOutput; //вывод сообщения на экран
        private readonly Func<string, HttpListenerResponse, string> GetRequesMethodtHandler; //обработка GET-запроса
        private readonly Func<string, string, string> PutRequesMethodtHandler; //обработка PUT-запроса
        private readonly Func<string, string, HttpListenerResponse, string> PostRequesMethodtHandler; //обработка POST-запроса
        private readonly Func<string, string, string> DeleteRequesMethodtHandler; //обработка DELETE-запроса

        public HTTPServer(
            Func<bool, string, bool> messageOutput //метод вывод сообщений
            , Func<string, HttpListenerResponse, string> getRequesMethodtHandler //метод обработки GET-запроса
            , Func<string, string, string> putRequesMethodtHandler //метод обработки PUT-запроса
            , Func<string, string, HttpListenerResponse, string> postRequesMethodtHandler //метод обработки POST-запроса
            , Func<string, string, string> deleteRequesMethodtHandler //метод обработки DELETE-запроса
            )
        {
            this.URI = Properties.Settings.Default.DefaultURI;
            this.ServerStatus = 1;
            this.MaxNumberOfWaitingUsers = Properties.Settings.Default.MaxNumberOfWaitingUsers;

            this.MessageOutput = messageOutput;
            this.GetRequesMethodtHandler = getRequesMethodtHandler;
            this.PutRequesMethodtHandler = putRequesMethodtHandler;
            this.PostRequesMethodtHandler = postRequesMethodtHandler;
            this.DeleteRequesMethodtHandler = deleteRequesMethodtHandler;
        }

        //запуск сервера
        public bool Start() //метод вывод сообщения
        {
            try
            {
                //если сервер не создан
                if (this.Listener == null || !this.Listener.IsListening)
                {
                    this.StartTime = DateTime.Now; //время запуска сервера

                    this.Listener = new HttpListener();
                    if (this.URI[this.URI.Length - 1] != '/') _URI = _URI + "/";
                    if (this.URI[this.URI.Length - 2] == '/') _URI = _URI.Remove(this.URI.Length - 1);
                    this.Listener.Prefixes.Add(this.URI);
                    this.Listener.Start();

                    Listening();

                    _ServerStatusID = 0; //изменение статуса сервера
                    this.MessageOutput(true, Properties.Resources.ServerStart_Message + "\nURI: " + this._URI + "\nДата и время запуска: " + this.StartTime); //вывод сообщения о запуске сервера
                    return true;
                }
            }
            catch
            {
                this.MessageOutput(true, Properties.Resources.ServerStart_Fail_Message + "\nURI: " + this._URI + "\nДата и время попытки запуска: " + this.StartTime); //вывод сообщения о запуске сервера
            }
            return false;
        }

        //отключение сервера
        public void Stop()
        {
            try
            {
                //если сервер создан
                if (this.Listener != null && this.Listener.IsListening)
                {
                    this.Listener.Stop();
                    this.Listener.Close();

                    DateTime timeStop = DateTime.Now;

                    _ServerStatusID = 1; //изменение статуса сервера
                    this.MessageOutput(true, Properties.Resources.ServerStop_Message + "\nДата и время отключения: " + timeStop + "\nВремя работы: " + Math.Round((timeStop.Ticks - this.StartTime.Ticks) * 1E-7, 3) + "c."); //вывод сообщения о запуске сервера
                }
            }
            catch { }
        }

        //прослушивние
        private async void Listening()
        {
            try
            {
                HttpListenerContext context = await this.Listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                //мультипоточная обработка запроса
                Listening();
                //обработка текущего запроса
                this.MessageOutput(true, RequestMessage(request)); //вывод текста ответа на экран


                switch (request.HttpMethod)
                {
                    case "GET":
                        if (request.RawUrl == "/") //пустой запрос (проверка соединения)
                        {
                            ResponceOutput("200", response); //отправка ответа
                        }
                        else Request_GetMethod(request, response); //обработка GET зарпоса
                        break;
                    case "PUT":
                        Request_PutMethod(request, response); //POST запрос
                        break;
                    case "POST":
                        Request_PostMethod(request, response); //POST запрос
                        break;
                    case "DELETE":
                        Request_DeleteMethod(request, response); //DELETE запрос
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        //обработка GET запроса
        private void Request_GetMethod(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string responseText = this.GetRequesMethodtHandler(request.RawUrl, response); //текст ответа
                //перевод и отправка ответа
                //если это не Long-Polling запрос
                //при формировании ответа не был определен статус-код: это возможно только в данном случаи
                if (responseText != "") ResponceOutput(responseText, response);
            }
            catch
            {
                //ошибка при расшировке GET запроса
                ResponceOutput("500", response); //отправка ответа 
            }
        }
        
        //обработка PUT запроса
        private void Request_PutMethod(HttpListenerRequest request, HttpListenerResponse response)
        {//расшифровка PUT запроса
            try
            {
                //весь текст PUT запроса
                StreamReader reader = new StreamReader(request.InputStream, Encoding.Default);
                string requestBody = reader.ReadToEnd(); //перевод в string

                this.MessageOutput(false, "Put запрос: "); //вывод текста ответа на экран

                string responseText = this.PutRequesMethodtHandler(request.RawUrl, requestBody); //текст ответа
                //перевод и отправка ответа
                if (responseText != "") ResponceOutput(responseText, response);
            }
            catch
            {
                //ошибка при расшировке put запроса
                ResponceOutput("500", response); //отправка ответа 
            }
        }
               
        //обработка POST запроса
        private void Request_PostMethod(HttpListenerRequest request, HttpListenerResponse response)
        {//расшифровка POST запроса
            try
            {
                //весь текст POST запроса
                StreamReader reader = new StreamReader(request.InputStream, Encoding.Default);
                string requestBody = reader.ReadToEnd(); //перевод в string

                this.MessageOutput(false, "Post запрос: "); //вывод текста ответа на экран

                string responseText = this.PostRequesMethodtHandler(request.RawUrl, requestBody, response); //текст ответа
                //перевод и отправка ответа
                if (responseText != "") ResponceOutput(responseText, response);
            }
            catch
            {
                //ошибка при расшировке post запроса
                ResponceOutput("500", response); //отправка ответа 
            }
        }
        
        //обработка DELETE запроса
        private void Request_DeleteMethod(HttpListenerRequest request, HttpListenerResponse response)
        {//расшифровка DELETE запроса
            try
            {
                //весь текст DELETE запроса
                StreamReader reader = new StreamReader(request.InputStream, Encoding.Default);
                string requestBody = reader.ReadToEnd(); //перевод в string
                
                string responseText = this.DeleteRequesMethodtHandler(request.RawUrl, requestBody); //текст ответа
                //перевод и отправка ответа
                if (responseText != "") ResponceOutput(responseText, response);
            }
            catch
            {
                //ошибка при расшировке delete запроса
                ResponceOutput("500", response); //отправка ответа 
            }
        }

        //сообщение о получении запроса
        private string RequestMessage(HttpListenerRequest request)
        {
            return Properties.Resources.Request_Message + " Метод: " + request.HttpMethod + "\nURI: " + request.Url + "\nДата и время получения: " + DateTime.Now;
        }

        //создание и отправка ответа
        public void ResponceOutput(string responceText, HttpListenerResponse response)
        {
            string code = "";
            try
            {
                //получение статус кода
                if (responceText.Length == 3)
                    code = responceText;
                else code = responceText.Remove(3);

                response.StatusCode = Convert.ToInt32(code);
                responceText = responceText.Remove(0, 3);

                this.MessageOutput(false, "Статус-Код: " + response.StatusCode); //вывод статус-кода на экран
                                                                                 //проверка статус-кода

                if (responceText != "") this.MessageOutput(false, "Ответ: " + responceText); //вывод текста ответа на экран

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responceText);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch
            {
                //код 500 может быть установлен и в случаи, когда response не существует (в данном случаи отправка ответа игнорируется 
                if (code != "500")
                    ResponceOutput("500", response); //отправка ответа 
            }
        }
               
        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //изменение и получение URI
        public string _URI
        {
            get { return this.URI; }
            set
            {
                if (value[value.Length - 1] != '/') value += "/";
                if (value[value.Length - 2] == '/') value = value.Remove(value.Length - 1);
                value.Replace(" ", ""); //удаление пробелов 

                this.MessageOutput(true, ChangeMessage("URI", this._URI, value));
                this.URI = value;
                RaisePropertyChanged("_URI");
            }
        }

        //изменение ServerStatus
        public string _ServerStatus
        {
            get
            {
                return Properties.Settings.Default.ServerStatus[_ServerStatusID];
            }
        }
        public int _ServerStatusID
        {
            get
            {
                return this.ServerStatus;
            }
            set
            {
                this.ServerStatus = value;
                RaisePropertyChanged("_ServerStatus");
            }
        }

        //максимальное кол-во ожидающих ответа пользователей
        public int _MaxNumberOfWaitingUsers
        {
            get { return this.MaxNumberOfWaitingUsers; }
            set
            {
                this.MessageOutput(true, ChangeMessage("Максимальное кол-во ожидающих ответа пользователей", this.MaxNumberOfWaitingUsers.ToString(), value.ToString()));
                this.MaxNumberOfWaitingUsers = value;
                RaisePropertyChanged("_MaxNumberOfWaitingUsers");
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