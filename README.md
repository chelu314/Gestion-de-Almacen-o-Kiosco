# Almacén La Tutti — Sistema de gestión (C# + MySQL)

## Qué necesitás instalar
1. **Visual Studio 2022** (Community es gratis) con la carga de trabajo **"Desarrollo de escritorio de .NET"**.
2. **MySQL 8** (o **XAMPP**, que trae MySQL/MariaDB) y un cliente: MySQL Workbench o phpMyAdmin.

> **¿Ya tenías la base creada con la versión anterior?** Ejecutá `BaseDeDatos/03_actualizar_v2.sql` (agrega venta por kilo y botones rápidos). Si la creás de cero, no hace falta.

> **Productos reales del almacén:** ejecutá `BaseDeDatos/04_productos_almacen.sql` (53 productos revisados en la planilla; también cambia el nombre del negocio a "Almacén La Tutti"). No ejecutes `02_datos_ejemplo.sql` en la base real: es solo para probar.

## Puesta en marcha (4 pasos)
1. Iniciá MySQL (en XAMPP: botón *Start* de MySQL).
2. Abrí `KioscoLaFamilia.sln` en Visual Studio. El paquete **MySql.Data** se descarga solo (NuGet).
3. Presioná **F5**. Si no hay conexión aparece la pantalla **Conexión a la base de datos**: cargá servidor, puerto, usuario y contraseña de TU MySQL, tocá **Crear la base de datos** (opcional: cargar los productos) y **Guardar y usar**.
4. Ingresá con **admin / admin123** y cambiá la contraseña desde *Mi cuenta*.

Cada computadora guarda sus datos de conexión en `conexion.txt` (junto al .exe), así cada uno usa su propio servidor.
También se puede cambiar desde el login: **Configurar conexión al servidor**.

## Crear cuenta desde el login
En el login está **"¿No tenés usuario? Crear cuenta"**: nombre, usuario y contraseña (dos veces). La cuenta se crea como **cajero**.
Por defecto queda pendiente hasta que un administrador la active en **Usuarios → Activar / desactivar**. Se puede cambiar en **Configuración**.

## Formularios con Diseñador de Visual Studio
`FrmLogin`, `FrmCrearCuenta` y `FrmConexion` están separados como los hace Visual Studio:
`FrmX.cs` (lógica) + `FrmX.Designer.cs` (diseño: posiciones, tamaños, textos). Se abren con doble clic y se editan arrastrando.
Los colores igual salen de `Diseno/Tema.cs` al ejecutar.
El resto de las pantallas están armadas por código en un solo archivo (sin Designer).

## Cómo están organizados los botones
- **Un botón principal por pantalla**, en color fuerte (Cobrar, Ver cuenta y cobrar, Registrar pago, Cierre de caja, Nuevo usuario…).
- Los de uso frecuente van en tono suave al lado.
- Lo que se usa poco (exportar, historial, eliminar…) está en **Más opciones ▾** y también con **clic derecho** sobre la fila.
- Los filtros de fechas se aplican solos (lista **Período**): no hay botones "Ver" ni "Buscar".
- Las ventanas se cierran con **Esc** o la X.
- Menú principal: 6 accesos de trabajo diario; Mi cuenta, Usuarios, Configuración y Cerrar sesión arriba a la derecha. El aviso de stock bajo aparece arriba en rojo solo cuando hace falta.

## Editar productos directo en la lista
En **Productos y precios**, las columnas con ✎ (Código, Producto, Categoría, Costo, Precio, Mínimo) se editan haciendo clic en la celda y escribiendo.
Las celdas cambiadas se pintan de amarillo y el botón **Actualizar** muestra cuántos cambios hay; al tocarlo (o Ctrl+G) se guardan todos juntos.
Para cargar el código de barras: clic en la celda Código del producto y escanearlo.
Un **mínimo en 0** significa "no avisar stock bajo".

## Comprobante de pago
Al registrar un pago en la cuenta de un cliente, el sistema ofrece mandarle el **comprobante por WhatsApp** (si la deuda queda en cero dice "DEUDA CANCELADA").
También está el botón **Enviar comprobante de pago** en la cuenta del cliente y en la lista de clientes (manda el último pago).

## Cambiar el diseño
Todo lo visual está separado de la lógica, en **`Diseno/Tema.cs`**:
- Colores (tomados del logo), colores de cada botón del menú, letras.
- Recuadro de los formularios (mostrar sí/no, color, grosor).
- Encabezado con logo de cada pantalla (mostrar sí/no, alto, colores).
- Marca de agua (sí/no, tamaño) y logo en el ticket impreso (sí/no, tamaño).
- Pantalla completa (sí/no), también para las pantallas de cargar/editar.
- Botones suaves (sí/no y cuánto): fondo claro con letra del color. La acción principal (Cobrar, Ingresar, Guardar) se ve fuerte igual.

Las imágenes están en **`Recursos/`**: `logo.png`, `logo_marca_agua.png` y `logo.ico` (ícono de las ventanas y del .exe).
Para cambiar el logo, reemplazá esos archivos con el mismo nombre. `Diseno/Marco.cs` es el que dibuja el recuadro y el encabezado en todas las pantallas automáticamente.

## Estructura
```
Program.cs                 Arranque, cultura es-AR y ciclo de login
Modelos/                   Clases Producto, Cliente, Usuario, ItemCarrito, etc.
Datos/                     Acceso a MySQL (Db) y repositorios por entidad
Servicios/                 Hash de contraseñas, tickets, WhatsApp, impresión
Diseno/                    Tema.cs (colores, letras, logo) y Marco.cs (recuadro y encabezado)
Recursos/                  Logo, marca de agua e ícono
Utilidades/                Ayudas para armar pantallas, diálogos y gráfico de barras
Formularios/               Todas las pantallas (armadas por código)
BaseDeDatos/               Scripts SQL
```
Los formularios están hechos 100 % por código (sin archivos .Designer), así se leen y modifican fácil.

## Cómo se vende cada tipo de producto
| Tipo | Cómo se carga en *Productos* | Cómo se vende |
|---|---|---|
| Almacén, limpieza, bebidas, lácteos envasados (leche, yogur, queso crema) | Con su **código de barras** (el que trae el envase), "Se vende: Por unidad" | Se **escanea** |
| Golosinas con envoltorio (alfajores, chocolates) | Con código de barras | Se escanea |
| Golosinas sueltas (caramelos, chicles sueltos) | **Sin código**, por unidad | **Botón** en la pantalla de venta (cantidad en "Cant." antes de tocarlo) |
| Pan, fiambre, cosas que se pesan | Sin código, "Se vende: **Por kilo**", precio por kilo | Botón → se escribe el **peso en gramos** que marca la balanza (hay atajos 100 g / 250 g / 500 g / 1 kg) |
| Facturas, panes por unidad | Sin código, por unidad | Botón |
| Helados: 1 kg, ½ kg, ¼ kg, cono, vaso | Un producto por presentación, sin código, **desmarcar "Controlar stock"** | Pestaña **Helados** → tocar cada presentación las veces que haga falta |

En la pantalla de venta aparece una fila de pestañas por tipo (Helados, Panes, Golosinas…) con los botones de esos productos.
Todo queda registrado igual que lo escaneado: ticket, stock, ganancia y reportes (incluido **Ventas por tipo de producto**).

## Atajos de teclado
| Dónde | Tecla | Acción |
|---|---|---|
| Menú | F1 / F2 / F3 | Vender / Productos / Clientes |
| Venta | Enter | Agregar el código escaneado |
| Venta | `3*779...` | Cargar 3 unidades de ese código |
| Venta | F3 | Buscar producto por nombre |
| Venta | F4 | Ir a "Paga con" |
| Venta | F12 | Cobrar |
| Venta | Supr | Quitar renglón (con la grilla seleccionada) |
| Venta | Esc | Cancelar venta / salir |
| Productos | Insert / Enter | Nuevo / Editar |

## Lector de código de barras
Cualquier lector USB funciona: actúa como un teclado que escribe el código y presiona Enter.
Si escaneás un código que no existe (siendo admin), el sistema ofrece darlo de alta en el momento.

## WhatsApp
Los teléfonos se guardan **con código de área, sin 0 y sin 15** (10 números, ej. `1181077056`). Los campos de teléfono no aceptan letras y el programa avisa si el número está mal (le sobra el 0, el 15 o le faltan cifras).
Por defecto se abre **WhatsApp Web** directo en el navegador con el mensaje escrito: solo tocás **Enviar**. La primera vez hay que vincular el celular (WhatsApp → Dispositivos vinculados → escanear el QR).
En **Configuración → Abrir WhatsApp con** se puede elegir la aplicación instalada.

## Roles
- **admin**: todo.
- **cajero**: vender, clientes y cobros, ver stock bajo, historial y cierre de caja, cambiar su usuario/contraseña.

## Problemas comunes
- *"No se pudo conectar a la base de datos"*: MySQL apagado, falta ejecutar el script, o la contraseña de `conexion.txt` no coincide.
- *Error de autenticación con MySQL 8*: probá crear un usuario propio para el sistema en Workbench y ponelo en `conexion.txt` (`uid=...;pwd=...;`).
