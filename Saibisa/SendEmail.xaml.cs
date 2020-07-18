using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Saibisa
{
    /// <summary>
    /// Interaction logic for SendEmail.xaml
    /// </summary>
    public partial class SendEmail : Window
    {
        string _attachmentPath = string.Empty;
        public SendEmail(string attachmentPath)
        {
            InitializeComponent();
            CenterWindowOnScreen();
            _attachmentPath = attachmentPath;
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
        private bool PerformValidation()
        {
            bool isEmailValid = Regex.IsMatch(fromEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (!isEmailValid)
            {
                MessageBox.Show("Please enter valid from email id", "Invalid email", MessageBoxButton.OK, MessageBoxImage.Error);
                fromEmail.Focus();
                return false;
            }
            if (password.Password.Length==0)
            {
                MessageBox.Show("Please enter password", "Password empty", MessageBoxButton.OK, MessageBoxImage.Error);
                password.Focus();
                return false;
            }
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
                MailAddress mailfrom = new MailAddress(fromEmail.Text);
                MailAddress mailto = new MailAddress(toEmail.Text);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);

                newmsg.Subject = subject.Text;
                newmsg.Body = message.Text;

                Attachment att = new Attachment(_attachmentPath);
                newmsg.Attachments.Add(att);

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(mailfrom.Address, password.Password)
                };

                smtp.Send(newmsg);
                MessageBox.Show("Email sent successfully", "Email sent",MessageBoxButton.OK,MessageBoxImage.Information);
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

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
