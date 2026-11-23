namespace KioscoLaFamilia.Formularios;

/// <summary>Alta y modificación de un producto, con cálculo de precio por margen.</summary>
public class FrmProductoEditar : Form
{
    private readonly Producto producto;
    private readonly TextBox txtCodigo;
    private readonly TextBox txtNombre;
    private readonly ComboBox cmbCategoria;
    private readonly NumericUpDown numCosto;
    private readonly NumericUpDown numMargen;
    private readonly NumericUpDown numPrecio;
    private readonly NumericUpDown numStock;
    private readonly NumericUpDown numMinimo;
    private readonly ComboBox cmbUnidad;
    private readonly CheckBox chkControla;
    private readonly CheckBox chkRapida;
    private readonly Label lblPrecio;
    private bool calculando;

    public int ProductoId { get; private set; }

    public FrmProductoEditar(Producto p, string codigoInicial = null)
    {
        producto = p ?? new Producto { CodigoBarras = codigoInicial };
        bool nuevo = producto.Id == 0;
        UI.PrepararDialogo(this, nuevo ? "Nuevo producto" : "Editar producto", 600, 600);

        var campos = UI.TablaCampos();

        txtCodigo = UI.Texto(300);
        txtCodigo.Text = producto.CodigoBarras;
        // El lector manda Enter al final: pasamos al siguiente campo
        txtCodigo.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtNombre.Focus(); }
        };
        UI.Campo(campos, "Código de barras", txtCodigo);

        txtNombre = UI.Texto(300);
        txtNombre.Text = producto.Nombre;
        UI.Campo(campos, "Nombre *", txtNombre);

        cmbCategoria = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
        try { foreach (var c in ProductoRepo.Categorias()) cmbCategoria.Items.Add(c); }
        catch { foreach (var c in Categorias.Predeterminadas) cmbCategoria.Items.Add(c); }
        cmbCategoria.Text = string.IsNullOrEmpty(producto.Categoria) || producto.Categoria == "General" ? "Almacén" : producto.Categoria;
        UI.Campo(campos, "Tipo / categoría", cmbCategoria);

        cmbUnidad = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbUnidad.Items.AddRange(new object[] { "Por unidad", "Por kilo (se pesa)" });
        cmbUnidad.SelectedIndex = producto.PorPeso ? 1 : 0;
        UI.Campo(campos, "Se vende", cmbUnidad);

        numCosto = UI.Numero();
        numCosto.Value = producto.PrecioCosto;
        UI.Campo(campos, "Precio de costo", numCosto);

        numMargen = UI.Numero(1000, 1, 100);
        numMargen.Minimum = -100;
        numMargen.Value = producto.PrecioCosto > 0
            ? Math.Clamp(Math.Round((producto.PrecioVenta - producto.PrecioCosto) / producto.PrecioCosto * 100, 1), -100, 1000)
            : 40;
        UI.Campo(campos, "Margen de ganancia %", numMargen);

        numPrecio = UI.Numero();
        numPrecio.Value = producto.PrecioVenta;
        numPrecio.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
        UI.Campo(campos, "Precio de venta *", numPrecio);
        lblPrecio = (Label)campos.GetControlFromPosition(0, campos.RowCount - 1);

        numStock = UI.Numero(1_000_000, 3, 120);
        numStock.Minimum = -1_000_000;
        numStock.Value = producto.Stock;
        numStock.Enabled = nuevo;
        UI.Campo(campos, nuevo ? "Stock inicial" : "Stock actual", numStock);
        if (!nuevo)
        {
            var aviso = UI.Etiqueta("Para cambiar el stock usá \"Ingreso / ajuste de stock\".", 8.5f);
            aviso.ForeColor = UI.Neutro;
            UI.Campo(campos, "", aviso);
        }

        numMinimo = UI.Numero(100_000, 3, 120);
        numMinimo.Value = producto.StockMinimo;
        UI.Campo(campos, "Stock mínimo (aviso, 0 = no avisar)", numMinimo);

        chkControla = new CheckBox { Text = "Controlar stock (desmarcar en presentaciones de helado)", AutoSize = true, Checked = producto.ControlaStock };
        UI.Campo(campos, "", chkControla);
        chkRapida = new CheckBox { Text = "Mostrar como botón en la pantalla de venta", AutoSize = true, Checked = producto.VentaRapida };
        UI.Campo(campos, "", chkRapida);
        var ayudaRapida = UI.Etiqueta("Marcalo para helados, pan por kilo y golosinas sueltas sin código.", 8.5f);
        ayudaRapida.ForeColor = UI.Neutro;
        UI.Campo(campos, "", ayudaRapida);

        chkControla.CheckedChanged += (s, e) => ActualizarUnidad();
        cmbUnidad.SelectedIndexChanged += (s, e) => ActualizarUnidad();
        ActualizarUnidad();

        // Costo o margen cambian → recalcula precio. Precio cambia → recalcula margen.
        numCosto.ValueChanged += (s, e) => CalcularPrecio();
        numMargen.ValueChanged += (s, e) => CalcularPrecio();
        numPrecio.ValueChanged += (s, e) => CalcularMargen();

        var barra = UI.BarraBotones(DockStyle.Bottom, true);
        var btnGuardar = UI.Boton("Guardar", UI.Exito, 130);
        UI.ColorearBoton(btnGuardar, UI.Exito, destacado: true);
        btnGuardar.Click += (s, e) => Guardar();
        var btnCancelar = UI.Boton("Cancelar", UI.Neutro, 130);
        btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        barra.Controls.Add(btnGuardar);
        barra.Controls.Add(btnCancelar);

        UI.Ensamblar(this, campos, barra);
        CancelButton = btnCancelar;
        Shown += (s, e) => { if (string.IsNullOrEmpty(txtCodigo.Text)) txtCodigo.Focus(); else txtNombre.Focus(); };
    }

    private void ActualizarUnidad()
    {
        bool kilo = cmbUnidad.SelectedIndex == 1;
        if (lblPrecio != null) lblPrecio.Text = kilo ? "Precio por KILO *" : "Precio de venta *";
        numStock.Enabled = producto.Id == 0 && chkControla.Checked;
        numMinimo.Enabled = chkControla.Checked;
    }

    private void CalcularPrecio()
    {
        if (calculando || numCosto.Value == 0) return;
        calculando = true;
        decimal precio = Math.Round(numCosto.Value * (1 + numMargen.Value / 100m), 2);
        numPrecio.Value = Math.Clamp(precio, numPrecio.Minimum, numPrecio.Maximum);
        calculando = false;
    }

    private void CalcularMargen()
    {
        if (calculando || numCosto.Value == 0) return;
        calculando = true;
        decimal margen = Math.Round((numPrecio.Value - numCosto.Value) / numCosto.Value * 100, 1);
        numMargen.Value = Math.Clamp(margen, numMargen.Minimum, numMargen.Maximum);
        calculando = false;
    }

    private void Guardar()
    {
        if (txtNombre.Text.Trim() == "") { UI.Advertir("El nombre es obligatorio."); txtNombre.Focus(); return; }
        if (numPrecio.Value <= 0) { UI.Advertir("El precio de venta debe ser mayor a cero."); numPrecio.Focus(); return; }
        if (numPrecio.Value < numCosto.Value &&
            !UI.Confirmar("El precio de venta es MENOR al costo (vas a perder plata).\n¿Guardar igual?"))
            return;

        producto.CodigoBarras = txtCodigo.Text.Trim();
        producto.Nombre = txtNombre.Text.Trim();
        producto.Categoria = cmbCategoria.Text.Trim() == "" ? "Almacén" : cmbCategoria.Text.Trim();
        producto.Unidad = cmbUnidad.SelectedIndex == 1 ? "KG" : "UNIDAD";
        producto.ControlaStock = chkControla.Checked;
        producto.VentaRapida = chkRapida.Checked;
        producto.PrecioCosto = numCosto.Value;
        producto.PrecioVenta = numPrecio.Value;
        producto.Stock = numStock.Value;
        producto.StockMinimo = numMinimo.Value;

        try
        {
            ProductoRepo.Guardar(producto);
        }
        catch (Exception ex) when (Db.EsDuplicado(ex))
        {
            UI.Advertir("Ya existe otro producto con ese código de barras.");
            txtCodigo.Focus();
            return;
        }
        catch (Exception ex)
        {
            UI.Error("No se pudo guardar: " + ex.Message);
            return;
        }

        ProductoId = producto.Id;
        DialogResult = DialogResult.OK;
        Close();
    }
}
