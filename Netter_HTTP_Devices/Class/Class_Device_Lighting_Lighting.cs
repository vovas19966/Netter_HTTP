using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_Devices
{
    class Class_Device_Lighting_Lighting : Class_Device
    {
        public int Brightness { get; set; } //положение дверь

        //инициализация
        public Class_Device_Lighting_Lighting(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            )
            : base(groupID, typeID)
        {
            this.Brightness = 0;
        }
        //инициализация
        public Class_Device_Lighting_Lighting(
            int groupID //номер группы устройств
            , int typeID //номер типа устройств
            , int deviceID //номер устройства
            )
            : base(groupID, typeID, deviceID)
        {
            this.Brightness = 0;
        }


        //изменение свойства Name
        public int _Brightness
        {
            get { return this.Brightness; }
            set
            {
                this.MessageOutput(2, ChangeMessage("Яркость", (this.Brightness * 10).ToString(), (value * 10).ToString()));
                this.Brightness = value;
                // Сообщаем всем, кто подписан на событие PropertyChanged, что поле изменилось
                RaisePropertyChanged("_Brightness");
            }
        }

        //возвращает Name устройста по умолчанию
        public override string GetDefaultName
        {
            //~ "Дверь #01.01.000"
            get { return "Lighting " + GetID; }
        }

        //возвращает список типа Ключ-Значение всех параметров устройства
        public override List<KeyValuePair<string, string>> GetOptions
        {
            get
            {
                List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();
                options.AddRange(GetRequestOptions_NameAndLocation);
                options.Add(new KeyValuePair<string, string>("brightness", this._Brightness.ToString()));
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
                    case "brightness":
                        try
                        {
                            _Brightness = Convert.ToInt32(option.Value);
                        }
                        catch
                        {
                            this.MessageOutput(2, IncorrectMessage("Яркость", option.Value));
                        }
                        break;
                }
            }
        }
    }
}
