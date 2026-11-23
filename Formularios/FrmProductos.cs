namespace KioscoLaFamilia.Formularios;

/// <summary>
/// ABM de productos, precios y aumentos masivos.
/// Las columnas Código, Producto, Categoría, Costo, Precio y Mínimo se editan
/// directamente en la grilla; el botón "Actualizar" guarda todos los cambios juntos.
/// </summary>
public class FrmProductos : Form
{
    private static readonly string[] Editables = { "Código", "Producto", "Categoría", "Costo", "Precio", "Mínimo" };
    private static readonly string[] Decimales = { "Costo", "Precio", "Mínimo" };
    private static readonly Color ColorEditado = Color.FromArgb(255, 243, 176);

    private readonly TextBox txtFiltro;
    private readonly CheckBox chkStockBajo;
    private readonly ComboBox cmbCategoria;
    private readonly DataGridView grid;
    private readonly Label lblCantidad;
    private readonly Button btnActualizar;
    private bool cargando;

    public FrmProductos()
    {
        UI.PrepararForm(this, "Productos y precios", 1150, 650);
        WindowState = FormWindowState.Maximized;

        var filtros = UI.BarraFiltros();
        filtros.Controls.Add(UI.Etiqueta("Tipo:", 10f, true));
        cmbCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Margin = new Padding(4, 6, 4, 4) };
        cmbCategoria.Items.Add("(Todos)");
        try { foreach (var c in ProductoRepo.Categorias()) cmbCategoria.Items.Add(c); } catch { }
        cmbCategoria.SelectedIndex = 0;
        cmbCategoria.SelectedIndexChanged += (s, e) => Cargar();
        filtros.Controls.Add(cmbCategoria);
        filtros.Controls.Add(UI.Etiqueta("Buscar:", 10f, true));
        txtFiltro = UI.Texto(260);
        txtFiltro.TextChanged += (s, e) => Cargar();
        filtros.Controls.Add(txtFiltro);
        chkStockBajo = new CheckBox { Text = "Solo stock bajo", AutoSize = true, Margin = new Padding(14, 9, 4, 4) };
        chkStockBajo.CheckedChanged += (s, e) => Cargar();
        filtros.Controls.Add(chkStockBajo);
        lblCantidad = UI.Etiqueta("");
        filtros.Controls.Add(lblCantidad);

        grid = UI.Grilla();
        UI.Configurar(grid, ocultas: new[] { "id", "bajo" },
            cantidades: new[] { "Stock", "Mínimo" },
            extra: g =>
            {
                foreach (DataGridViewColumn c in g.Columns)
                {
                    bool editable = Editables.Contains(c.Name);
                    c.ReadOnly = !editable;
                    if (editable)
                    {
                        c.HeaderText = c.Name + " ✎";
                        c.DefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
                    }
                    if (c.Name == "Costo" || c.Name == "Precio")
                    {
                        c.DefaultCellStyle.Format = "N2";
                        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
                foreach (DataGridViewRow r in g.Rows)
                {
                    if (Convert.ToInt32(r.Cells["bajo"].Value) == 1)
                    {
                        r.DefaultCellStyle.ForeColor = UI.Peligro;
                        r.DefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                    }
                }
                if (g.Columns.Contains("Producto")) g.Columns["Producto"].FillWeight = 220;
            });
        grid.ReadOnly = false;
        grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        // Doble clic en una columna que no se edita en la grilla abre la ficha completa
        grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].ReadOnly) Editar();
        };
        // Números en formato argentino: 1.500,50 o $ 1.500
        grid.CellParsing += (s, e) =>
        {
            if (!Decimales.Contains(grid.Columns[e.ColumnIndex].Name) || e.Value == null) return;
            var texto = e.Value.ToString().Replace("$", "").Trim();
            if (texto == "") { e.Value = 0m; e.ParsingApplied = true; return; }
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out var d) && d >= 0)
            {
                e.Value = d;
                e.ParsingApplied = true;
            }
        };
        grid.DataError += (s, e) =>
        {
            e.ThrowException = false;
            UI.Advertir("Valor no válido. Escribí un número, por ejemplo 1500 o 1.500,50.");
        };
        grid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = ColorEditado;
            ActualizarEstadoCambios();
        };

        var barra = UI.BarraBotones();
        btnActualizar = Boton("Actualizar (Ctrl+G)", UI.Exito, () => GuardarCambios(), 180);
        barra.Controls.Add(btnActualizar);
        barra.Controls.Add(UI.BotonSecundario("Nuevo producto (Ins)", Nuevo, 190));
        barra.Controls.Add(UI.BotonSecundario("Aumentar precios %", AumentoMasivo, 180));
        // Lo que se usa poco va en "Más opciones" (y con clic derecho sobre un producto)
        var opciones = new[]
        {
            new UI.Opcion("Ver ficha completa", Editar),
            new UI.Opcion("Historial de precios", VerHistorial),
            new UI.Opcion("Exportar a Excel", () => UI.ExportarCsv(grid, "productos")),
            UI.Separador,
            new UI.Opcion("Eliminar producto", Eliminar, Peligrosa: true)
        };
        barra.Controls.Add(UI.BotonMas(opciones));
        UI.MenuEnGrilla(grid, opciones);

        UI.Ensamblar(this, grid, filtros, barra);

        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.G) { e.SuppressKeyPress = true; grid.EndEdit(); GuardarCambios(); }
            else if (grid.IsCurrentCellInEditMode) return;   // mientras se edita una celda, Enter y Esc son de la celda
            else if (e.KeyCode == Keys.Insert) Nuevo();
            else if (e.KeyCode == Keys.Escape) Close();
        };
        FormClosing += (s, e) => { if (!ResolverCambiosPendientes(true)) e.Cancel = true; };
        Load += (s, e) => Cargar();
    }

    private static Button Boton(string texto, Color color, Action accion, int ancho = 140)
    {
        var b = UI.Boton(texto, color, ancho);
        b.Click += (s, e) => accion();
        return b;
    }

    private DataTable Tabla => grid.DataSource as DataTable;

    private bool HayCambios
    {
        get
        {
            if (grid.IsCurrentCellInEditMode) grid.EndEdit();
            return Tabla?.GetChanges(DataRowState.Modified)?.Rows.Count > 0;
        }
    }

    private void ActualizarEstadoCambios()
    {
        if (cargando) return;
        int n = Tabla?.GetChanges(DataRowState.Modified)?.Rows.Count ?? 0;
        btnActualizar.Text = n > 0 ? $"Actualizar ({n}) Ctrl+G" : "Actualizar (Ctrl+G)";
        UI.ColorearBoton(btnActualizar, n > 0 ? UI.Advertencia : UI.Exito, destacado: n > 0);
    }

    /// <summary>Si hay cambios sin guardar pregunta qué hacer. Devuelve false si el usuario cancela.</summary>
    private bool ResolverCambiosPendientes(bool permitirCancelar)
    {
        if (!HayCambios) return true;
        var r = MessageBox.Show("Hay cambios sin guardar en la lista.\n\n¿Guardarlos ahora?", UI.Negocio,
            permitirCancelar ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r == DialogResult.Cancel) return false;
        if (r == DialogResult.Yes) return GuardarCambios(recargar: false);
        return true;
    }

    /// <summary>Guarda todas las filas modificadas en la grilla.</summary>
    private bool GuardarCambios() => GuardarCambios(recargar: true);

    private bool GuardarCambios(bool recargar)
    {
        if (grid.IsCurrentCellInEditMode) grid.EndEdit();
        var cambios = Tabla?.GetChanges(DataRowState.Modified);
        if (cambios == null || cambios.Rows.Count == 0)
        {
            if (recargar) UI.Info("No hay cambios para guardar.\n\nHacé clic en una celda con ✎, escribí el dato nuevo y después tocá Actualizar.");
            return true;
        }

        int ok = 0;
        var errores = new List<string>();
        foreach (DataRow r in cambios.Rows)
        {
            var p = ProductoRepo.ObtenerPorId(r.Int("id"));
            if (p == null) continue;
            string nombre = r.Str("Producto").Trim();
            decimal precio = r.Dec("Precio");
            if (nombre == "") { errores.Add($"• Un producto quedó sin nombre (se dejó como \"{p.Nombre}\")."); continue; }
            if (precio <= 0) { errores.Add($"• {nombre}: el precio debe ser mayor a cero."); continue; }

            p.CodigoBarras = r.Str("Código").Trim();
            p.Nombre = nombre;
            p.Categoria = r.Str("Categoría").Trim() == "" ? p.Categoria : r.Str("Categoría").Trim();
            p.PrecioCosto = r.Dec("Costo");
            p.PrecioVenta = precio;
            p.StockMinimo = r.Dec("Mínimo");
            try
            {
                ProductoRepo.Guardar(p);
                ok++;
            }
            catch (Exception ex) when (Db.EsDuplicado(ex))
            {
                errores.Add($"• {nombre}: el código {p.CodigoBarras} ya lo tiene otro producto.");
            }
            catch (Exception ex)
            {
                errores.Add($"• {nombre}: {ex.Message}");
            }
        }

        if (errores.Count == 0)
        {
            if (recargar) UI.Info($"Listo: se actualizaron {ok} producto(s).");
            Tabla.AcceptChanges();
        }
        else
        {
            UI.Advertir($"Se actualizaron {ok} producto(s). No se pudieron guardar:\n\n" + string.Join("\n", errores));
        }
        if (recargar) Recargar();
        return errores.Count == 0;
    }

    /// <summary>Recarga la lista (pidiendo antes guardar los cambios pendientes).</summary>
    private void Cargar()
    {
        if (cargando) return;
        ResolverCambiosPendientes(false);
        Recargar();
    }

    private void Recargar()
    {
        cargando = true;
        try
        {
            UI.Intentar(() =>
            {
                string cat = cmbCategoria.SelectedIndex <= 0 ? null : cmbCategoria.SelectedItem.ToString();
                var t = ProductoRepo.Tabla(txtFiltro.Text.Trim(), chkStockBajo.Checked, cat);
                foreach (DataColumn c in t.Columns) c.ReadOnly = false;
                grid.DataSource = t;
                lblCantidad.Text = $"{t.Rows.Count} producto(s)  —  hacé clic en una celda con ✎ para editarla";
            });
        }
        finally
        {
            cargando = false;
            ActualizarEstadoCambios();
        }
    }

    private void Nuevo()
    {
        if (!ResolverCambiosPendientes(true)) return;
        using var f = new FrmProductoEditar(null);
        if (f.ShowDialog(this) == DialogResult.OK) Recargar();
    }

    private void Editar()
    {
        if (!ResolverCambiosPendientes(true)) return;
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var p = ProductoRepo.ObtenerPorId(id.Value);
        using var f = new FrmProductoEditar(p);
        if (f.ShowDialog(this) == DialogResult.OK) Recargar();
    }

    private void Eliminar()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var p = ProductoRepo.ObtenerPorId(id.Value);
        if (!UI.Confirmar($"¿Eliminar \"{p.Nombre}\"?\n\nSe conserva en el historial de ventas, pero ya no aparecerá para vender.")) return;
        if (UI.Intentar(() => ProductoRepo.Eliminar(p.Id))) Recargar();
    }

    private void AumentoMasivo()
    {
        if (!ResolverCambiosPendientes(true)) return;
        var categorias = new List<string> { "(Todas las categorías)" };
        categorias.AddRange(ProductoRepo.Categorias());
        var cat = Dialogos.Elegir(this, "Aumento de precios", "¿A qué productos aplicar el cambio?", categorias);
        if (cat == null) return;

        var pct = Dialogos.PedirNumero(this, "Aumento de precios",
            "Porcentaje de aumento (usá un número negativo para bajar precios):", 10, -90, 500, 2);
        if (pct == null || pct.Value == 0) return;

        bool tambienCosto = UI.Confirmar("¿Aplicar el mismo porcentaje también al precio de COSTO?\n\n" +
                                         "(Elegí Sí si el aumento viene del proveedor.)");
        string categoria = cat.StartsWith("(") ? null : cat;

        if (!UI.Confirmar($"Se va a modificar en {pct.Value:0.##}% el precio de venta de " +
                          (categoria == null ? "TODOS los productos" : $"la categoría \"{categoria}\"") +
                          (tambienCosto ? " (y también el costo)" : "") + ".\n\n¿Continuar?"))
            return;

        UI.Intentar(() =>
        {
            int n = ProductoRepo.AumentarPrecios(pct.Value, categoria, tambienCosto);
            UI.Info($"Listo: se actualizaron {n} producto(s).");
            Recargar();
        });
    }

    private void VerHistorial()
    {
        var id = UI.IdSeleccionado(grid);
        if (id == null) return;
        var p = ProductoRepo.ObtenerPorId(id.Value);

        using var f = new Form();
        UI.PrepararForm(f, "Historial de precios: " + p.Nombre, 760, 420);
        var g = UI.Grilla();
        UI.Configurar(g, moneda: new[] { "Costo anterior", "Costo nuevo", "Precio anterior", "Precio nuevo" }, fechas: new[] { "Fecha" });
        f.Controls.Add(g);
        f.Load += (s, e) => g.DataSource = ProductoRepo.HistorialPrecios(p.Id);
        f.ShowDialog(this);
    }
}
