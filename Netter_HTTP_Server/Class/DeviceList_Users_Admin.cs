using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Netter_HTTP_Server
{
    public class DeviceList_Users_Admin : INotifyPropertyChanged
    {
        public List<Device> DeviceList;
        public List<int[]> AllDeviceNumber; //кол-во подключенных когда-либо устройтсв (в пределах одного цикла работы) 

        private string AdminLogin;
        private string AdminPassword;
        public int AdminID;
        private int AllAdminNumber; //кол-во авторизованных когда-либо как админ пользователей (в пределах одного цикла работы) 

        private List<HttpListenerResponse> UserLongPollingResponses;

        private readonly Func<bool, string, bool> MessageOutput; //вывод сообщения на экран

        public DeviceList_Users_Admin(Func<bool, string, bool> messageOutput)
        {
            this.DeviceList = new List<Device>();

            this.MessageOutput = messageOutput;
            
            //кол-во подключенных устройств согласно группе и типу
            this.AllDeviceNumber = new List<int[]>();
            this.AllDeviceNumber.Add(new int[Properties.Settings.Default.TypeIDs_of_SecurityGroup != null ? Properties.Settings.Default.TypeIDs_of_SecurityGroup.Count() : 0]);
            this.AllDeviceNumber.Add(new int[Properties.Settings.Default.TypeIDs_of_ClimateGroup != null ? Properties.Settings.Default.TypeIDs_of_ClimateGroup.Count() : 0]);
            this.AllDeviceNumber.Add(new int[Properties.Settings.Default.TypeIDs_of_LightingGroup != null ? Properties.Settings.Default.TypeIDs_of_LightingGroup.Count() : 0]);

            this.AllAdminNumber = 0;
            this.AdminID = 0;
            this.AdminLogin = Properties.Settings.Default.AdminLogin;
            this.AdminPassword = Properties.Settings.Default.AdminPassword;
        }

        public Device GetDevice(int groupID, int typeID, int deviceNum)
        {
            List<Device> deviceList = this.DeviceList.Where(a => a.GroupID == groupID && a.TypeID == typeID).ToList();
            if (deviceList != null && deviceList.Count >= deviceNum)
                return deviceList[deviceNum - 1];
            else return null;
        }


        //доабвление устройства
        public void Add(Device device)
        {
            this.DeviceList.Add(device); //удаление данного устройства
            RaisePropertyChanged("_DeviceNumber");
        }
        //доабвление пользователя
        public void Add(HttpListenerResponse user)
        {
            this.UserLongPollingResponses.Add(user); //удаление данного устройства
            RaisePropertyChanged("_UserNumber");
        }

        public void Remove(Device device)
        {
            this.DeviceList.Remove(device); //удаление данного устройства
        }

        //авторизвация администратора
        public bool AdminOn (string login, string password)
        {
            if (login == _AdminLogin && password == _AdminPassword)
            {
                this.AllAdminNumber++;
                _AdminID = this.AllAdminNumber;
                return true;
            }
            return false;
        }

        //отключение администратора
        public void AdminOff()
        {
            _AdminID = 0;
        }


        #region Вывод списка устройств с выборкой по типам        
        public List<Device> _Door
        {
            get
            {
                return this.DeviceList.Where(a => a.GroupID == 1 && a.TypeID == 1).ToList();
            }
        }
        public List<Device> _Window
        {
            get
            {
                return this.DeviceList.Where(a => a.GroupID == 1 && a.TypeID == 2).ToList();
            }
        }
        public List<Device> _Lighting
        {
            get
            {
                return this.DeviceList.Where(a => a.GroupID == 3 && a.TypeID == 1).ToList();
            }
        }
        #endregion

        #region привязка поля окна к переменной
        public event PropertyChangedEventHandler PropertyChanged; //событие по обновлению данных
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //привязка поля Login
        public string _AdminLogin
        {
            get { return this.AdminLogin; }
            set
            {
                this.MessageOutput(true, ChangeMessage("Логин", this.AdminLogin, value));
                this.AdminLogin = value;
                Properties.Settings.Default.AdminLogin = value;
                RaisePropertyChanged("_AdminLogin");
            }
        }

        //привязка поля Password
        public string _AdminPassword
        {
            get { return this.AdminPassword; }
            set
            {
                this.MessageOutput(true, ChangeMessage("Пароль", this.AdminPassword, value));
                this.AdminPassword = value;
                Properties.Settings.Default.AdminPassword = value;
                RaisePropertyChanged("_AdminPassword");
            }
        }

        //привядка к полю кол-во пользователей
        public int _UserNumber
        {
            get { return this.UserLongPollingResponses.Count; }
        }

        //привядка к полю кол-во устройств
        public int _DeviceNumber
        {
            get { return this.DeviceList.Count; }
        }

        //привядка к полю кол-во устройств
        public int _AdminID
        {
            get { return this.AdminID; }
            set
            {
                this.AdminID = value;
                RaisePropertyChanged("_AdminID_str");
            }
        }
        public string _AdminID_str
        {
            get { return this.AdminID.ToString("D3"); }
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
