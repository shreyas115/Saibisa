using iTextSharp.text.pdf;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Saibisa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool _isTwoColumn = false;
        bool _isAdobeInstalled = false; 
        public MainWindow()
        {
            InitializeComponent();
            dtDate.SelectedDate = DateTime.Now;
            cbTowards.Items.Add("Donation");
            DeleteOldFiles();
            CheckIfAdobeInstalled();
            if (!_isAdobeInstalled)
            {
                this.Width = 600;
            }
            CenterWindowOnScreen();
            txtReceipt.Focus();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize == e.NewSize)
                return;

            var w = SystemParameters.PrimaryScreenWidth;
            var h = SystemParameters.PrimaryScreenHeight;

            this.Left = (w - e.NewSize.Width) / 2;
            this.Top = (h - e.NewSize.Height) / 2;
        }
        public void CheckIfAdobeInstalled()
        {
            RegistryKey software = Registry.LocalMachine.OpenSubKey("Software");

            if (software != null)
            {
                RegistryKey adobe = null;

                // Try to get 64bit versions of adobe
                if (Environment.Is64BitOperatingSystem)
                {
                    RegistryKey software64 = software.OpenSubKey("Wow6432Node");

                    if (software64 != null)
                        adobe = software64.OpenSubKey("Adobe");
                }

                // If a 64bit version is not installed, try to get a 32bit version
                if (adobe == null)
                    adobe = software.OpenSubKey("Adobe");

                // If no 64bit or 32bit version can be found, chances are adobe reader is not installed.
                if (adobe != null)
                {
                    RegistryKey acroRead = adobe.OpenSubKey("Acrobat Reader");

                    if (acroRead != null)
                    {
                        _isAdobeInstalled = true;
                        string[] acroReadVersions = acroRead.GetSubKeyNames();
                        Console.WriteLine("The following version(s) of Acrobat Reader are installed: ");

                        foreach (string versionNumber in acroReadVersions)
                        {
                            Console.WriteLine(versionNumber);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("Adode acrobat reader is required to preview documents, else you can open in browser. Would you like to install the reader?", "Adobe acrobat reader not installed", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                            System.Diagnostics.Process.Start("https://get.adobe.com/reader/otherversions/");
                    }
                }
                else
                {
                    var result = MessageBox.Show("Adode acrobat reader is required to preview documents, else you can open in browser. Would you like to install the reader?", "Adobe acrobat reader not installed", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                        System.Diagnostics.Process.Start("https://get.adobe.com/reader/otherversions/");
                }
            }
        }

        private void DeleteOldFiles()
        {
            try
            {
                bool exists = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//Pdfs");
                if (!exists)
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//Pdfs");
                DirectoryInfo d = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//Pdfs");
                FileInfo[] Files = d.GetFiles("*.pdf");
                foreach (FileInfo file in Files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message+ Environment.NewLine+ ex.InnerException, "Folder Access Error");
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
        private void PreviewClicked(object sender, RoutedEventArgs e)
        {
            if (PerformValidation())
            {
                if (!_isTwoColumn && _isAdobeInstalled)
                    AnimateToTwoColumns();
                GeneratePdf();
            }
        }

        private bool PerformValidation()
        {
            int count = txtAddr.Text.Split('\r').Length - 1;
            if (count > 2)
            {
                MessageBox.Show("Please limit address to 3 lines", "Address Too long", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtReceipt.Text.Trim()))
            {
                MessageBox.Show("Please enter receipt no", "Receipt number empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtReceipt.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(dtDate.Text.Trim()))
            {
                MessageBox.Show("Please enter date of receipt", "Date empty", MessageBoxButton.OK, MessageBoxImage.Error);
                dtDate.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtFrom.Text.Trim()))
            {
                MessageBox.Show("Please enter received from", "Received from empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFrom.Focus();
                return false;
            }
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^[a-zA-Z]{5}[0-9]{4}[a-zA-Z]{1}$");
            Match match = regex.Match(txtPan.Text);
            if (!match.Success)
            {
                MessageBox.Show("Please enter valid PAN", "Invalid PAN", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPan.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtAddr.Text.Trim()))
            {
                MessageBox.Show("Please enter address", "Address empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtAddr.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtRupeeInNumber.Text.Trim()))
            {
                MessageBox.Show("Please enter amount", "Amount empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtRupeeInNumber.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtRupeeInWord.Text.Trim()))
            {
                MessageBox.Show("Please enter amount in words", "Amount in words empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtRupeeInWord.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtVide.Text.Trim()))
            {
                MessageBox.Show("Please enter vide", "Vide empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtVide.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtDrawn.Text.Trim()))
            {
                MessageBox.Show("Please enter drawn on", "Drawn on empty", MessageBoxButton.OK, MessageBoxImage.Error);
                txtDrawn.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(cbTowards.Text.Trim()))
            {
                MessageBox.Show("Please enter towards", "Towards empty", MessageBoxButton.OK, MessageBoxImage.Error);
                cbTowards.Focus();
                return false;
            }
            return true;
        }
        private string GeneratePdf(bool isSendEmail = false)
        {
            try
            {
                var path = System.IO.Path.Combine(Environment.CurrentDirectory + "\\Saibisa_Receipt_Template.pdf");
                PdfReader pdfReader = new PdfReader(path);
                var myUniqueFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pdfs\\" + $@"{Guid.NewGuid().ToString().Replace('-', '_')}.pdf";
                if (isSendEmail)
                    myUniqueFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pdfs\\" + txtReceipt.Text + ".pdf";
                PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(myUniqueFileName, FileMode.Create));
                AcroFields pdfFormFields = pdfStamper.AcroFields;
                pdfFormFields.SetField("receiptNo", txtReceipt.Text);
                pdfFormFields.SetField("date", dtDate.Text);
                pdfFormFields.SetField("from", txtFrom.Text);
                pdfFormFields.SetField("address", txtAddr.Text);
                pdfFormFields.SetField("vide", txtVide.Text);
                pdfFormFields.SetField("drawnOn", txtDrawn.Text);
                pdfFormFields.SetField("towards", cbTowards.Text);
                pdfFormFields.SetField("amount", txtRupeeInNumber.Text + " /-");
                pdfFormFields.SetField("rupees", txtRupeeInWord.Text + " Only");
                pdfFormFields.SetField("pan", txtPan.Text);
                if (chk80G.IsChecked != true)
                {
                    pdfFormFields.SetField("80gText1", string.Empty);
                    pdfFormFields.SetField("80gtext2", string.Empty);
                }
                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                if (!isSendEmail)
                {
                    if (_isAdobeInstalled)
                        pdfViewer.Navigate(myUniqueFileName);
                    else
                        System.Diagnostics.Process.Start(myUniqueFileName);
                }
                return myUniqueFileName;
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message+ Environment.NewLine+ ex.InnerException, "File Access Error");
                return string.Empty;
            }
        }

        private void AnimateToTwoColumns()
        {
            this.Width = 1200;
            _isTwoColumn = true;
            Storyboard storyboard = new Storyboard();

            Duration duration = new Duration(TimeSpan.FromMilliseconds(1000));
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            GridLengthAnimation animation = new GridLengthAnimation();
            animation.Duration = duration;

            animation.Completed += delegate
            {
                // Set the animation to null on completion. This allows the grid to be resized manually
                previewColumn.BeginAnimation(RowDefinition.HeightProperty, null);

                // Set the final height.
                previewColumn.Width = new GridLength(2, GridUnitType.Star);
            };

            storyboard.Children.Add(animation);
            animation.From = new GridLength(0, GridUnitType.Star);
            animation.To = new GridLength(2, GridUnitType.Star);
            Storyboard.SetTarget(animation, previewColumn);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));
            storyboard.Children.Add(animation);

            storyboard.Begin();
        }

        private void txtRupeeInNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            long amount;
            bool isAmountEmpty = Int64.TryParse(txtRupeeInNumber.Text, out amount);
            var rupeeInWord = ConvertNumbertoWords(amount);
            txtRupeeInWord.Text = rupeeInWord;
        }

        public string ConvertNumbertoWords(long number)
        {
            if (number == 0) return "ZERO";
            if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
            string words = "";
            if ((number / 100000) > 0)
            {
                words += ConvertNumbertoWords(number / 100000) + " Lakh ";
                number %= 100000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumbertoWords(number / 100) + " Hundred ";
                number %= 100;
            }
            //if ((number / 10) > 0)  
            //{  
            // words += ConvertNumbertoWords(number / 10) + " RUPEES ";  
            // number %= 10;  
            //}  
            if (number > 0)
            {
                if (words != "") words += "And ";
                var unitsMap = new[]
                {
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };
                var tensMap = new[]
                {
            "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }

        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void ResetClicked(object sender, RoutedEventArgs e)
        {
            if (_isTwoColumn)
                AnimateToOneColumn();
            txtAddr.Text = txtDrawn.Text = txtFrom.Text = cbTowards.Text = txtPan.Text = txtReceipt.Text = txtRupeeInNumber.Text = txtRupeeInWord.Text = txtVide.Text = dtDate.Text = string.Empty;
        }

        private void AnimateToOneColumn()
        {
            this.Width = 600;

            _isTwoColumn = false;

            Storyboard storyboard = new Storyboard();

            Duration duration = new Duration(TimeSpan.FromMilliseconds(1000));
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            GridLengthAnimation animation = new GridLengthAnimation();
            animation.Duration = duration;

            animation.Completed += delegate
            {
                // Set the animation to null on completion. This allows the grid to be resized manually
                previewColumn.BeginAnimation(RowDefinition.HeightProperty, null);

                // Set the final height.
                previewColumn.Width = new GridLength(0, GridUnitType.Star);
            };

            storyboard.Children.Add(animation);
            animation.From = new GridLength(2, GridUnitType.Star);
            animation.To = new GridLength(0, GridUnitType.Star);
            Storyboard.SetTarget(animation, previewColumn);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));
            storyboard.Children.Add(animation);

            storyboard.Begin();
        }

        private void SendEmailClicked(object sender, RoutedEventArgs e)
        {
            if (PerformValidation())
                new SendEmail(GeneratePdf(true)).ShowDialog();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
