using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Netter_HTTP_Devices
{
    public class Class_Device : INotifyPropertyChanged
    {
        public bool DeviceStatus { get; set; } //статус устройства
        public int GroupID { get; } //Номер группы устройств
        public int TypeID { get; } //Номер типа устройств
        public int DeviceID { get; set; } //Номер устройства
        public string Name { get; set; } //Имя устройства
        public string Location { get; set; } //Имя устройства

        public Func<int, string, bool> MessageOutput; //вывод сообщения на экран

        #region Инициализация класса
        public Class_Device(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            )
        {
            this.GroupID = groupID;
            this.TypeID = typeID;
            this.DeviceID = 0;
            
            this.Name = GetDefaultName;
            this.Location = Properties.Settings.Default.DefaultLocation;
        }

        public Class_Device(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            , int deviceID //номер устройства
            )
        {
            this.GroupID = groupID;
            this.TypeID = typeID;
            this.DeviceID = deviceID;


            this.Name = GetDefaultName;
            this.Location = Properties.Settings.Default.DefaultLocation;
        }
        #endregion

        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            // Если кто-то на него подписан, то вызывем его
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        //изменение свойства Name
        public string _Name
        {
            get { return this.Name; }
            set
            {
                value = value.Trim();
                this.MessageOutput(2, ChangeMessage("Название", this._Name, value));
                this.Name = value;
                // Сообщаем всем, кто подписан на событие PropertyChanged, что поле изменилось
                RaisePropertyChanged("_Name");
            }
        }

        //изменение свойства Location
        public string _Location
        {
            get { return this.Location; }
            set
            {
                value = value.Trim();
                this.MessageOutput(2, ChangeMessage("Расположение", this._Location, value));
                this.Location = value;
                // Сообщаем всем, кто подписан на событие PropertyChanged, что поле изменилось
                RaisePropertyChanged("_Location");
            }
        }

        //изменение свойства ID
        //вывод полного ID устройтсва
        public string _ID
        {
            get { return GetID; }
        }
        //изменение ID устрйоства (без учета ID группы и ID типа устройства)
        public int _DeviceID
        {
            get { return this.DeviceID; }
            set
            {
                string oldID = this._ID;
                // Устанавливаем новое значение
                this.DeviceID = value;


                this.MessageOutput(2, ChangeMessage("ID", oldID, this._ID));

                // Сообщаем всем, кто подписан на событие PropertyChanged, что поле изменилось
                RaisePropertyChanged("_ID");
                RaisePropertyChanged("_DeviceID");
            }
        }
        #endregion




        #region GET/SET
        //возвращение ID (строка)
        public string GetID
        {
            get { return this.GroupID.ToString("D2") + "." + this.TypeID.ToString("D2") + "." + this.DeviceID.ToString("D3"); }
        }
        public string GetID_URI
        {
            get { return this.GroupID.ToString("D2") + "/" + this.TypeID.ToString("D2") + "/" + this.DeviceID.ToString("D3"); }
        }

        //возвращение Group и Type ID (строка)
        public string GetGroupAndTypeID
        {
            get { return this.GroupID.ToString("D2") + "." + this.TypeID.ToString("D2"); }
        }

        #region Virtual Methods
        public virtual string GetDefaultName { get { return "Device #" + GetID; } }

        //возвращает список типа Ключ-Значение всех параметров устройства
        public virtual List<KeyValuePair<string, string>> GetOptions
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();
                options.AddRange(GetRequestOptions_NameAndLocation);
                return options;
            }
        }
        #endregion
        #endregion
        
        //возвращает список типа Ключ-Значение стандартных (имя, расположение) параметров устройства
        protected List<KeyValuePair<string, string>> GetRequestOptions_NameAndLocation
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();
                options.Add(new KeyValuePair<string, string>("name", this._Name));
                options.Add(new KeyValuePair<string, string>("location", this._Location));
                return options;
            }
        }
        
        //конструктор сообщения об изменении параметра
        protected string ChangeMessage(string optionName, string oldValue, string newValue)
        {
            if (oldValue != newValue)
                return (Properties.Resources.ChangeOption + optionName + ": " + oldValue + " -> " + newValue);
            return "";
        }

        //конструктор сообщения о некорректном значении параметра
        protected string IncorrectMessage(string optionName, string value)
        {
            return (Properties.Resources.IncorrectValue + optionName + ": " + value);
        }

        //обработка параметор, полученных от пользователя
        public virtual void OptionsProcessing(List<KeyValuePair<string, string>> options)
        {
        }
    }
}
