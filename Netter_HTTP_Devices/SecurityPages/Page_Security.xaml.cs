using Netter_HTTP_Devices.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace Netter_HTTP_Devices
{
    /// <summary>
    /// Логика взаимодействия для Page_Security.xaml
    /// </summary>
    public partial class Page_Security : Page
    {
        private int GroupIndex = 0;

        public Page_Security()
        {
            InitializeComponent();

            for (int i = 0; i < (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.TypeTitles_of_SecurityGroup[i].ToUpper();
        }

        private void Button_Door_Click(object sender, RoutedEventArgs e)
        {
            int typeIndex = 0;

            //подключение страницы устройства
            Page_Security_Door device = new Page_Security_Door();

            new Window_Device(
                Properties.Settings.Default.GroupTitles[this.GroupIndex], //название группы устройств
                Properties.Settings.Default.TypeTitles_of_SecurityGroup[typeIndex], //название типа устройств
                device, //стртаница устройства
                (Class_Device)device.InfoCreate(Properties.Settings.Default.GroupIDs[this.GroupIndex], Properties.Settings.Default.TypeIDs_of_SecurityGroup[typeIndex]), //информация об устройстве
                (sender as MenuButton).First_Color,
                (sender as MenuButton).Second_Color
                ).Show();
        }

        private void Button_Window_Click(object sender, RoutedEventArgs e)
        {
            int typeIndex = 1;

            //подключение страницы устройства
            Page_Security_Window device = new Page_Security_Window();

            new Window_Device(
                Properties.Settings.Default.GroupTitles[this.GroupIndex], //название группы устройств
                Properties.Settings.Default.TypeTitles_of_SecurityGroup[typeIndex], //название типа устройств
                device, //стртаница устройства
                (Class_Device)device.InfoCreate(Properties.Settings.Default.GroupIDs[this.GroupIndex], Properties.Settings.Default.TypeIDs_of_SecurityGroup[typeIndex]), //информация об устройстве
                (sender as MenuButton).First_Color,
                (sender as MenuButton).Second_Color
                ).Show();
        }

        private void Button_Floor_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
