using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_Server
{
    public class Request
    {
        public bool IsErrorRequest; //ошибка при расшировке запроса
        public List<int> ID; //ID устройства
        public int DevicePage; //номер запрашиваемого устройства из списка (с учетом группы и типа)
        public bool IsAdminRequest; //запрос от администратора  
        public bool IsLongPollingRequest; //запрос от администратора  

        public Request(string str)
        {
            this.IsErrorRequest = false;
            this.ID = new List<int>();
            this.IsAdminRequest = false;
            this.IsLongPollingRequest = false;
            this.DevicePage = -1;

            //удаление лишних символов
            while (str[0] == '/')
                str = str.Remove(0, 1);
            while (str[str.Length - 1] == '/')
                str = str.Remove(str.Length - 1, 1);

            string[] request = str.Split('/'); //разделение запроса по символу "/"

            foreach (string item in request) //обработка каждой части запроса
                try //расшифровка ID
                {
                    int i = Convert.ToInt32(item);
                    this.ID.Add(i);
                }
                catch //дальнейшая обработка
                {
                    try
                    {
                        string[] KeyValue = item.Split('=');
                        switch (KeyValue[0])
                        {
                            case "pg": //запрос информации об устройстве из списка
                                this.DevicePage = Convert.ToInt32(KeyValue[1]);
                                break;
                            case "lp": //long-polling-запрос
                                this.IsLongPollingRequest = true;
                                break;
                            case "admin": //запрос от имени администратора
                                this.IsAdminRequest = true;
                                break;
                            default:
                                this.IsErrorRequest = true;
                                break;
                        }
                    }
                    catch { this.IsErrorRequest = true; }
                }

            if (this.IsErrorRequest)
            {
                this.ID = null;
                this.IsAdminRequest = false;
                this.IsLongPollingRequest = false;
                this.DevicePage = -1;
            }

        }

        ////расшифровка ID устройства из строки RawURL
        //private int[] IDDecoder(string str)

        //    //ID: 0-group, 1-type, 2-device
        //    string[] s_id = str.Split('/');
        //    int[] i_id = new int[s_id.Length];
        //    //если в строке более трех сегментов, разделенных /
        //    //получение id
        //    try
        //    {
        //        for (int i = 0; i < s_id.Length; i++)
        //        {
        //            i_id[i] = Convert.ToInt32(s_id[i]);
        //        }
        //        return i_id;
        //    }
        //    catch { return null; } //если возникли проблемы при разшифровке
        //}
    }
}
