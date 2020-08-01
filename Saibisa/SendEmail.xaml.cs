using System;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;

namespace Saibisa
{
    /// <summary>
    /// Interaction logic for SendEmail.xaml
    /// </summary>
    public partial class SendEmail : Window
    {
        string _attachmentPath = string.Empty;
        public SendEmail(string attachmentPath, string name)
        {
            InitializeComponent();
            CenterWindowOnScreen();
            MetadataPopulation(attachmentPath, name);
            //InsertIntoDb();
        }

        private void MetadataPopulation(string attachmentPath, string name)
        {
            _attachmentPath = attachmentPath;
            subject.Text = "SAIBISA FOUNDATION : Donation Receipt";
            string greeting = @"Dear {0},

Greetings.

The Trustees, Volunteers and Children of SAIBISA thank you for your prodigious support and noble gesture. Your contribution goes a long way in realizing the objective and fulfilling the dreams of another girl child.

We request your continued support and patronage.

Thank you, again.

Warm Regards,
Team SAIBISA
";
            message.Text = string.Format(greeting, name);
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
        private void SendClicked(object sender, RoutedEventArgs e)
        {
            if (PerformValidation())
                SendEmailWithAttachment();
        }

        public static string TextToHtml(string text)
        {
            text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            return text;
        }

        private bool PerformValidation()
        {
            //bool isEmailValid = Regex.IsMatch(fromEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            //if (!isEmailValid)
            //{
            //    MessageBox.Show("Please enter valid from email id", "Invalid email", MessageBoxButton.OK, MessageBoxImage.Error);
            //    fromEmail.Focus();
            //    return false;
            //}
            //if (password.Password.Length==0)
            //{
            //    MessageBox.Show("Please enter password", "Password empty", MessageBoxButton.OK, MessageBoxImage.Error);
            //    password.Focus();
            //    return false;
            //}
            bool isToEmailValid = Regex.IsMatch(toEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (!isToEmailValid)
            {
                MessageBox.Show("Please enter valid to email id", "Invalid email", MessageBoxButton.OK, MessageBoxImage.Error);
                toEmail.Focus();
                return false;
            }
            if (subject.Text.Length ==0)
            {
                MessageBox.Show("Please enter subject", "Subject Empty", MessageBoxButton.OK, MessageBoxImage.Error);
                subject.Focus();
                return false;
            }
            if (message.Text.Length==0)
            {
                MessageBox.Show("Please enter message", "Message Empty", MessageBoxButton.OK, MessageBoxImage.Error);
                message.Focus();
                return false;
            }
            return true;
        }
        private void SendEmailWithAttachment()
        {
            //Turn on allow less secure app to ON in myaccount.google.com/lesssecureapps
            try
            {
                MailAddress mailfrom = new MailAddress(Login.username);
                MailAddress mailto = new MailAddress(toEmail.Text);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);
                newmsg.IsBodyHtml = true;
                newmsg.Subject = subject.Text;
                newmsg.Body = TextToHtml( message.Text);

                Attachment att = new Attachment(_attachmentPath);
                newmsg.Attachments.Add(att);

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(mailfrom.Address, Login.password)
                };

                smtp.Send(newmsg);
                MessageBox.Show("Email sent successfully", "Email sent",MessageBoxButton.OK,MessageBoxImage.Information);
                InsertIntoDb();
                this.Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("The SMTP server requires a secure connection "))
                {
                    var response = MessageBox.Show("Unable to send e-mail. Please check you credentials. Please ensure 'Less secure app access' is enabled in you google account settings and try again. Would you like to enable now?", "Error sending email", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (response == MessageBoxResult.Yes)
                        System.Diagnostics.Process.Start("https://myaccount.google.com/lesssecureapps");
                }
                Console.WriteLine(ex.Message);
            }
        }

        private void InsertIntoDb()
        {
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB\\saibisa.db";
            string cs = string.Format("URI=file:{0}", dbPath);
            using (var con = new SQLiteConnection(cs, true))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "INSERT INTO tblReceipts(ReceiptNo, ReceiptDate,DonorName,Address, Pan, Amount,PaymentMode, Purpose) " +
                        string.Format(" VALUES('{0}','{1}','{2}', '{3}','{4}',{5},'{6}','{7}')",
                        MainWindow._receiptNo, MainWindow._receiptDate,MainWindow._donorName,MainWindow._address,MainWindow._pan,MainWindow._amount,MainWindow._paymentMode, MainWindow._purpose);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
