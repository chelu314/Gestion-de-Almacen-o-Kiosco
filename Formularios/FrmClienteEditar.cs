namespace KioscoLaFamilia.Formularios;

public class FrmClienteEditar : Form
{
    private readonly Cliente cliente;
    private readonly TextBox txtNombre;
    private readonly TextBox txtTelefono;
    private readonly TextBox txtDireccion;
    private readonly NumericUpDown numLimite;
    private readonly TextBox txtObs;

    public int ClienteId { get; private set; }

    public FrmClienteEditar(Cliente c)
    {
        cliente = c ?? new Cliente();
        UI.PrepararDialogo(this, cliente.Id == 0 ? "Nuevo cliente" : "Editar cliente", 560, 360);

        var campos = UI.TablaCampos();
        txtNombre = UI.Texto(320);
        txtNombre.Text = cliente.Nombre;
        UI.Campo(campos, "Nombre y apellido *", txtNombre);

        txtTelefono = UI.TextoTelefono(200);
        txtTelefono.Text = cliente.Telefono;
        UI.Campo(campos, "WhatsApp", txtTelefono);
        var ayuda = UI.Etiqueta("Con código de área, sin 0 ni 15. Ej: 3794111222", 8.5f);
        ayuda.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayuda);

        txtDireccion = UI.Texto(320);
        txtDireccion.Text = cliente.Direccion;
        UI.Campo(campos, "Dirección", txtDireccion);

        numLimite = UI.Numero();
        numLimite.Value = cliente.LimiteCredito;
        UI.Campo(campos, "Límite de fiado (0 = sin límite)", numLimite);

        txtObs = UI.Texto(320);
        txtObs.Text = cliente.Observaciones;
        UI.Campo(campos, "Observaciones", txtObs);

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnGuardar = UI.Boton("Guardar", UI.Exito, 130);
        UI.ColorearBoton(btnGuardar, UI.Exito, destacado: true);
        btnGuardar.Click += (s, e) => Guardar();
        var btnCancelar = UI.Boton("Cancelar", UI.Neutro, 130);
        btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        barra.Controls.Add(btnGuardar);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, campos, barra);
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
    }

    private void Guardar()
    {
        if (txtNombre.Text.Trim() == "") { UI.Advertir("El nombre es obligatorio."); txtNombre.Focus(); return; }

        cliente.Nombre = txtNombre.Text.Trim();
        if (txtTelefono.Text.Trim() != "")
        {
            var error = WhatsApp.Validar(txtTelefono.Text);
            if (error != null) { UI.Advertir(error); txtTelefono.Focus(); return; }
        }
        cliente.Telefono = txtTelefono.Text.Trim() == "" ? "" : WhatsApp.Local(txtTelefono.Text);
        cliente.Direccion = txtDireccion.Text.Trim();
        cliente.LimiteCredito = numLimite.Value;
        cliente.Observaciones = txtObs.Text.Trim();

        if (!UI.Intentar(() => ClienteRepo.Guardar(cliente), "No se pudo guardar el cliente.")) return;
        ClienteId = cliente.Id;
        DialogResult = DialogResult.OK;
        Close();
    }
}
