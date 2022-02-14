using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;

namespace Mars_Restaurant
{
    // This class functionality to printout a customer's invoice
    // It also displays the invoice in a form via a RichTextBox.
    public partial class ReceiptForm : Form
    {
        DoubleBufferedWindow _parent;

        // The constructor for the ReceiptForm... Passed in is the
        // DoubleBufferedWindow class so that we have access to the
        // data from that class.
        // This sets up the invoice data so that it is nicely formatted
        // for printing out to a printer.  The items are extracted from the
        // _lstVewInvoiceDup ListView control and placed in the RichTextBox
        // control "richTxtBxReceipt" in the formatted form for display to user
        // in the RichTextBox.
        public ReceiptForm(DoubleBufferedWindow parentWindow)
        {
            _parent = parentWindow; // Retain the parent window data via the member variable _parent.
            InitializeComponent();
            
            string txtPrice, category, food;
            double price;
            double sum = 0.0;
            string receiptData="";
            string marsRestaurant = " Mars Restaurant\n\n";
            string heading="";

            marsRestaurant = String.Format("{0,23}", marsRestaurant);
            food = "Food";
            category = "Category";
            txtPrice = "Price";

            FontFamily ff = FontFamily.GenericMonospace; 

            heading = String.Format(" {0,-13} - {1,-9} - {2,-7}\n", food, category, txtPrice);
            foreach (ListViewItem itm in _parent._lstVwInvoiceDup.Items)
            {
                food = itm.SubItems[0].Text;
                category = itm.SubItems[1].Text;
                txtPrice = itm.SubItems[2].Text;
                price = double.Parse(txtPrice);
                receiptData += string.Format(" {0,-15} - {1,-10} - {2,-7}\n", food, category, price);

                sum += price;
            }
            receiptData += "\n";
            richTxtBxReceipt.SelectionFont = new Font(ff.Name, 12, FontStyle.Bold);
            richTxtBxReceipt.SelectionColor = Color.Red;
            richTxtBxReceipt.SelectedText = marsRestaurant;
            richTxtBxReceipt.SelectionFont = new Font(ff.Name, 10, FontStyle.Bold);
            richTxtBxReceipt.SelectionColor = Color.Blue;
            richTxtBxReceipt.SelectedText = heading;
            richTxtBxReceipt.SelectionFont = new Font(ff.Name, 8, FontStyle.Bold);
            richTxtBxReceipt.SelectionColor = Color.Black;
            richTxtBxReceipt.SelectedText = receiptData;

            string totalSum = string.Format(" {0} Total: {1}\n", "-----------------------", sum);
            richTxtBxReceipt.SelectionFont = new Font(ff.Name, 8, FontStyle.Bold);
            richTxtBxReceipt.SelectionColor = Color.Violet;
            richTxtBxReceipt.SelectedText = totalSum;
            string dateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");            
            richTxtBxReceipt.SelectedText = string.Format(" {0}", dateTime);

            // Fix the window size
            this.MinimumSize = new Size(this.Width, this.Height);
            this.MaximumSize = new Size(this.Width, this.Height);
        }

        
        // When "Print" button is clicked on the receipt form, this function runs.
        // It launches a print dialog to print out the formatted RichTextBox text.
         private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            PrintDocument documentToPrint = new PrintDocument();
            printDialog.Document = documentToPrint;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {

                // Attach the function to handle placement of the rich text box data into the documentToPrint
                documentToPrint.PrintPage += new PrintPageEventHandler(DocumentToPrint_PrintPage); 
                // Execute printing of formatted text for the receipt.
                documentToPrint.Print();
            }
        }


        // This puts the richTextBox data into a formatted form for printing.
        private void DocumentToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            FontFamily ff = FontFamily.GenericMonospace;
            StringReader reader = new StringReader(richTxtBxReceipt.Text);
            float LinesPerPage = 0;
            float YPosition = 0;
            int Count = 0;
            float LeftMargin = e.MarginBounds.Left + 150;
            float TopMargin = e.MarginBounds.Top;
            string Line = null;
            Font PrintFont = this.richTxtBxReceipt.Font;
            SolidBrush PrintBrush = new SolidBrush(Color.Red);

            LinesPerPage = e.MarginBounds.Height / PrintFont.GetHeight(e.Graphics);

            YPosition = TopMargin;
            while (Count < LinesPerPage && ((Line = reader.ReadLine()) != null))
            {              

                if (Line.Contains("Mars Restaurant"))
                {
                    //12
                    PrintFont = new Font(ff.Name, 20, FontStyle.Bold);
                    PrintBrush = new SolidBrush(Color.Red);                   
                }
                else if (Line.Contains("Total:"))
                {
                    //8
                    PrintFont = new Font(ff.Name, 16, FontStyle.Bold);
                    PrintBrush = new SolidBrush(Color.Violet);
                }
                else if (Line.Contains("Category"))
                {
                    //10
                    PrintFont = new Font(ff.Name, 18, FontStyle.Bold);
                    PrintBrush = new SolidBrush(Color.Blue);
                }
                else
                {
                    PrintFont = new Font(ff.Name, 16, FontStyle.Bold);
                    PrintBrush = new SolidBrush(Color.Black);
                }

                // This draws the string which is output by the printer
                e.Graphics.DrawString(Line, PrintFont, PrintBrush, LeftMargin, YPosition, new StringFormat());
                YPosition += PrintFont.GetHeight(e.Graphics);
                Count++;
            }

            if (Line != null)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
            PrintBrush.Dispose();
        }
    }
}
