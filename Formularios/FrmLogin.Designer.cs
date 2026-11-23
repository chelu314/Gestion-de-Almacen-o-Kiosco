namespace KioscoLaFamilia.Formularios
{
    partial class FrmLogin
    {
        /// <summary>Variable del diseñador necesaria.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Limpiar los recursos que se estén usando.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Diseño del login: pantalla completa con una tarjeta blanca centrada.
        /// Dentro de la tarjeta, flpCampos acomoda los controles uno debajo del otro
        /// (así nunca se pisan, aunque Windows agrande las letras).
        /// Se puede abrir con el Diseñador de Visual Studio (doble clic en FrmLogin.cs).
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTarjeta = new System.Windows.Forms.Panel();
            this.flpCampos = new System.Windows.Forms.FlowLayoutPanel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.chkVerClave = new System.Windows.Forms.CheckBox();
            this.btnIngresar = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.lnkCrearCuenta = new System.Windows.Forms.LinkLabel();
            this.lnkConexion = new System.Windows.Forms.LinkLabel();
            this.pnlTarjeta.SuspendLayout();
            this.flpCampos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            //
            // pnlTarjeta
            //
            this.pnlTarjeta.AutoSize = true;
            this.pnlTarjeta.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlTarjeta.BackColor = System.Drawing.Color.White;
            this.pnlTarjeta.Controls.Add(this.flpCampos);
            this.pnlTarjeta.Location = new System.Drawing.Point(300, 40);
            this.pnlTarjeta.Name = "pnlTarjeta";
            this.pnlTarjeta.Padding = new System.Windows.Forms.Padding(36, 24, 36, 28);
            this.pnlTarjeta.TabIndex = 0;
            this.pnlTarjeta.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTarjeta_Paint);
            //
            // flpCampos
            //
            this.flpCampos.AutoSize = true;
            this.flpCampos.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpCampos.BackColor = System.Drawing.Color.White;
            this.flpCampos.Controls.Add(this.picLogo);
            this.flpCampos.Controls.Add(this.lblTitulo);
            this.flpCampos.Controls.Add(this.lblUsuario);
            this.flpCampos.Controls.Add(this.txtUsuario);
            this.flpCampos.Controls.Add(this.lblClave);
            this.flpCampos.Controls.Add(this.txtClave);
            this.flpCampos.Controls.Add(this.chkVerClave);
            this.flpCampos.Controls.Add(this.btnIngresar);
            this.flpCampos.Controls.Add(this.lblMensaje);
            this.flpCampos.Controls.Add(this.lnkCrearCuenta);
            this.flpCampos.Controls.Add(this.lnkConexion);
            this.flpCampos.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpCampos.Location = new System.Drawing.Point(36, 24);
            this.flpCampos.Name = "flpCampos";
            this.flpCampos.TabIndex = 0;
            this.flpCampos.WrapContents = false;
            //
            // picLogo
            //
            this.picLogo.BackColor = System.Drawing.Color.White;
            this.picLogo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(360, 210);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            //
            // lblTitulo
            //
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(10, 28, 180);
            this.lblTitulo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(360, 32);
            this.lblTitulo.TabIndex = 1;
            this.lblTitulo.Text = "Iniciar sesión";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblUsuario
            //
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.Margin = new System.Windows.Forms.Padding(0, 4, 0, 3);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.TabIndex = 2;
            this.lblUsuario.Text = "Usuario";
            //
            // txtUsuario
            //
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtUsuario.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(360, 29);
            this.txtUsuario.TabIndex = 3;
            //
            // lblClave
            //
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblClave.Margin = new System.Windows.Forms.Padding(0, 4, 0, 3);
            this.lblClave.Name = "lblClave";
            this.lblClave.TabIndex = 4;
            this.lblClave.Text = "Contraseña";
            //
            // txtClave
            //
            this.txtClave.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtClave.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.txtClave.Name = "txtClave";
            this.txtClave.Size = new System.Drawing.Size(360, 29);
            this.txtClave.TabIndex = 5;
            this.txtClave.UseSystemPasswordChar = true;
            //
            // chkVerClave
            //
            this.chkVerClave.AutoSize = true;
            this.chkVerClave.Margin = new System.Windows.Forms.Padding(0, 4, 0, 12);
            this.chkVerClave.Name = "chkVerClave";
            this.chkVerClave.TabIndex = 6;
            this.chkVerClave.Text = "Mostrar contraseña";
            this.chkVerClave.UseVisualStyleBackColor = true;
            this.chkVerClave.CheckedChanged += new System.EventHandler(this.chkVerClave_CheckedChanged);
            //
            // btnIngresar
            //
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(10, 28, 180);
            this.btnIngresar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.FlatAppearance.BorderSize = 0;
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.btnIngresar.Name = "btnIngresar";
            this.btnIngresar.Size = new System.Drawing.Size(360, 48);
            this.btnIngresar.TabIndex = 7;
            this.btnIngresar.Text = "Ingresar";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
            //
            // lblMensaje  (crece hacia abajo si el texto es largo; empuja a los enlaces en vez de taparlos)
            //
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(200, 32, 45);
            this.lblMensaje.Margin = new System.Windows.Forms.Padding(0, 4, 0, 10);
            this.lblMensaje.MaximumSize = new System.Drawing.Size(360, 0);
            this.lblMensaje.MinimumSize = new System.Drawing.Size(360, 0);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.TabIndex = 8;
            //
            // lnkCrearCuenta
            //
            this.lnkCrearCuenta.AutoSize = true;
            this.lnkCrearCuenta.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.lnkCrearCuenta.Name = "lnkCrearCuenta";
            this.lnkCrearCuenta.TabIndex = 9;
            this.lnkCrearCuenta.TabStop = true;
            this.lnkCrearCuenta.Text = "¿No tenés usuario? Crear cuenta";
            this.lnkCrearCuenta.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCrearCuenta_LinkClicked);
            //
            // lnkConexion
            //
            this.lnkConexion.AutoSize = true;
            this.lnkConexion.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lnkConexion.Name = "lnkConexion";
            this.lnkConexion.TabIndex = 10;
            this.lnkConexion.TabStop = true;
            this.lnkConexion.Text = "Configurar conexión al servidor";
            this.lnkConexion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkConexion_LinkClicked);
            //
            // FrmLogin
            //
            this.AcceptButton = this.btnIngresar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(243, 246, 250);
            this.ClientSize = new System.Drawing.Size(1024, 720);
            this.Controls.Add(this.pnlTarjeta);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(520, 600);
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ingreso";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Shown += new System.EventHandler(this.FrmLogin_Shown);
            this.Resize += new System.EventHandler(this.FrmLogin_Resize);
            this.flpCampos.ResumeLayout(false);
            this.flpCampos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlTarjeta.ResumeLayout(false);
            this.pnlTarjeta.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlTarjeta;
        private System.Windows.Forms.FlowLayoutPanel flpCampos;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.CheckBox chkVerClave;
        private System.Windows.Forms.Button btnIngresar;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.LinkLabel lnkCrearCuenta;
        private System.Windows.Forms.LinkLabel lnkConexion;
    }
}
