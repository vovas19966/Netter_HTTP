using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_Server
{
    public class KeyValue : INotifyPropertyChanged
    {
        private string Key;
        private string Value;

        public KeyValue()
        {
            this._Key = "";
            this._Value = "";
        }
        public KeyValue(string key, string value)
        {
            this._Key = key;
            this._Value = value;
        }

        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string _Key
        {
            get { return this.Key; }
            set
            {
                this.Key = value;
                RaisePropertyChanged("_Key");
            }
        }
        //вывод параметров устройства
        public string _Value
        {
            get { return this.Value; }
            set
            {
                this.Value = value;
                RaisePropertyChanged("_Value");
            }
        }
        #endregion

        public new string ToString
        {
            get
            {
                return _Key + "=" + _Value;
            }
        }
    }
}
