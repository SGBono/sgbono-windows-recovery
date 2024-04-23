using iNKORE.UI.WPF.Modern;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml.Permissions;

namespace beforewindeploy_custom_recovery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

            LoadingScreen.Visibility = Visibility.Visible;
            this.Loaded += (object sender, RoutedEventArgs e) =>
            {
                ComponentSelection componentSelection = new ComponentSelection();
                frame.Visibility = Visibility.Visible;
                frame.Content = componentSelection;
            };
        }
    }
}