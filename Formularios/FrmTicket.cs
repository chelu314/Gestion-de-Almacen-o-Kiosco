namespace KioscoLaFamilia.Formularios;

/// <summary>Vista previa de un ticket con opciones de imprimir, copiar y enviar por WhatsApp.</summary>
public class FrmTicket : Form
{
    private readonly string texto;
    private string telefono;

    public FrmTicket(string titulo, string texto, string telefono = null)
    {
        this.texto = texto;
        this.telefono = telefono;
        UI.PrepararForm(this, titulo, 520, 620);
        // Comprobante en blanco, como el papel: sin el logo de fondo
        BackColor = Color.White;
        Tag = Marco.SinMarcaDeAgua;

        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical,
            Font = UI.FuenteMono,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Dock = DockStyle.Fill,
            Text = texto.Replace("\r\n", "\n").Replace("\n", "\r\n")
        };

        // "Papel" centrado: el ticket no se estira a lo ancho de toda la pantalla
        var papel = new Panel { BackColor = Color.White };
        papel.Controls.Add(txt);
        papel.Resize += (s, e) =>
        {
            int lado = Math.Max(16, (papel.Width - 520) / 2);
            papel.Padding = new Padding(lado, 10, lado, 10);
        };

        var barra = UI.BarraBotones();
        // (Copiar no hace falta: al mandar por WhatsApp el texto también queda copiado)
        var btnWsp = UI.BotonPrincipal("Enviar por WhatsApp", EnviarWhatsApp, 200, Tema.WhatsApp);
        var btnImprimir = UI.BotonSecundario("Imprimir", () => UI.Intentar(() => Impresion.Imprimir(this.texto, this), "No se pudo imprimir."), 130);
        var btnCerrar = UI.BotonSecundario("Cerrar", Close, 120, Tema.Neutro);
        barra.Controls.AddRange(new Control[] { btnWsp, btnImprimir, btnCerrar });

        if (Tema.Logo != null)
        {
            var logo = new PictureBox { Image = Tema.Logo, SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Top, Height = 110, BackColor = Color.White };
            UI.Ensamblar(this, papel, logo, barra);
        }
        else
        {
            UI.Ensamblar(this, papel, barra);
        }
        CancelButton = btnCerrar;
        Shown += (s, e) => { txt.SelectionStart = 0; txt.SelectionLength = 0; btnCerrar.Focus(); };
    }

    private void EnviarWhatsApp()
    {
        var numero = Dialogos.PedirTelefono(this, "Enviar por WhatsApp",
            "Número de WhatsApp: código de área sin 0 + número sin 15.\nEjemplo: 1181077056", telefono ?? "");
        if (numero == null) return;
        telefono = numero;

        if (WhatsApp.Enviar(numero, texto))
            UI.Info("Se abrió WhatsApp con el mensaje listo: solo tocá ENVIAR.\n\n" +
                    "La primera vez, WhatsApp Web pide vincular tu celular escaneando un código QR " +
                    "(en el celular: WhatsApp → Dispositivos vinculados). Después ya queda abierto.");
    }
}
