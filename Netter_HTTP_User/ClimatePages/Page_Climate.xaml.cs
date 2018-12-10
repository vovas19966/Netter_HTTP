using Netter_HTTP_User.CustomControls;
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
    /// Логика взаимодействия для Page_Climate.xaml
    /// </summary>
    public partial class Page_Climate : Page
    {
        private int GroupIndex = 1;
        private int TypeIndex = 0;

        public Page_Climate()
        {
            InitializeComponent();

            MenuReset();

            for (int i = 0; i < (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
            {
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.TypeTitles_of_ClimateGroup[i].ToUpper();
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).Click += Button_Click;
            }
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
                Frame_DeviceList.Navigate(null);
            }
            else
            {
                //сброс меню
                MenuReset();

                //активация выбранной кнопки
                (sender as MenuButton).IsActive = true;
                Frame_DeviceList.Navigate(TypeMenuOpen());
            }
        }
        #endregion



        #region Управление Frame_Type
        //сброс меню
        private void MenuReset()
        {
            //обнуление нажатых кнопок
            foreach (object button in (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children)
                (button as MenuButton).IsActive = false;
        }

        //выбор и отобращение Page типов для соответствующей группы
        private Page TypeMenuOpen()
        {
            return null;
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