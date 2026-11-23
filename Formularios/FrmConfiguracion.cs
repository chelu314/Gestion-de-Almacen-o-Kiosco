namespace KioscoLaFamilia.Formularios;

/// <summary>Datos del negocio, ticket, WhatsApp y avisos.</summary>
public class FrmConfiguracion : Form
{
    private readonly TextBox txtNombre;
    private readonly TextBox txtDireccion;
    private readonly TextBox txtTelefono;
    private readonly TextBox txtPais;
    private readonly TextBox txtProveedor;
    private readonly ComboBox cmbWhatsApp;
    private readonly ComboBox cmbAncho;
    private readonly TextBox txtPie;
    private readonly CheckBox chkAlerta;
    private readonly CheckBox chkTicket;
    private readonly CheckBox chkAprobacion;

    public FrmConfiguracion()
    {
        UI.PrepararDialogo(this, "Configuración", 620, 500);
        var campos = UI.TablaCampos();

        txtNombre = UI.Texto(340);
        // MaxLength = cantidad máxima de letras que se pueden escribir.
        // 40 alcanza para el nombre y no rompe el encabezado ni el ticket.
        txtNombre.MaxLength = 40;
        txtNombre.Text = ConfigRepo.Obtener("nombre_negocio", "Almacén La Tutti");
        UI.Campo(campos, "Nombre del negocio", txtNombre);

        txtDireccion = UI.Texto(340);
        txtDireccion.Text = ConfigRepo.Obtener("direccion");
        UI.Campo(campos, "Dirección", txtDireccion);
        txtDireccion.MaxLength = 100;

        txtTelefono = UI.TextoTelefono(200);
        txtTelefono.Text = ConfigRepo.Obtener("telefono");
        UI.Campo(campos, "Teléfono del negocio", txtTelefono);

        // Caja de teléfono: no deja escribir letras. Máximo 4 números (549 = Argentina celulares).
        txtPais = UI.TextoTelefono(80);
        txtPais.MaxLength = 4;
        txtPais.Text = ConfigRepo.Obtener("codigo_pais", "549");
        UI.Campo(campos, "Código país WhatsApp", txtPais);
        var ayuda = UI.Etiqueta("Argentina celulares: 549", 8.5f);
        ayuda.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayuda);

        txtProveedor = UI.TextoTelefono(200);
        txtProveedor.Text = ConfigRepo.Obtener("telefono_proveedor");
        UI.Campo(campos, "WhatsApp proveedor", txtProveedor);

        cmbWhatsApp = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
        cmbWhatsApp.Items.AddRange(new object[] { "WhatsApp Web (en el navegador)", "Aplicación de WhatsApp instalada" });
        cmbWhatsApp.SelectedIndex = ConfigRepo.Obtener("whatsapp_modo", "web") == "app" ? 1 : 0;
        UI.Campo(campos, "Abrir WhatsApp con", cmbWhatsApp);

        cmbAncho = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        cmbAncho.Items.AddRange(new object[] { "32 (impresora 58 mm)", "42 (impresora 80 mm)", "48 (impresora 80 mm)" });
        var ancho = ConfigRepo.Obtener("ancho_ticket", "32");
        cmbAncho.SelectedIndex = ancho == "48" ? 2 : ancho == "42" ? 1 : 0;
        UI.Campo(campos, "Ancho del ticket", cmbAncho);

        txtPie = UI.Texto(340);
        txtPie.Text = ConfigRepo.Obtener("pie_ticket", "¡Gracias por su compra!");
        UI.Campo(campos, "Mensaje al pie", txtPie);
        txtPie.MaxLength = 100;

        chkAlerta = new CheckBox { Text = "Avisar productos con poco stock al iniciar", AutoSize = true, Checked = ConfigRepo.ObtenerBool("alerta_stock_inicio", true) };
        UI.Campo(campos, "", chkAlerta);

        chkTicket = new CheckBox { Text = "Mostrar el ticket después de cada venta", AutoSize = true, Checked = ConfigRepo.ObtenerBool("mostrar_ticket", false) };
        UI.Campo(campos, "", chkTicket);

        chkAprobacion = new CheckBox { Text = "Cuentas nuevas: las tiene que activar el administrador", AutoSize = true, Checked = ConfigRepo.ObtenerBool("registro_requiere_aprobacion", true) };
        UI.Campo(campos, "", chkAprobacion);

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnGuardar = UI.Boton("Guardar", UI.Exito, 130);
        UI.ColorearBoton(btnGuardar, UI.Exito, destacado: true);
        btnGuardar.Click += (s, e) => Guardar();
        var btnCancelar = UI.Boton("Cancelar", UI.Neutro, 130);
        btnCancelar.Click += (s, e) => Close();
        barra.Controls.Add(btnGuardar);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, campos, barra);
        CancelButton = btnCancelar;
    }

    private void Guardar()
    {
        if (txtNombre.Text.Trim() == "") { UI.Advertir("El nombre del negocio es obligatorio."); return; }
        if (txtProveedor.Text.Trim() != "" && WhatsApp.Validar(txtProveedor.Text) is string errorProv)
        {
            UI.Advertir("WhatsApp del proveedor: " + errorProv);
            txtProveedor.Focus();
            return;
        }
        var pais = new string(txtPais.Text.Where(char.IsDigit).ToArray());

        bool ok = UI.Intentar(() =>
        {
            ConfigRepo.Guardar("nombre_negocio", txtNombre.Text.Trim());
            ConfigRepo.Guardar("direccion", txtDireccion.Text.Trim());
            ConfigRepo.Guardar("telefono", txtTelefono.Text.Trim());
            ConfigRepo.Guardar("codigo_pais", pais == "" ? "549" : pais);
            ConfigRepo.Guardar("telefono_proveedor", txtProveedor.Text.Trim() == "" ? "" : WhatsApp.Local(txtProveedor.Text));
            ConfigRepo.Guardar("whatsapp_modo", cmbWhatsApp.SelectedIndex == 1 ? "app" : "web");
            ConfigRepo.Guardar("ancho_ticket", cmbAncho.SelectedItem.ToString().Substring(0, 2));
            ConfigRepo.Guardar("pie_ticket", txtPie.Text.Trim());
            ConfigRepo.Guardar("alerta_stock_inicio", chkAlerta.Checked ? "1" : "0");
            ConfigRepo.Guardar("mostrar_ticket", chkTicket.Checked ? "1" : "0");
            ConfigRepo.Guardar("registro_requiere_aprobacion", chkAprobacion.Checked ? "1" : "0");
        }, "No se pudo guardar la configuración.");

        if (ok)
        {
            UI.Info("Configuración guardada.");
            Close();
        }
    }
}
