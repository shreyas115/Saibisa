using iTextSharp.text.pdf;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
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
        public static string _receiptNo, _receiptDate, _donorName, _address, _pan, _paymentMode, _purpose;
        public static float _amount = 0.0f;
        public MainWindow()
        {
            InitializeComponent();
            NewMethod();

            DeleteOldFiles();
            CheckIfAdobeInstalled();
            if (!_isAdobeInstalled)
            {
                this.Width = 600;
            }
            CenterWindowOnScreen();
            txtReceipt.Focus();
            cbFinYear_Selected(null, null);
        }

        private void NewMethod()
        {
            dtDate.SelectedDate = DateTime.Now;
            cbTowards.Items.Add("Donation - General");
            cbTowards.Items.Add("Donation - Corpus Fund");
            cbTowards.Items.Add("Donation - Annadhaan");
            cbTowards.Items.Add("Donation - Children Education");
            cbTowards.SelectedIndex = 0;
            cbFinYear.Items.Add("2018-2019");
            cbFinYear.Items.Add("2019-2020");
            cbFinYear.Items.Add("2020-2021");
            cbFinYear.Items.Add("2021-2022");
            cbFinYear.Items.Add("2022-2023");
            cbFinYear.Items.Add("2023-2024");
            cbFinYear.SelectedIndex = 1;
            cbSalutation.Items.Add("Mr.");
            cbSalutation.Items.Add("Ms.");
            cbSalutation.Items.Add("Messrs.");
            cbSalutation.SelectedIndex = 0;
            cbVide.Items.Add("Cash");
            cbVide.Items.Add("Direct Bank Transfer");
            cbVide.Items.Add("Cheque/DD");
            cbVide.SelectedIndex = 0;
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
            if (string.IsNullOrEmpty(cbVide.Text.Trim()))
            {
                MessageBox.Show("Please enter vide", "Vide empty", MessageBoxButton.OK, MessageBoxImage.Error);
                cbVide.Focus();
                return false;
            }
            //if (string.IsNullOrEmpty(txtDrawn.Text.Trim()))
            //{
            //    MessageBox.Show("Please enter drawn on", "Drawn on empty", MessageBoxButton.OK, MessageBoxImage.Error);
            //    txtDrawn.Focus();
            //    return false;
            //}
            if (string.IsNullOrEmpty(cbTowards.Text.Trim()))
            {
                MessageBox.Show("Please enter towards", "Towards empty", MessageBoxButton.OK, MessageBoxImage.Error);
                cbTowards.Focus();
                return false;
            }
            if(IsReceiptNoExists())
            {
                MessageBox.Show("Receipt no already exists, please update and try again", "Duplicate Receipt No.", MessageBoxButton.OK, MessageBoxImage.Error);
                txtReceipt.Focus();
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
                pdfFormFields.SetField("receiptNo", cbFinYear.Text +@"/"+ txtReceipt.Text);
                _receiptNo = cbFinYear.Text + @"/" + txtReceipt.Text;
                pdfFormFields.SetField("date", dtDate.Text);
                _receiptDate = dtDate.Text;
                pdfFormFields.SetField("from", cbSalutation.Text +" "+ txtFrom.Text);
                _donorName = cbSalutation.Text + " " + txtFrom.Text;
                pdfFormFields.SetField("address", txtAddr.Text);
                _address = txtAddr.Text;
                pdfFormFields.SetField("vide", cbVide.Text);
                _paymentMode = cbVide.Text;
                pdfFormFields.SetField("drawnOn", txtDrawn.Text);
                pdfFormFields.SetField("towards", cbTowards.Text);
                _purpose = cbTowards.Text;
                pdfFormFields.SetField("amount", txtRupeeInNumber.Text + " /-");
                _amount = float.Parse(txtRupeeInNumber.Text);
                pdfFormFields.SetField("rupees", txtRupeeInWord.Text);
                pdfFormFields.SetField("pan", txtPan.Text);
                _pan = txtPan.Text;
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
            txtRupeeInWord.Text = rupeeInWord + " Only.";
            CheckForCashAbove2K();
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

        private void cbFinYear_Selected(object sender, RoutedEventArgs e)
        {
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB\\saibisa.db";
            string cs = string.Format("URI=file:{0}", dbPath);
            using (var con = new SQLiteConnection(cs, true))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = string.Format("SELECT ReceiptNo FROM tblReceipts WHERE ReceiptNo LIKE '{0}%'", cbFinYear.Text);
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        long receiptNo = 0;
                        while (rdr.Read())
                        {
                            var rcptNo = $"{rdr.GetString(0)}";
                            var temp = rcptNo.Split('/')[1];
                            long tmpRecptNo = 0;
                            Int64.TryParse(temp, out tmpRecptNo);
                            if (tmpRecptNo > receiptNo)
                                receiptNo = tmpRecptNo;
                        }
                        txtReceipt.Text = (receiptNo+1).ToString().PadLeft(4, '0').Substring(0,4);
                    }
                }
            }
        }

        private bool IsReceiptNoExists()
        {
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB\\saibisa.db";
            string cs = string.Format("URI=file:{0}", dbPath);
            using (var con = new SQLiteConnection(cs, true))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = string.Format("SELECT ReceiptNo FROM tblReceipts WHERE ReceiptNo = '{0}'", cbFinYear.Text + @"/" + txtReceipt.Text);
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        int receiptCount = 0;
                        while (rdr.Read())
                        {
                            receiptCount++;
                        }
                        if (receiptCount > 0)
                            return true;
                    }
                }
            }
            return false;
        }
        private void ExportToExcel()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pdfs\\Saibisa_Receipt.csv";
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\saibisa\\DB\\saibisa.db";
            string cs = string.Format("URI=file:{0}", dbPath);
            using (var con = new SQLiteConnection(cs, true))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = string.Format("SELECT ReceiptNo, ReceiptDate,DonorName,Address, Pan, Amount,PaymentMode, Purpose FROM tblReceipts ORDER BY ReceiptNo DESC", cbFinYear.Text);
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        using (System.IO.StreamWriter fs = new System.IO.StreamWriter(fileName))
                        {
                            fs.Write("Receipt No, Receipt Date,Donor Name,Address, Pan, Amount,Payment Mode, Purpose");
                            fs.WriteLine();

                            // Loop through the rows and output the data
                            while (dr.Read())
                            {
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    string value = dr[i].ToString();
                                    if (value.Contains(","))
                                        value = "\"" + value + "\"";

                                    fs.Write(value + ",");
                                }
                                fs.WriteLine();
                            }
                            fs.Close();
                        }
                    }
                }
                if(ConvertWithNPOI(fileName.Replace("csv","xlsx"), "Receipt Report", ReadCsv(fileName)))
                {
                    tempBrowser.Source = new Uri(fileName.Replace("csv", "xlsx"));
                }
            }
        }
        private IEnumerable<string[]> ReadCsv(string fileName, char delimiter = ',')
        {
            var lines = System.IO.File.ReadAllLines(fileName, Encoding.UTF8).Select(a => a.Split(delimiter));
            return (lines);
        }
        private void ExportClicked(object sender, RoutedEventArgs e)
        {
            ExportToExcel();
        }
        private static bool ConvertWithNPOI(string excelFileName, string worksheetName, IEnumerable<string[]> csvLines)
        {
            if (csvLines == null )
            {
                return (false);
            }

            int rowCount = 0;
            int colCount = 0;

            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet(worksheetName);

            foreach (var line in csvLines)
            {
                IRow row = worksheet.CreateRow(rowCount);

                colCount = 0;
                foreach (var col in line)
                {
                    row.CreateCell(colCount).SetCellValue(col);
                    colCount++;
                }
                rowCount++;
            }

            using (FileStream fileWriter = File.Create(excelFileName))
            {
                workbook.Write(fileWriter);
                fileWriter.Close();
            }

            worksheet = null;
            workbook = null;

            return (true);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            cbFinYear_Selected(null,null);
        }

        private void ResetClicked(object sender, RoutedEventArgs e)
        {
            if (_isTwoColumn)
                AnimateToOneColumn();
            txtAddr.Text = txtDrawn.Text = txtFrom.Text = cbTowards.Text = txtPan.Text = txtReceipt.Text = txtRupeeInNumber.Text = txtRupeeInWord.Text = cbVide.Text = dtDate.Text = string.Empty;
            cbFinYear_Selected(null, null);
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
                new SendEmail(GeneratePdf(true), cbSalutation.Text + " " + txtFrom.Text).ShowDialog();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbVide_Selected(object sender, RoutedEventArgs e)
        {
            CheckForCashAbove2K();
        }

        private void CheckForCashAbove2K()
        {
            long amount = 0;
            long.TryParse(txtRupeeInNumber.Text, out amount);
            if (string.CompareOrdinal(cbVide.Text, "Cash") == 0 && amount > 2000)
            {
                imgWarning.Visibility = Visibility.Visible;
                chk80G.IsChecked = false;
                chk80G.IsEnabled = false;
            }
            else
            {
                imgWarning.Visibility = Visibility.Hidden;
                chk80G.IsEnabled = true;
                chk80G.IsChecked = true;
            }
        }
    }
}
