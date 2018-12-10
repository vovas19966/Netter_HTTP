using Netter_HTTP_Devices.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace Netter_HTTP_Devices
{
    /// <summary>
    /// Логика взаимодействия для Page_Lighting.xaml
    /// </summary>
    public partial class Page_Lighting : Page
    {
        private int GroupIndex = 2;

        public Page_Lighting()
        {
            InitializeComponent();

            for (int i = 0; i < (ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children.Count; i++)
                ((ScrollViewer_Types.MainContent as System.Windows.Controls.StackPanel).Children[i] as MenuButton).TitleContent = Properties.Settings.Default.TypeTitles_of_LightingGroup[i].ToUpper();
        }

        private void Button_Lighting_Click(object sender, RoutedEventArgs e)
        {
            int typeIndex = 0;

            //подключение страницы устройства
            Page_Lighting_Lighting device = new Page_Lighting_Lighting();

            new Window_Device(
                Properties.Settings.Default.GroupTitles[this.GroupIndex], //название группы устройств
                Properties.Settings.Default.TypeTitles_of_LightingGroup[typeIndex], //название типа устройств
                device, //стртаница устройства
                (Class_Device)device.InfoCreate(Properties.Settings.Default.GroupIDs[this.GroupIndex], Properties.Settings.Default.TypeIDs_of_LightingGroup[typeIndex]), //информация об устройстве
                (sender as MenuButton).First_Color,
                (sender as MenuButton).Second_Color
                ).Show();
        }
    }
}