using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_Devices
{
    class Class_Device_Security_Window : Class_Device
    {
        public int Position { get; set; } //положение дверь (0 - открыто, 1 - закрыто, 2 - заблокировано)

        //инициализация
        public Class_Device_Security_Window(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            )
            : base(groupID, typeID)
        {
            this.Position = 2;
        }
        //инициализация
        public Class_Device_Security_Window(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            , int deviceID //номер устройства
            )
            : base(groupID, typeID, deviceID)
        {
            this.Position = 2;
        }


        //изменение свойства Name
        public int _Position
        {
            get { return this.Position; }
            set
            {
                this.MessageOutput(2, ChangeMessage("Положение", Properties.Settings.Default.Door_Position[this._Position], Properties.Settings.Default.Door_Position[value]));
                this.Position = value;
                // Сообщаем всем, кто подписан на событие PropertyChanged, что поле изменилось
                RaisePropertyChanged("_Position");
            }
        }

        //возвращает Name устройста по умолчанию
        public override string GetDefaultName
        {
            //~ "Window #01.01.000"
            get { return "Window " + GetID; }
        }

        //возвращает список типа Ключ-Значение всех параметров устройства
        public override List<KeyValuePair<string, string>> GetOptions
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();
                options.AddRange(GetRequestOptions_NameAndLocation);
                options.Add(new KeyValuePair<string, string>("position", this._Position.ToString()));
                return options;
            }
        }

        //обработка параметор, полученных от пользователя
        public override void OptionsProcessing(List<KeyValuePair<string, string>> options)
        {
            foreach (KeyValuePair<string, string> option in options)
            {
                switch (option.Key)
                {
                    case "name":
                        _Name = option.Value;
                        break;
                    case "location":
                        _Location = option.Value;
                        break;
                    case "position":
                        try
                        {
                            _Position = Convert.ToInt32(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Положение", option.Value));
                        }
                        break;
                }
            }
        }
    }
}
