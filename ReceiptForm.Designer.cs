
namespace Mars_Restaurant
{
    partial class ReceiptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReceiptForm));
            this.richTxtBxReceipt = new System.Windows.Forms.RichTextBox();
            this.btnPrintReceipt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTxtBxReceipt
            // 
            this.richTxtBxReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTxtBxReceipt.Location = new System.Drawing.Point(3, 4);
            this.richTxtBxReceipt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTxtBxReceipt.Name = "richTxtBxReceipt";
            this.richTxtBxReceipt.ReadOnly = true;
            this.richTxtBxReceipt.Size = new System.Drawing.Size(401, 397);
            this.richTxtBxReceipt.TabIndex = 0;
            this.richTxtBxReceipt.Text = "";
            // 
            // btnPrintReceipt
            // 
            this.btnPrintReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrintReceipt.Location = new System.Drawing.Point(312, 409);
            this.btnPrintReceipt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPrintReceipt.Name = "btnPrintReceipt";
            this.btnPrintReceipt.Size = new System.Drawing.Size(81, 34);
            this.btnPrintReceipt.TabIndex = 1;
            this.btnPrintReceipt.Text = "Print";
            this.btnPrintReceipt.UseVisualStyleBackColor = true;
            this.btnPrintReceipt.Click += new System.EventHandler(this.btnPrintReceipt_Click);
            // 
            // ReceiptForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 448);
            this.Controls.Add(this.btnPrintReceipt);
            this.Controls.Add(this.richTxtBxReceipt);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ReceiptForm";
            this.Text = "Mars Restaurant Receipt";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTxtBxReceipt;
        private System.Windows.Forms.Button btnPrintReceipt;
    }
}