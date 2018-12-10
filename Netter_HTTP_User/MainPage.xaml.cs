using Netter_HTTP_User.CustomControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Netter_HTTP_User
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly Action<int, string> MessageOutput; //метод для вывод сообщений

        public MainPage(Action<int, string> messageOutput)
        {
            InitializeComponent();

            this.MessageOutput = messageOutput;

            MenuReset();

            for (int i = 0; i < (ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
                ((ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.GroupTitles[i].ToUpper();
        }


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
        #endregion



        #region Управление Frame_Type
        //сброс меню
        private void MenuReset()
        {
            //обнуление нажатых кнопок
            foreach (object button in (ScrollViewer_Groups.MainContent as System.Windows.Controls.StackPanel).Children)
                (button as MenuButton).IsActive = false;
        }

        //выбор и отобращение Page типов для соответствующей группы
        private Page TypeMenuOpen(int id)
        {
            switch (id)
            {
                case 0:
                    //загрузка страницы. Группа: Безопасность
                    return new Page_Security(MessageOutput);
                case 1:
                    //загрузка страницы. Группа: Климат
                    return new Page_Climate();
                case 2:
                    //загрузка страницы. Группа: Освещение
                    return new Page_Lighting(MessageOutput);
                default: return null;
            }
        }

        //Изменение frame
        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Frame frame = (sender as Frame);

            if (frame.NavigationService.RemoveBackEntry() != null)
            {
                //frame.RemoveBackEntry();
                frame.NavigationService.RemoveBackEntry();
            }
        }
        #endregion
    }
}
