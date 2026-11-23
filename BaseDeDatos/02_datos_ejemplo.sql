-- Datos de ejemplo (opcional) para probar el sistema.
USE kiosco_la_familia;

-- Con código de barras (se escanean)
INSERT IGNORE INTO productos (codigo_barras, nombre, categoria, precio_costo, precio_venta, unidad, stock, stock_minimo) VALUES
 ('7790895000997', 'Coca-Cola 500 ml',          'Bebidas',    900.00, 1400.00, 'UNIDAD', 24, 6),
 ('7790895001000', 'Coca-Cola 2,25 L',          'Bebidas',   2100.00, 3200.00, 'UNIDAD', 12, 4),
 ('7790315000012', 'Agua mineral 500 ml',       'Bebidas',    450.00,  800.00, 'UNIDAD', 30, 10),
 ('7790580660003', 'Alfajor triple chocolate',  'Golosinas',  550.00,  950.00, 'UNIDAD', 40, 12),
 ('7790040133003', 'Galletitas de agua',        'Almacén',    700.00, 1150.00, 'UNIDAD',  3, 5),
 ('7790070012345', 'Yerba mate 1 kg',           'Almacén',   3200.00, 4600.00, 'UNIDAD',  8, 4),
 ('7791234500011', 'Leche entera 1 L',          'Lácteos',    950.00, 1350.00, 'UNIDAD',  2, 6),
 ('7791234500028', 'Yogur bebible frutilla 1 L','Lácteos',   1300.00, 1950.00, 'UNIDAD',  6, 3),
 ('7791234500035', 'Queso crema 290 g',         'Lácteos',   1700.00, 2500.00, 'UNIDAD',  5, 2),
 ('7790742000014', 'Pan lactal',                'Panes',     1600.00, 2400.00, 'UNIDAD',  6, 3),
 ('7791290000017', 'Lavandina 1 L',             'Limpieza',   600.00, 1000.00, 'UNIDAD', 10, 4),
 ('7791290000024', 'Detergente 500 ml',         'Limpieza',   900.00, 1500.00, 'UNIDAD',  8, 3);

-- Sin código: aparecen como BOTONES en la pantalla de venta
INSERT INTO productos (codigo_barras, nombre, categoria, precio_costo, precio_venta, unidad, stock, stock_minimo, controla_stock, venta_rapida) VALUES
 (NULL, 'Pan francés (kg)',       'Panes',     1500.00, 2500.00, 'KG',      20,   5, 1, 1),
 (NULL, 'Facturas (unidad)',      'Panes',      250.00,  450.00, 'UNIDAD',  48,  12, 1, 1),
 (NULL, 'Caramelos sueltos',      'Golosinas',   40.00,  100.00, 'UNIDAD', 300,  50, 1, 1),
 (NULL, 'Helado 1 kg',            'Helados',   4500.00, 9000.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Helado 1/2 kg',          'Helados',   2300.00, 5000.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Helado 1/4 kg',          'Helados',   1200.00, 2800.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Cono simple',            'Helados',    500.00, 1500.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Cono doble',             'Helados',    800.00, 2200.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Vaso chico',             'Helados',    500.00, 1500.00, 'UNIDAD',   0,   0, 0, 1),
 (NULL, 'Vaso grande',            'Helados',    900.00, 2500.00, 'UNIDAD',   0,   0, 0, 1);

INSERT INTO clientes (nombre, telefono, direccion, limite_credito) VALUES
 ('María González', '3794111222', 'Barrio Centro', 50000),
 ('Juan Pérez',     '3794333444', 'Calle 5 N° 120', 30000);
