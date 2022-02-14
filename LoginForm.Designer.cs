
namespace Mars_Restaurant
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.label4 = new System.Windows.Forms.Label();
            this.txtBxEmail = new System.Windows.Forms.TextBox();
            this.picBoxLogo1 = new System.Windows.Forms.PictureBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.grpBoxEmployees = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBxPassword = new System.Windows.Forms.TextBox();
            this.btnCreateAcctForm = new System.Windows.Forms.Button();
            this.picBoxSplash = new System.Windows.Forms.PictureBox();
            this.lblSplashName = new System.Windows.Forms.Label();
            this.lblMarsRestaurant = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo1)).BeginInit();
            this.grpBoxEmployees.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSplash)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Email:";
            // 
            // txtBxEmail
            // 
            this.txtBxEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxEmail.Location = new System.Drawing.Point(96, 48);
            this.txtBxEmail.Name = "txtBxEmail";
            this.txtBxEmail.Size = new System.Drawing.Size(170, 22);
            this.txtBxEmail.TabIndex = 7;        
            // 
            // picBoxLogo1
            // 
            this.picBoxLogo1.Image = global::Mars_Restaurant.Properties.Resources.MarsLogo;
            this.picBoxLogo1.Location = new System.Drawing.Point(299, 16);
            this.picBoxLogo1.Name = "picBoxLogo1";
            this.picBoxLogo1.Size = new System.Drawing.Size(170, 122);
            this.picBoxLogo1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxLogo1.TabIndex = 8;
            this.picBoxLogo1.TabStop = false;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(301, 148);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(80, 25);
            this.btnLogin.TabIndex = 10;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(388, 148);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(80, 25);
            this.btnExit.TabIndex = 11;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // grpBoxEmployees
            // 
            this.grpBoxEmployees.Controls.Add(this.label5);
            this.grpBoxEmployees.Controls.Add(this.txtBxPassword);
            this.grpBoxEmployees.Controls.Add(this.txtBxEmail);
            this.grpBoxEmployees.Controls.Add(this.label4);
            this.grpBoxEmployees.Controls.Add(this.lblSplashName);
            this.grpBoxEmployees.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpBoxEmployees.Location = new System.Drawing.Point(7, 5);
            this.grpBoxEmployees.Name = "grpBoxEmployees";
            this.grpBoxEmployees.Size = new System.Drawing.Size(276, 133);
            this.grpBoxEmployees.TabIndex = 12;
            this.grpBoxEmployees.TabStop = false;
            this.grpBoxEmployees.Text = "Employee Login";
            this.grpBoxEmployees.UseCompatibleTextRendering = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 89);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 16);
            this.label5.TabIndex = 9;
            this.label5.Text = "Password:";
            // 
            // txtBxPassword
            // 
            this.txtBxPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxPassword.Location = new System.Drawing.Point(97, 83);
            this.txtBxPassword.Name = "txtBxPassword";
            this.txtBxPassword.PasswordChar = '*';
            this.txtBxPassword.Size = new System.Drawing.Size(170, 22);
            this.txtBxPassword.TabIndex = 8;
            // 
            // btnCreateAcctForm
            // 
            this.btnCreateAcctForm.Location = new System.Drawing.Point(12, 148);
            this.btnCreateAcctForm.Name = "btnCreateAcctForm";
            this.btnCreateAcctForm.Size = new System.Drawing.Size(94, 25);
            this.btnCreateAcctForm.TabIndex = 13;
            this.btnCreateAcctForm.Text = "Create Account";
            this.btnCreateAcctForm.UseVisualStyleBackColor = true;
            this.btnCreateAcctForm.Click += new System.EventHandler(this.btnCreateAcctForm_Click);
            // 
            // picBoxSplash
            // 
            this.picBoxSplash.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picBoxSplash.Image = ((System.Drawing.Image)(resources.GetObject("picBoxSplash.Image")));
            this.picBoxSplash.Location = new System.Drawing.Point(-3, -5);
            this.picBoxSplash.Name = "picBoxSplash";
            this.picBoxSplash.Size = new System.Drawing.Size(488, 192);
            this.picBoxSplash.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBoxSplash.TabIndex = 14;
            this.picBoxSplash.TabStop = false;
            // 
            // lblSplashName
            // 
            this.lblSplashName.AutoSize = true;
            this.lblSplashName.BackColor = System.Drawing.Color.Transparent;
            this.lblSplashName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSplashName.ForeColor = System.Drawing.Color.Blue;
            this.lblSplashName.Location = new System.Drawing.Point(121, 83);
            this.lblSplashName.Name = "lblSplashName";
            this.lblSplashName.Size = new System.Drawing.Size(205, 24);
            this.lblSplashName.TabIndex = 15;
            this.lblSplashName.Text = "Welcome: Clark Kent";
            // 
            // lblMarsRestaurant
            // 
            this.lblMarsRestaurant.AutoSize = true;
            this.lblMarsRestaurant.BackColor = System.Drawing.Color.Transparent;
            this.lblMarsRestaurant.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMarsRestaurant.ForeColor = System.Drawing.Color.Blue;
            this.lblMarsRestaurant.Location = new System.Drawing.Point(159, 32);
            this.lblMarsRestaurant.Name = "lblMarsRestaurant";
            this.lblMarsRestaurant.Size = new System.Drawing.Size(160, 24);
            this.lblMarsRestaurant.TabIndex = 15;
            this.lblMarsRestaurant.Text = "Mars Restaurant";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 184);
            this.Controls.Add(this.btnCreateAcctForm);
            this.Controls.Add(this.grpBoxEmployees);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.picBoxLogo1);
            this.Controls.Add(this.picBoxSplash);
            this.Controls.Add(this.lblMarsRestaurant);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.Text = "LoginForm";
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLogo1)).EndInit();
            this.grpBoxEmployees.ResumeLayout(false);
            this.grpBoxEmployees.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSplash)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBxEmail;
        private System.Windows.Forms.PictureBox picBoxLogo1;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.GroupBox grpBoxEmployees;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBxPassword;
        private System.Windows.Forms.Button btnCreateAcctForm;
        private System.Windows.Forms.PictureBox picBoxSplash;
        private System.Windows.Forms.Label lblSplashName;
        private System.Windows.Forms.Label lblMarsRestaurant;
    }
}