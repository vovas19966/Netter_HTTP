using Netter_HTTP_Devices.UserControls;
using Netter_HTTP_Server;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Netter_HTTP_Devices
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Title.MainTitle = Properties.Settings.Default.ProjectName.ToUpper();
            Title.IDTitle = "Devices".ToUpper();

            for (int i = 0; i < (ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
                ((ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.GroupTitles[i].ToUpper();
        }
        #region Window
        //перемещение окна
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            if (this.WindowState != WindowState.Normal) this.WindowState = WindowState.Normal;
        }
        #endregion

        #region Button CLick
        //нажатие на кнопку (выбор группы устройств)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //если выбранная кнопка активна
            if ((sender as MenuButton).IsActive)
            {
                //изменение статуса выбранной кнопки
                (sender as MenuButton).IsActive ^= true;
                //обнуление frame
                Frame_Types.Navigate(null);
            }
            else
            {
                //сброс меню
                MenuReset();

                //активация выбранной кнопки
                (sender as MenuButton).IsActive = true;
                Frame_Types.Navigate(TypeMenuOpen(Convert.ToInt16((sender as MenuButton).Tag)));
            }
        }

        //закрыть окно
        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //открыть серверное приложение
        private void OpenServer_Button_Click(object sender, RoutedEventArgs e)
        {
            var server = App.Current.Windows.OfType<Netter_HTTP_Server.MainWindow>().FirstOrDefault();

            //если нет открытого серверного приложения
            if (server == null)
            {
                (new Netter_HTTP_Server.MainWindow()).Show();
                (sender as MenuButton).IsActive = true;
            }
            else
            {
                (server as Netter_HTTP_Server.MainWindow).Activate();
            }
        }

        //открыть пользовательское приложение
        private void OpenUser_Button_Click(object sender, RoutedEventArgs e)
        {
            (new Netter_HTTP_User.MainWindow()).Show();
        }
        #endregion

        #region Управление Frame_Type
        //сброс меню
        private void MenuReset()
        {
            //обнуление нажатых кнопок
            ((ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children[0] as MenuButton).IsActive = false;
            ((ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children[1] as MenuButton).IsActive = false;
            ((ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children[2] as MenuButton).IsActive = false;
        }

        //выбор и отобращение Page типов для соответствующей группы
        private Page TypeMenuOpen(int id)
        {
            switch (id)
            {
                case 0:
                    //загрузка страницы. Группа: Безопасность
                    return new Page_Security();
                case 1:
                    //загрузка страницы. Группа: Климат
                    return new Page_Climate();
                case 2:
                    //загрузка страницы. Группа: Освещение
                    return new Page_Lighting();
                default: return null;
            }
        }

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
    }
}
