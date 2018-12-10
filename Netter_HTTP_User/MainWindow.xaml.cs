using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Netter_HTTP_User
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Class_HTTPClient HTTPClient; //http client
        public static Frame _MainFrame;
        public static bool IsAdmin; //авторизация пользователя как администратора
        public static int AdminID; //ID администратора
        private bool LastMessageIsChangeMessage; //тип последнего выведенного сообщения (true - Изменение:б false - прочее). Необходим для группировки сообщений на изменение

        public MainWindow()
        {
            InitializeComponent();

            IsAdmin = false;
            AdminID = 0;

            Title.MainTitle = Properties.Settings.Default.ProjectName.ToUpper();
            Title.IDTitle = "User".ToUpper();

            HTTPClient = new Class_HTTPClient(MessageOutput);

            _MainFrame = MainFrame;
            _MainFrame.Navigate(new MainPage(MessageOutput));
            DataContext = HTTPClient;
        }


        #region Window
        //перемещение окна
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (this.WindowState != WindowState.Normal) this.WindowState = WindowState.Normal;
        }
        #endregion

        #region Button
        //закрыть окно
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Button_OFF_Click(sender, e);
            Close();
        }

        //очистка поля сообщений 
        private void Button_MessageTextBoxClear_Click(object sender, RoutedEventArgs e)
        {
            (ScrollViewer_Message.MainContent as TextBox).Text = "";
        }

        //авторизация/отключение администратора
        private void Button_AdminAuthorizationUnauthorization_Click(object sender, RoutedEventArgs e)
        {
            ConnectionCheck();
        }

        //На главную
        private void Button_ToMain_Click(object sender, RoutedEventArgs e)
        {
            _MainFrame.Navigate(new MainPage(MessageOutput));
        }

        #region ON OFF
        //ВКЛ/ВЫКЛ
        private void Button_ON_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Button_OFF_Click(object sender, RoutedEventArgs e)
        {
        }

        //визуальное отображение работы с сервером
        private void ONOFF_VisualDisplay(bool onoff)
        {
        }
        #endregion
        #endregion

        #region TextBox
        //проверка вводимого текста
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length > Properties.Settings.Default.LengthLong) //ограничение по длине
                e.Handled = true;
        }
        private void URI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthShot) //ограничение по длине
            {
                for (int i = 0; i < e.Text.Length && e.Handled != true; i++)
                    if (e.Text[i] == 43 || e.Text[i] == 47 || e.Text[i] == 61 || (e.Text[i] >= 'а' && e.Text[i] <= 'я') || (e.Text[i] >= 'А' && e.Text[i] <= 'Я')) //запрет на ввод символов + = /
                        e.Handled = true;
            }
            else e.Handled = true;
        }
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((sender as TextBox).Text.Length + e.Text.Length <= Properties.Settings.Default.LengthShot) //ограничение по длине
            {
                for (int i = 0; i < e.Text.Length && e.Handled != true; i++)
                    if (e.Text[i] < 48 || e.Text[i] > 57) //запрет символов, отличных от цифр
                        e.Handled = true;
            }
            else e.Handled = true;
        }
        #endregion

        #region Управление Frame_Type

        //Изменение frame
        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Frame frame = (sender as Frame);

            if (frame.NavigationService.RemoveBackEntry() != null)
            {
                frame.RemoveBackEntry();
                frame.NavigationService.RemoveBackEntry();
            }
        }
        #endregion


        //вывод сообщения
        private void MessageOutput(int separator, string str)
        {
            //проверка длины поля сообщений 
            if ((ScrollViewer_Message.MainContent as TextBox).LineCount > Properties.Settings.Default.MaxCountMessageLine) Button_MessageTextBoxClear_Click(new object(), new RoutedEventArgs());
            //выводить только в случаи наличия сообщения
            if (str != "")
            {
                //является ли текущее сообщение сообщение на Изменение
                bool thisMessageIsChangeMessage = str.Contains(Properties.Resources.ChangeOption);

                //если имеются уже выведенные на экран сообщения               
                if ((ScrollViewer_Message.MainContent as TextBox).Text != "")
                {
                    //если последнее и текущее сообщения не являются сообщениями на Изменение
                    if (!this.LastMessageIsChangeMessage || !thisMessageIsChangeMessage)
                        //вывод заданного числа enter'оф
                        for (int i = 0; i < separator; i++)
                            (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
                    //простой переход в следующую строку
                    else (ScrollViewer_Message.MainContent as TextBox).Text += "\n";
                }
                //вывод сообщения
                (ScrollViewer_Message.MainContent as TextBox).Text += (str);

                //перевод scrollBar в нижнее положение
                ScrollViewer_Message.ScrollToEnd();

                this.LastMessageIsChangeMessage = thisMessageIsChangeMessage;
            }
        }
        
        #region Connection Checked
        //проверка соединения (GET)
        private void ConnectionCheck()
        {
            //вывод сообщения о выполняемом запросе
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.ConnectionCheckRequest + GetNowTime);
            MainWindow.HTTPClient.GetRequest(0, "", ConnectionCheck_ResponseHandler);
        }

        private bool ConnectionCheck_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.ConnectionCheckRequest + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                if (!IsAdmin) AdminAuthorizationConnection();
                else UnadminAuthorizationConnection();
            }
            else
            {
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region AdminAuthorization Connection
        //запрос кол-ва подключенных к серверу устройств
        private void AdminAuthorizationConnection()
        {
            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.AdminAuthorization + GetNowTime);

            List<KeyValuePair<string, string>> loginPassword = new List<KeyValuePair<string, string>>();
                        
            loginPassword.Add(new KeyValuePair<string, string>("login", (((ScrollViewer_ServerInfo.MainContent as Grid).Children[3] as Border).Child as TextBox).Text));
            loginPassword.Add(new KeyValuePair<string, string>("password", (((ScrollViewer_ServerInfo.MainContent as Grid).Children[5] as Border).Child as TextBox).Text));

            MainWindow.HTTPClient.PutRequest(0, "admin/", loginPassword, AdminAuthorizationConnection_ResponseHandler);
        }

        private bool AdminAuthorizationConnection_ResponseHandler(bool code, string response)
        {
            MessageOutput(1, Properties.Resources.AdminAuthorization + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
                Button_Admin.IsActive = true;

                IsAdmin = true;
                AdminID = Convert.ToInt32(response);

                for (int i = 6; i < 14; i++)
                {
                    ((ScrollViewer_ServerInfo.MainContent as Grid).Children[i] as Label).Visibility = Visibility.Visible;
                    ((ScrollViewer_ServerInfo.MainContent as Grid).Children[++i] as Border).Visibility = Visibility.Visible;
                }
                ScrollViewer_Message.Visibility = Visibility.Visible;
                Button_ClearMessageBox.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        #region UnadminAuthorization Connection
        //запрос кол-ва подключенных к серверу устройств
        private void UnadminAuthorizationConnection()
        {
            IsAdmin = false;
            AdminID = 0;
            for (int i = 6; i < 14; i++)
            {
                ((ScrollViewer_ServerInfo.MainContent as Grid).Children[i] as Label).Visibility = Visibility.Collapsed;
                ((ScrollViewer_ServerInfo.MainContent as Grid).Children[++i] as Border).Visibility = Visibility.Collapsed;
            }
            ScrollViewer_Message.Visibility = Visibility.Collapsed;
            Button_ClearMessageBox.Visibility = Visibility.Collapsed;

            MessageOutput(2, Properties.Resources.Processing + Properties.Resources.AdminAuthorization + GetNowTime);
            
            MainWindow.HTTPClient.DeleteRequest(0, "admin/", UnadminAuthorizationConnection_ResponseHandler);
        }

        private bool UnadminAuthorizationConnection_ResponseHandler(bool code, string response)
        {
            Button_Admin.IsActive = false;

            MessageOutput(1, Properties.Resources.AdminAuthorization + Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            if (code)
            {
            }
            else
            {
                MessageBox.Show(Properties.Resources.ResponseReceived + response + ". " + GetNowTime);
            }
            return true;
        }
        #endregion

        //возвращение текущего времени
        private string GetNowTime
        {
            get { return ("Время: " + DateTime.Now.ToLongTimeString()); }
        }
    }
}
