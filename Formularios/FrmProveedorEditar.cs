namespace KioscoLaFamilia.Formularios;

/// <summary>Ventana para cargar un proveedor nuevo o cambiar sus datos.</summary>
public class FrmProveedorEditar : Form
{
    private readonly Proveedor proveedor;
    private readonly TextBox txtNombre;
    private readonly TextBox txtTelefono;
    private readonly TextBox txtQueVende;

    // Si recibe null es un proveedor NUEVO; si recibe uno, lo está EDITANDO.
    public FrmProveedorEditar(Proveedor p)
    {
        proveedor = p ?? new Proveedor();
        UI.PrepararDialogo(this, proveedor.Id == 0 ? "Nuevo proveedor" : "Editar proveedor", 560, 300);

        var campos = UI.TablaCampos();

        txtNombre = UI.Texto(320);
        txtNombre.MaxLength = 100;           // igual que en la base: VARCHAR(100)
        txtNombre.Text = proveedor.Nombre;
        UI.Campo(campos, "Nombre *", txtNombre);

        // TextoTelefono: no deja escribir letras
        txtTelefono = UI.TextoTelefono(200);
        txtTelefono.Text = proveedor.Telefono;
        UI.Campo(campos, "WhatsApp", txtTelefono);
        var ayuda = UI.Etiqueta("Con código de área, sin 0 ni 15. Ej: 3794111222", 8.5f);
        ayuda.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayuda);

        txtQueVende = UI.Texto(320);
        txtQueVende.MaxLength = 150;
        txtQueVende.Text = proveedor.QueVende;
        UI.Campo(campos, "Qué vende", txtQueVende);
        var ayuda2 = UI.Etiqueta("Ej: Lácteos, Bebidas, Pan, Limpieza", 8.5f);
        ayuda2.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayuda2);

        // Botones abajo a la derecha: Guardar (fuerte) y Cancelar
        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnGuardar = UI.Boton("Guardar", UI.Exito, 130);
        UI.ColorearBoton(btnGuardar, UI.Exito, destacado: true);
        btnGuardar.Click += (s, e) => Guardar();
        var btnCancelar = UI.Boton("Cancelar", UI.Neutro, 130);
        btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        barra.Controls.Add(btnGuardar);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, campos, barra);
        AcceptButton = btnGuardar;   // Enter = Guardar
        CancelButton = btnCancelar;  // Esc = Cancelar
    }

    private void Guardar()
    {
        // 1) Validaciones
        if (txtNombre.Text.Trim() == "") { UI.Advertir("El nombre es obligatorio."); txtNombre.Focus(); return; }
        if (txtTelefono.Text.Trim() != "")
        {
            var error = WhatsApp.Validar(txtTelefono.Text);   // null = está bien
            if (error != null) { UI.Advertir(error); txtTelefono.Focus(); return; }
        }

        // 2) Pasar lo escrito al objeto
        proveedor.Nombre = txtNombre.Text.Trim();
        proveedor.Telefono = txtTelefono.Text.Trim() == "" ? "" : WhatsApp.Local(txtTelefono.Text);
        proveedor.QueVende = txtQueVende.Text.Trim();

        // 3) Guardar en la base y cerrar avisando que salió bien (OK)
        if (!UI.Intentar(() => ProveedorRepo.Guardar(proveedor), "No se pudo guardar el proveedor.")) return;
        DialogResult = DialogResult.OK;
        Close();
    }
}
