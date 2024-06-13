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
using Barcos.Services;
using Barcos.WPF;

namespace Barcos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserService _userService;
        public MainWindow()
        {
            var databaseService = new DatabaseService("mongodb://localhost:27017", "recu");
            _userService = new UserService(databaseService);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            // Verify login in the database
            if (_userService.AuthenticateUser(username, password))
            {
                MenuWindow menuWindow = new MenuWindow(username);
                menuWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

    }
}