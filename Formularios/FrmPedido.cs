namespace KioscoLaFamilia.Formularios;

/// <summary>
/// Pedido MANUAL a un proveedor: vos elegís qué pedir y cuánto.
/// Al final se arma el mensaje y se manda por WhatsApp al número del proveedor.
/// </summary>
public class FrmPedido : Form
{
    private readonly Proveedor proveedor;
    private readonly TextBox txtProducto;
    private readonly TextBox txtCantidad;
    private readonly DataGridView grid;

    public FrmPedido(Proveedor p)
    {
        proveedor = p;
        UI.PrepararForm(this, "Pedido a " + p.Nombre, 900, 600);

        // ---------- ARRIBA: qué producto y cuánto ----------
        var filtros = UI.BarraFiltros();

        filtros.Controls.Add(UI.Etiqueta("Producto:", 10f, true));
        txtProducto = UI.Texto(340);
        // Autocompletar: al escribir "lec" te sugiere los productos cargados que empiezan así.
        // Igual podés escribir cualquier cosa, aunque no esté cargada en el programa.
        txtProducto.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        txtProducto.AutoCompleteSource = AutoCompleteSource.CustomSource;
        filtros.Controls.Add(txtProducto);

        filtros.Controls.Add(UI.Etiqueta("Cantidad:", 10f, true));
        txtCantidad = UI.Texto(140);
        // Es texto libre a propósito: "10", "2 cajas", "1 bulto", "5 kg"...
        txtCantidad.PlaceholderText = "ej: 2 cajas";
        filtros.Controls.Add(txtCantidad);

        var btnAgregar = UI.BotonSecundario("Agregar (Enter)", Agregar, 160);
        filtros.Controls.Add(btnAgregar);

        // Enter en Producto → salta a Cantidad. Enter en Cantidad → agrega a la lista.
        txtProducto.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtCantidad.Focus(); }
        };
        txtCantidad.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Agregar(); }
        };

        // ---------- CENTRO: la lista del pedido ----------
        grid = UI.Grilla();
        grid.ReadOnly = false;               // se puede corregir escribiendo en la celda
        grid.AllowUserToDeleteRows = true;   // Supr borra el renglón elegido
        grid.Columns.Add("Producto", "Producto");
        grid.Columns.Add("Cantidad", "Cantidad");
        grid.Columns["Producto"].FillWeight = 75;   // 75% del ancho para el nombre
        grid.Columns["Cantidad"].FillWeight = 25;

        // ---------- ABAJO: botones ----------
        var barra = UI.BarraBotones();
        barra.Controls.Add(UI.BotonPrincipal("Enviar por WhatsApp", Enviar, 210, Tema.WhatsApp));
        barra.Controls.Add(UI.BotonSecundario("Quitar producto (Supr)", Quitar, 210));
        barra.Controls.Add(UI.BotonSecundario("Cancelar", Close, 130, Tema.Neutro));

        UI.Ensamblar(this, grid, filtros, barra);
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        Load += (s, e) =>
        {
            // Cargamos los nombres de los productos para el autocompletar
            UI.Intentar(() =>
            {
                var nombres = ProductoRepo.Buscar("").Select(x => x.Nombre).ToArray();
                txtProducto.AutoCompleteCustomSource.AddRange(nombres);
            });
            txtProducto.Focus();
        };
    }

    /// <summary>Pasa lo escrito arriba a la lista y deja todo listo para el próximo.</summary>
    private void Agregar()
    {
        string producto = txtProducto.Text.Trim();
        string cantidad = txtCantidad.Text.Trim();
        if (producto == "") { txtProducto.Focus(); return; }
        if (cantidad == "") { UI.Advertir("Escribí la cantidad (ej: 10 o 2 cajas)."); txtCantidad.Focus(); return; }

        grid.Rows.Add(producto, cantidad);
        txtProducto.Clear();
        txtCantidad.Clear();
        txtProducto.Focus();
    }

    private void Quitar()
    {
        if (grid.CurrentRow == null) return;
        grid.Rows.Remove(grid.CurrentRow);
    }

    /// <summary>Arma el texto del pedido y abre la vista previa con el botón de WhatsApp.</summary>
    private void Enviar()
    {
        grid.EndEdit();   // por si quedó una celda a medio escribir

        // Juntamos los renglones de la grilla en una lista (producto, cantidad)
        var items = new List<(string Producto, string Cantidad)>();
        foreach (DataGridViewRow r in grid.Rows)
        {
            string prod = r.Cells["Producto"].Value?.ToString()?.Trim() ?? "";
            string cant = r.Cells["Cantidad"].Value?.ToString()?.Trim() ?? "";
            if (prod != "") items.Add((prod, cant));
        }
        if (items.Count == 0) { UI.Info("Agregá al menos un producto al pedido."); txtProducto.Focus(); return; }

        var texto = Ticket.PedidoManual(proveedor.Nombre, items);
        // FrmTicket ya sabe mostrar el texto, validar el número y abrir WhatsApp
        using var f = new FrmTicket("Pedido a " + proveedor.Nombre, texto, proveedor.Telefono);
        f.ShowDialog(this);
    }
}
