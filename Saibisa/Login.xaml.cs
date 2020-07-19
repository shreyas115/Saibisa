using System;
using System.Windows;

namespace Saibisa
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public static string username = string.Empty;
        public static string password = string.Empty;
        public Login()
        {
            InitializeComponent();
            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
        private void LoginClicked(object sender, RoutedEventArgs e)
        {

            var isValidLogin = ValidateCredentials(txtUsername.Text, txtPassword.Password, "smtp.gmail.com", 587, true);
            if (isValidLogin)
            {
                username = txtUsername.Text;
                password = txtPassword.Password;
                new MainWindow().Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid login credentials!!!", "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public static bool ValidateCredentials(string username, string password, string server, int port, bool certificationValidation)
        {
            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => certificationValidation;
                        client.Connect(server, port, false);
                        client.Authenticate(username, password);
                        client.Disconnect(true);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("ValidateCredentials Exception: {0}", ex.Message));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(string.Format("ValidateCredentials Exception: {0}", ex.Message));
            }

            return false;
        }
        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
