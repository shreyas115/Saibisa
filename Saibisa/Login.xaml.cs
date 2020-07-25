using System;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
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
            InitDb();
        }

        private void InitDb()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB");

            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB\\saibisa.db";
            string cs = string.Format("URI=file:{0}", dbPath);
            if (!File.Exists(dbPath))
            {
                System.Data.SQLite.SQLiteConnection.CreateFile(dbPath);
                using (var con = new SQLiteConnection(cs, true))
                {
                    con.Open();

                    using (var cmd = new SQLiteCommand(con))
                    {
                        //cmd.Connection = con;
                        //cmd.CommandText = "DROP TABLE IF EXISTS cars";
                        //cmd.ExecuteNonQuery();

                        cmd.CommandText = @"CREATE TABLE tblReceipts(id INTEGER PRIMARY KEY,
                                                                    ReceiptNo   TEXT, 
                                                                    ReceiptDate TEXT,
                                                                    DonorName   TEXT,
                                                                    Address     TEXT,   
                                                                    Pan         TEXT,
                                                                    Amount      REAL,
                                                                    PaymentMode TEXT,
                                                                    Purpose     TEXT)";
                        cmd.ExecuteNonQuery();


                    }
                }
            }
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
        private bool PerformValidation()
        {
            bool isToEmailValid = Regex.IsMatch(txtUsername.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (!isToEmailValid)
            {
                MessageBox.Show("Please enter valid to email id", "Invalid email", MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Focus();
                return false;
            }
            if (txtPassword.Password.Length == 0)
            {
                MessageBox.Show("Please enter password", "Password empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return false;
            }
            return true;
        }
        private void LoginClicked(object sender, RoutedEventArgs e)
        {
            if (PerformValidation())
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
                    var response = MessageBox.Show("Unable to validate. Please check you credentials. Also ensure 'Less secure app access' is enabled in your google account settings. Would you like to enable now?", "Error sending email", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (response == MessageBoxResult.Yes)
                        System.Diagnostics.Process.Start("https://myaccount.google.com/lesssecureapps");
                }
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
                        //return true;
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
