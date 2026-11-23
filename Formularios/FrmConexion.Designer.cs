namespace KioscoLaFamilia.Formularios
{
    partial class FrmConexion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.lblServidor = new System.Windows.Forms.Label();
            this.txtServidor = new System.Windows.Forms.TextBox();
            this.lblPuerto = new System.Windows.Forms.Label();
            this.numPuerto = new System.Windows.Forms.NumericUpDown();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.chkVerClave = new System.Windows.Forms.CheckBox();
            this.lblBase = new System.Windows.Forms.Label();
            this.txtBase = new System.Windows.Forms.TextBox();
            this.lblEstado = new System.Windows.Forms.Label();
            this.btnProbar = new System.Windows.Forms.Button();
            this.btnCrearBase = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numPuerto)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(10, 28, 180);
            this.lblTitulo.Location = new System.Drawing.Point(28, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(330, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Conexión a la base de datos";
            //
            // lblMensaje
            //
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.lblMensaje.Location = new System.Drawing.Point(30, 54);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(500, 44);
            this.lblMensaje.TabIndex = 1;
            this.lblMensaje.Text = "Cada computadora guarda acá los datos de su propio servidor MySQL.";
            //
            // lblServidor
            //
            this.lblServidor.AutoSize = true;
            this.lblServidor.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblServidor.Location = new System.Drawing.Point(30, 108);
            this.lblServidor.Name = "lblServidor";
            this.lblServidor.Size = new System.Drawing.Size(70, 19);
            this.lblServidor.TabIndex = 2;
            this.lblServidor.Text = "Servidor";
            //
            // txtServidor
            //
            this.txtServidor.Location = new System.Drawing.Point(30, 130);
            this.txtServidor.Name = "txtServidor";
            this.txtServidor.Size = new System.Drawing.Size(360, 25);
            this.txtServidor.TabIndex = 3;
            this.txtServidor.Text = "localhost";
            //
            // lblPuerto
            //
            this.lblPuerto.AutoSize = true;
            this.lblPuerto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPuerto.Location = new System.Drawing.Point(410, 108);
            this.lblPuerto.Name = "lblPuerto";
            this.lblPuerto.Size = new System.Drawing.Size(55, 19);
            this.lblPuerto.TabIndex = 4;
            this.lblPuerto.Text = "Puerto";
            //
            // numPuerto
            //
            this.numPuerto.Location = new System.Drawing.Point(410, 130);
            this.numPuerto.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPuerto.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPuerto.Name = "numPuerto";
            this.numPuerto.Size = new System.Drawing.Size(120, 25);
            this.numPuerto.TabIndex = 5;
            this.numPuerto.Value = new decimal(new int[] { 3306, 0, 0, 0 });
            //
            // lblUsuario
            //
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.Location = new System.Drawing.Point(30, 166);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(130, 19);
            this.lblUsuario.TabIndex = 6;
            this.lblUsuario.Text = "Usuario de MySQL";
            //
            // txtUsuario
            //
            this.txtUsuario.Location = new System.Drawing.Point(30, 188);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(240, 25);
            this.txtUsuario.TabIndex = 7;
            this.txtUsuario.Text = "root";
            //
            // lblClave
            //
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblClave.Location = new System.Drawing.Point(290, 166);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(160, 19);
            this.lblClave.TabIndex = 8;
            this.lblClave.Text = "Contraseña de MySQL";
            //
            // txtClave
            //
            this.txtClave.Location = new System.Drawing.Point(290, 188);
            this.txtClave.Name = "txtClave";
            this.txtClave.Size = new System.Drawing.Size(240, 25);
            this.txtClave.TabIndex = 9;
            this.txtClave.UseSystemPasswordChar = true;
            //
            // chkVerClave
            //
            this.chkVerClave.AutoSize = true;
            this.chkVerClave.Location = new System.Drawing.Point(292, 218);
            this.chkVerClave.Name = "chkVerClave";
            this.chkVerClave.Size = new System.Drawing.Size(140, 23);
            this.chkVerClave.TabIndex = 10;
            this.chkVerClave.Text = "Mostrar contraseña";
            this.chkVerClave.UseVisualStyleBackColor = true;
            this.chkVerClave.CheckedChanged += new System.EventHandler(this.chkVerClave_CheckedChanged);
            //
            // lblBase
            //
            this.lblBase.AutoSize = true;
            this.lblBase.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBase.Location = new System.Drawing.Point(30, 248);
            this.lblBase.Name = "lblBase";
            this.lblBase.Size = new System.Drawing.Size(240, 19);
            this.lblBase.TabIndex = 11;
            this.lblBase.Text = "Nombre de la base de datos";
            //
            // txtBase
            //
            this.txtBase.Location = new System.Drawing.Point(30, 270);
            this.txtBase.Name = "txtBase";
            this.txtBase.Size = new System.Drawing.Size(240, 25);
            this.txtBase.TabIndex = 12;
            this.txtBase.Text = "kiosco_la_familia";
            //
            // lblEstado
            //
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblEstado.Location = new System.Drawing.Point(30, 306);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(500, 44);
            this.lblEstado.TabIndex = 13;
            //
            // btnProbar
            //
            this.btnProbar.BackColor = System.Drawing.Color.FromArgb(10, 28, 180);
            this.btnProbar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnProbar.FlatAppearance.BorderSize = 0;
            this.btnProbar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProbar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProbar.ForeColor = System.Drawing.Color.White;
            this.btnProbar.Location = new System.Drawing.Point(30, 358);
            this.btnProbar.Name = "btnProbar";
            this.btnProbar.Size = new System.Drawing.Size(240, 40);
            this.btnProbar.TabIndex = 14;
            this.btnProbar.Text = "Probar conexión";
            this.btnProbar.UseVisualStyleBackColor = false;
            this.btnProbar.Click += new System.EventHandler(this.btnProbar_Click);
            //
            // btnCrearBase
            //
            this.btnCrearBase.BackColor = System.Drawing.Color.FromArgb(0, 121, 107);
            this.btnCrearBase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCrearBase.FlatAppearance.BorderSize = 0;
            this.btnCrearBase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrearBase.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCrearBase.ForeColor = System.Drawing.Color.White;
            this.btnCrearBase.Location = new System.Drawing.Point(290, 358);
            this.btnCrearBase.Name = "btnCrearBase";
            this.btnCrearBase.Size = new System.Drawing.Size(240, 40);
            this.btnCrearBase.TabIndex = 15;
            this.btnCrearBase.Text = "Crear la base de datos";
            this.btnCrearBase.UseVisualStyleBackColor = false;
            this.btnCrearBase.Click += new System.EventHandler(this.btnCrearBase_Click);
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(30, 408);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(240, 44);
            this.btnGuardar.TabIndex = 16;
            this.btnGuardar.Text = "Guardar y usar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(84, 110, 122);
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(290, 408);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(240, 44);
            this.btnCancelar.TabIndex = 17;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            //
            // FrmConexion
            //
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(560, 476);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblServidor);
            this.Controls.Add(this.txtServidor);
            this.Controls.Add(this.lblPuerto);
            this.Controls.Add(this.numPuerto);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.chkVerClave);
            this.Controls.Add(this.lblBase);
            this.Controls.Add(this.txtBase);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.btnProbar);
            this.Controls.Add(this.btnCrearBase);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConexion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Conexión a la base de datos";
            ((System.ComponentModel.ISupportInitialize)(this.numPuerto)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.Label lblServidor;
        private System.Windows.Forms.TextBox txtServidor;
        private System.Windows.Forms.Label lblPuerto;
        private System.Windows.Forms.NumericUpDown numPuerto;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.CheckBox chkVerClave;
        private System.Windows.Forms.Label lblBase;
        private System.Windows.Forms.TextBox txtBase;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Button btnProbar;
        private System.Windows.Forms.Button btnCrearBase;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
