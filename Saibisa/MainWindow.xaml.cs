using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Saibisa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            pdfViewer.Navigate(new Uri("about:blank"));
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            previewColumn.Width = new GridLength(2, GridUnitType.Star);

            var path = System.IO.Path.Combine(Environment.CurrentDirectory + "\\Saibisa_Receipt_Template.pdf");
            PdfReader pdfReader = new PdfReader(path);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(@"C:/Users/vlpra/OneDrive/Desktop/temp.pdf", FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;
            pdfFormFields.SetField("receiptNo",txtReceipt.Text);
            pdfFormFields.SetField("date",dtDate.Text);
            pdfFormFields.SetField("from",txtFrom.Text);
            pdfFormFields.SetField("address",txtAddr.Text);
            pdfFormFields.SetField("vide",txtVide.Text);
            pdfFormFields.SetField("drawnOn",txtDrawn.Text);
            pdfFormFields.SetField("towards",cbTowards.Text);
            pdfFormFields.SetField("amount",txtRupeeInNumber.Text);
            pdfFormFields.SetField("pan", txtPan.Text);
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfViewer.Navigate(@"C:/Users/vlpra/OneDrive/Desktop/temp.pdf");
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

        private void ResetClicked (object sender, RoutedEventArgs e)
        {
            previewColumn.Width = new GridLength(0, GridUnitType.Star);
        }
    }
}
