using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Netter_HTTP_Server
{
    public class Device : INotifyPropertyChanged
    {
        public int GroupID { get; } //Номер группы устройств
        private int GroupIndex; //индекс группы
        public int TypeID { get; } //Номер типа устройств
        private int TypeIndex; //индекс типа 
        public int DeviceID { get; set; } //Номер устройства

        public HttpListenerResponse LongPollingResponse; //ответ на Long-Polling-запрос
        public List<HttpListenerResponse> UserResponses; //ответы на пользовательские запросы

        private int OptionNumber; //кол-во параметров устройства
        private List<KeyValue> Options; //параметры устройства

        public DispatcherTimer DeleteTimer { get; set; } //таймер ожидания запроса
        private TimeSpan DeleteTimeSpan; //время ожидания запроса
        private string UpdateTime; //время последнего обновления параметров

        private Func<Device, bool> DeviceDelete; //метод удаления устройства

        #region Инициализация класса
        public Device(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            , List<int[]> deviceNum //кол-во подключенных устройств (получение уникального номера
            , TimeSpan deleteTime //время ожидания запроса
            , Func<Device, bool> deviceDelete //метод удаления устройства
            )
        {
            this.GroupID = groupID;
            this.TypeID = typeID;

            IndexDefinition();
            if (deviceNum!=null)
            this.DeviceID = ++deviceNum[this.GroupIndex][this.TypeIndex];

            this.LongPollingResponse = null;
            this.UserResponses = new List<HttpListenerResponse>();

            //создания списка параметров-заглушек
            this.Options = new List<KeyValue>();
            for (int i = 0; i < this.OptionNumber; i++)
                Options.Add(new KeyValue());
            
            this.UpdateTime = "";
            this.DeleteTimeSpan = deleteTime;

            this.DeviceDelete = deviceDelete;

            //создание таймера
            this.DeleteTimer = new DispatcherTimer();
            this.DeleteTimer.Interval = _DeleteTimeSpan;
            this.DeleteTimer.Tick += DeleteTime_Tick;
            this.DeleteTimer.Start();
        }

        ~Device()
        {
            this.DeleteTimer.Stop();
            this.LongPollingResponse = null;
            this.UserResponses = null;
            for (int i = 0; i < this.Options.Count; i++)
                this.Options[i] = null;
        }
        #endregion

        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
               
        //изменение свойства ID
        //вывод полного ID устройтсва
        public string _ID
        {
            get { return this.GroupID.ToString("D2") + "." + this.TypeID.ToString("D2") + "." + this.DeviceID.ToString("D3"); }
        }
        
        //параметры устройства
        public List<KeyValue> _Options
        {
            get { return this.Options; }
            set
            {
                this.Options = value;
                RaisePropertyChanged("_Options");
            }
        }

        //время последнего обновления параметров устройства
        public string _UpdateTime
        {
            get { return this.UpdateTime; }
            set
            {
                this.UpdateTime = DateTime.Now.ToLongTimeString();
                RaisePropertyChanged("_UpdateTime");
            }
        }

        //время ожидания запроса
        public TimeSpan _DeleteTimeSpan
        {
            get { return this.DeleteTimeSpan; }
            set
            {
                this.DeleteTimeSpan = new TimeSpan(Properties.Settings.Default.TimeOutMultiplication * value.Ticks);
                this.DeleteTimer.Interval = this.DeleteTimeSpan;
                RaisePropertyChanged("_DeleteTimeSpan");
            }
        }
        #endregion

        //определение индекса группы и типа устройства
        private void IndexDefinition()
        {
            this.GroupIndex = Properties.Settings.Default.GroupIDs.ToList().IndexOf(this.GroupID);
            switch (this.GroupIndex)
            {
                case 0:
                    this.TypeIndex = Properties.Settings.Default.TypeIDs_of_SecurityGroup.ToList().IndexOf(this.TypeID);
                    try
                    {
                        this.OptionNumber = Properties.Settings.Default.OptionNumber_of_SecurityGroup[this.TypeIndex];
                    }
                    catch
                    {
                        this.OptionNumber = 2;
                    }
                    break;
                case 1:
                    this.TypeIndex = Properties.Settings.Default.TypeIDs_of_ClimateGroup.ToList().IndexOf(this.TypeID);
                    try
                    {
                        this.OptionNumber = Properties.Settings.Default.OptionNumber_of_ClimateGroup[this.TypeIndex];
                    }
                    catch
                    {
                        this.OptionNumber = 2;
                    }
                    break;
                case 2:
                    this.TypeIndex = Properties.Settings.Default.TypeIDs_of_LightingGroup.ToList().IndexOf(this.TypeID);
                    try
                    {
                        this.OptionNumber = Properties.Settings.Default.OptionNumber_of_LightingGroup[this.TypeIndex];
                    }
                    catch
                    {
                        this.OptionNumber = 2;
                    }
                    break;
            }
        }

        //обновление параметров устройств
        public void OptionsUpdate(List<KeyValue> options)
        {
            this.DeleteTimer.Stop();

            this._UpdateTime = ""; //настоящее время определяется в методе set

            if (this.Options[0]._Key!="")
                foreach (KeyValue option in options)
                {
                    int index = this.Options.FindIndex(a => a._Key == option._Key);
                    this.Options[index]._Value = option._Value;

                    //если был изменен таймер ожидания status-зарпоса
                    if (option._Key == Properties.Settings.Default.DeleteTimerTitle)
                    {
                        _DeleteTimeSpan = TimeSpan.Parse(option._Value);
                    }
                }
            else
                for (int i = 0; i < this.Options.Count; i++)
                {
                    this.Options[i]._Key = options[i]._Key;
                    this.Options[i]._Value = options[i]._Value;

                    //если был изменен таймер ожидания status-зарпоса
                    if (options[i]._Key == Properties.Settings.Default.DeleteTimerTitle)
                    {
                        _DeleteTimeSpan = TimeSpan.Parse(options[i]._Value);
                    }
                }

            this.DeleteTimer.Start(); //перезагрузка таймера удаления устройства
        }

        //удаление данного устройства
        private void DeleteTime_Tick(object sender, System.EventArgs e)
        {
            this.DeleteTimer.Stop();
            this.DeviceDelete(this);
        }

        //запрос основной информации об устройстве (ID, имя, расположение
        public string GetMainDeviceInfo
        {
            get
            {
                string str = "";
                str = "id=" + _ID + "&";
                KeyValue keyValue = this.Options.FirstOrDefault(a => a._Key == "name");
                if (keyValue != null)
                    str += "name=" + keyValue._Value + "&";
                keyValue = this.Options.FirstOrDefault(a => a._Key == "location");
                if (keyValue != null)
                    str += "location=" + keyValue._Value;
                return str;
            }
        }

        //запрос всей пользовательской информации об устройстве
        public string GetDeviceInfo
        {
            get
            {
                string str = "id=" + _ID;

                foreach (KeyValue option in this.Options)
                {
                    //исключение параметров, доступных только для админиматратора
                    bool isNotAdminOption = true;
                    for (int i = 0; i < Properties.Settings.Default.AdminDeviceOptions.Count; i++)
                        if (option._Key == Properties.Settings.Default.AdminDeviceOptions[i]) isNotAdminOption = false;

                    if (isNotAdminOption)
                        str += "&" + option.ToString;
                }

                return str;
            }
        }

        //запрос всей пользовательской информации об устройстве
        public string GetAdminDeviceInfo
        {
            get
            {
                string str = "id=" + _ID;

                foreach (KeyValue option in this.Options)
                {
                    str += "&" + option.ToString;
                }

                return str;
            }
        }
    }
}
