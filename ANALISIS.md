# Almacén La Tutti — Análisis de requerimientos

## 1. Objetivo
Sistema de escritorio (C# Windows Forms + MySQL) para administrar un kiosco/almacén:
ventas con lector de código de barras, control de stock, clientes con cuenta corriente
("fiado"), envío de tickets por WhatsApp y reportes de ventas y ganancias por mes.

## 2. Actores
| Actor | Qué puede hacer |
|---|---|
| **Administrador** | Todo: productos y precios, stock, usuarios, configuración, reportes de ganancias, anular ventas. |
| **Cajero** | Vender, consultar stock bajo, clientes y cuentas, registrar pagos, ver historial del día, cambiar su usuario y contraseña. |

## 3. Requerimientos funcionales

### RF1 — Seguridad / usuarios
- RF1.1 Login con usuario y contraseña (contraseña guardada con hash PBKDF2 + salt, nunca en texto plano).
- RF1.2 Bloqueo temporal de 30 s tras 5 intentos fallidos.
- RF1.3 Cada usuario puede cambiar **su nombre de usuario y su contraseña** (pidiendo la contraseña actual).
- RF1.4 El administrador crea usuarios, asigna rol, resetea contraseñas y activa/desactiva cuentas.
- RF1.5 En la primera ejecución se crea `admin` / `admin123` (se pide cambiarla).

### RF2 — Productos y precios
- RF2.1 Alta, modificación y baja (lógica) de productos: código de barras, nombre, categoría, precio de costo, precio de venta, stock y stock mínimo.
- RF2.2 El código de barras se carga escaneándolo; es único.
- RF2.3 Cálculo automático del precio a partir del costo + % de margen (y al revés).
- RF2.4 **Aumento masivo de precios por %** (todos o por categoría).
- RF2.5 Historial de cambios de precio (quién, cuándo, valor anterior y nuevo).

### RF3 — Ventas (punto de venta)
- RF3.1 Agregar productos escaneando el código (el lector funciona como teclado + Enter).
- RF3.2 Atajo `3*779...` para cargar cantidad; búsqueda por nombre (F3).
- RF3.3 Modificar cantidades, quitar ítems, ver total en grande.
- RF3.4 Formas de pago: efectivo (calcula vuelto), transferencia/QR, débito, crédito, **cuenta corriente**.
- RF3.5 Al confirmar se guarda la venta con fecha y hora, cajero, detalle, **costo al momento de la venta** (para calcular ganancia real) y se descuenta el stock — todo en una transacción.
- RF3.6 Ticket: ver, imprimir (impresora térmica o común), copiar y **enviar por WhatsApp**.
- RF3.7 Anulación de ventas (admin) con devolución automática del stock.

### RF3b — Tipos de producto y formas de venta (v2)
- Tipos predeterminados: Almacén, Bebidas, Golosinas, Helados, Lácteos, Limpieza, Panes (se pueden agregar otros).
- Productos **por unidad** (con o sin código de barras) y **por kilo** (precio por kg; al vender se ingresa el peso en gramos).
- **Botones de venta rápida** agrupados por tipo para productos sin código: helados (1 kg, ½, ¼, cono, vaso), pan, golosinas sueltas.
- Productos que **no controlan stock** (presentaciones de helado).
- Filtro de productos por tipo y reporte de **ventas y ganancia por tipo de producto**.

### RF3c — Mejoras v3
- Edición directa en la lista de productos (código, nombre, tipo, costo, precio, mínimo) con botón **Actualizar** que guarda todos los cambios.
- Tipos nuevos: Snacks y Perfumería. Stock mínimo 0 = no avisar.
- Botones de venta rápida solo para productos marcados (helados, pan por kilo, golosinas sueltas).
- Todas las pantallas a pantalla completa con el logo como marca de agua (configurable en `Diseno/Tema.cs`).
- **Comprobante de pago** por WhatsApp al cliente; indica "DEUDA CANCELADA" cuando el saldo queda en cero.

### RF4 — Stock
- RF4.1 Ingreso de mercadería (suma), ajuste por conteo (fija el stock real), baja por pérdida/vencimiento (resta).
- RF4.2 Todo movimiento queda registrado con usuario, fecha y stock resultante.
- RF4.3 **Aviso de poco stock**: al iniciar el sistema y después de cada venta que deje un producto en o por debajo del mínimo.
- RF4.4 Pantalla de stock bajo con cantidad sugerida a pedir y envío de la **lista de pedido por WhatsApp** al proveedor.

### RF5 — Clientes y cuenta corriente
- RF5.1 Alta/modificación/baja de clientes (nombre, teléfono WhatsApp, dirección, límite de crédito).
- RF5.2 Ventas "fiadas" asociadas al cliente; aviso si supera su límite.
- RF5.3 **Estado de cuenta**: lista de productos llevados con fecha, hora, cantidad, precio y subtotal, total de compras, pagos y **saldo a pagar**.
- RF5.4 Botón "Solo lo adeudado": calcula automáticamente desde qué compra se debe.
- RF5.5 Registrar pagos (totales o parciales) con recibo.
- RF5.6 Enviar el estado de cuenta o el comprobante de pago por **WhatsApp**.

### RF6 — Reportes
- RF6.1 Ventas por mes de un año: cantidad de ventas, total vendido, costo, **ganancia** y margen.
- RF6.2 **Mes con más y con menos ventas**, total anual, ganancia anual y promedio mensual.
- RF6.3 Gráfico de barras ventas vs. ganancia.
- RF6.4 Productos más vendidos en un período; productos sin ventas en N días.
- RF6.5 Historial de ventas por fecha y **cierre de caja** (totales por forma de pago + cobros de cuentas).
- RF6.6 Exportar cualquier listado a CSV (se abre con Excel).

### RF7 — Configuración
- Nombre, dirección y teléfono del negocio (salen en el ticket), código de país para WhatsApp, ancho del ticket, pie del ticket y avisos.

## 4. Requerimientos no funcionales
- **Plataforma**: Windows 10/11, .NET 8, Visual Studio 2022.
- **Base de datos**: MySQL 8 (o MariaDB vía XAMPP).
- **Integridad**: ventas, anulaciones y movimientos de stock en transacciones.
- **Seguridad**: contraseñas con hash; consultas parametrizadas (sin inyección SQL); permisos por rol.
- **Usabilidad**: pensado para teclado y lector (F1 vender, F3 buscar, F12 cobrar, Esc cancelar, Supr quitar).
- **Moneda/formato**: pesos argentinos, formato `es-AR`.

## 5. Hardware sugerido
- Lector de código de barras USB (modo teclado, con Enter al final — viene así de fábrica).
- Impresora térmica de 58 mm (ticket de 32 caracteres) u 80 mm (42–48 caracteres).
- WhatsApp Desktop o WhatsApp Web abierto en la PC.

## 6. Sobre WhatsApp
El sistema abre WhatsApp con el ticket **ya escrito** para el número del cliente; solo hay que tocar **Enviar**.
El envío 100 % automático requiere la API oficial de WhatsApp Business (paga y con alta en Meta); queda como mejora futura.

## 7. Modelo de datos
```
usuarios(id, usuario, nombre, hash, salt, rol, activo)
productos(id, codigo_barras, nombre, categoria, precio_costo, precio_venta, stock, stock_minimo, activo)
clientes(id, nombre, telefono, direccion, limite_credito, observaciones, activo)
ventas(id, fecha, usuario_id, cliente_id, total, costo_total, forma_pago, pagado_con, vuelto, estado)
venta_detalle(id, venta_id, producto_id, descripcion, cantidad, precio_unitario, costo_unitario, subtotal)
pagos_clientes(id, cliente_id, fecha, monto, forma_pago, usuario_id, observacion)
movimientos_stock(id, producto_id, fecha, tipo, cantidad, stock_resultante, usuario_id, observacion)
historial_precios(id, producto_id, fecha, costo_anterior, precio_anterior, costo_nuevo, precio_nuevo, usuario_id)
configuracion(clave, valor)
```
- **Saldo del cliente** = Σ ventas en cuenta corriente activas − Σ pagos.
- **Ganancia** = Σ (total − costo_total) de ventas activas. Se guarda el costo de cada ítem al vender, así un aumento posterior no altera las ganancias pasadas.
- Las ventas fiadas cuentan como venta del mes en que se entregó la mercadería.

## 8. Mejoras futuras posibles
- Venta por peso (fiambre, golosinas sueltas) con balanza.
- Proveedores y órdenes de compra.
- Control de vencimientos por lote.
- Facturación electrónica AFIP/ARCA.
- Backup automático de la base.
- Envío automático por API de WhatsApp Business.
