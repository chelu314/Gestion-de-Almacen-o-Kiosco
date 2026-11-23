-- =============================================================
--  Almacén La Tutti - Script de creación de la base de datos
--  MySQL 8 / MariaDB 10.4+ (XAMPP)
--  Ejecutar completo en MySQL Workbench, phpMyAdmin o consola.
-- =============================================================

CREATE DATABASE IF NOT EXISTS kiosco_la_familia
  CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;

USE kiosco_la_familia;

-- ---------- Usuarios del sistema ----------
CREATE TABLE IF NOT EXISTS usuarios (
  id        INT AUTO_INCREMENT PRIMARY KEY,
  usuario   VARCHAR(50)  NOT NULL UNIQUE,
  nombre    VARCHAR(100) NOT NULL,
  hash      VARCHAR(100) NOT NULL,
  salt      VARCHAR(50)  NOT NULL,
  rol       ENUM('admin','cajero') NOT NULL DEFAULT 'cajero',
  activo    TINYINT(1)   NOT NULL DEFAULT 1,
  creado    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);
-- El usuario inicial (admin / admin123) lo crea el programa la primera vez que se abre.

-- ---------- Productos ----------
CREATE TABLE IF NOT EXISTS productos (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  codigo_barras  VARCHAR(50)   NULL UNIQUE,
  nombre         VARCHAR(150)  NOT NULL,
  categoria      VARCHAR(60)   NOT NULL DEFAULT 'General',
  precio_costo   DECIMAL(12,2) NOT NULL DEFAULT 0,
  precio_venta   DECIMAL(12,2) NOT NULL DEFAULT 0,
  unidad         ENUM('UNIDAD','KG') NOT NULL DEFAULT 'UNIDAD',   -- KG = se vende por peso
  stock          DECIMAL(12,3) NOT NULL DEFAULT 0,
  stock_minimo   DECIMAL(12,3) NOT NULL DEFAULT 5,
  controla_stock TINYINT(1)    NOT NULL DEFAULT 1,   -- 0 = no descuenta stock (ej. presentaciones de helado)
  venta_rapida   TINYINT(1)    NOT NULL DEFAULT 0,   -- 1 = aparece como botón en la pantalla de venta
  activo         TINYINT(1)    NOT NULL DEFAULT 1,
  creado         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado    DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX ix_productos_nombre (nombre),
  INDEX ix_productos_categoria (categoria)
);

-- ---------- Clientes (cuenta corriente) ----------
CREATE TABLE IF NOT EXISTS clientes (
  id              INT AUTO_INCREMENT PRIMARY KEY,
  nombre          VARCHAR(120)  NOT NULL,
  telefono        VARCHAR(30)   NULL,
  direccion       VARCHAR(150)  NULL,
  limite_credito  DECIMAL(12,2) NOT NULL DEFAULT 0,   -- 0 = sin límite
  observaciones   VARCHAR(255)  NULL,
  activo          TINYINT(1)    NOT NULL DEFAULT 1,
  creado          DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  INDEX ix_clientes_nombre (nombre)
);

-- ---------- Ventas ----------
CREATE TABLE IF NOT EXISTS ventas (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  fecha        DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  usuario_id   INT           NOT NULL,
  cliente_id   INT           NULL,
  total        DECIMAL(12,2) NOT NULL,
  costo_total  DECIMAL(12,2) NOT NULL DEFAULT 0,
  forma_pago   ENUM('EFECTIVO','TRANSFERENCIA','DEBITO','CREDITO','CUENTA_CORRIENTE') NOT NULL,
  pagado_con   DECIMAL(12,2) NULL,
  vuelto       DECIMAL(12,2) NULL,
  estado       ENUM('ACTIVA','ANULADA') NOT NULL DEFAULT 'ACTIVA',
  INDEX ix_ventas_fecha (fecha),
  INDEX ix_ventas_cliente (cliente_id),
  CONSTRAINT fk_ventas_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
  CONSTRAINT fk_ventas_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id)
);

CREATE TABLE IF NOT EXISTS venta_detalle (
  id               INT AUTO_INCREMENT PRIMARY KEY,
  venta_id         INT           NOT NULL,
  producto_id      INT           NOT NULL,
  descripcion      VARCHAR(150)  NOT NULL,
  cantidad         DECIMAL(12,3) NOT NULL,
  unidad           VARCHAR(10)   NOT NULL DEFAULT 'UNIDAD',
  precio_unitario  DECIMAL(12,2) NOT NULL,
  costo_unitario   DECIMAL(12,2) NOT NULL DEFAULT 0,
  subtotal         DECIMAL(12,2) NOT NULL,
  CONSTRAINT fk_detalle_venta    FOREIGN KEY (venta_id)    REFERENCES ventas(id) ON DELETE CASCADE,
  CONSTRAINT fk_detalle_producto FOREIGN KEY (producto_id) REFERENCES productos(id)
);

-- ---------- Pagos de clientes ----------
CREATE TABLE IF NOT EXISTS pagos_clientes (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  cliente_id   INT           NOT NULL,
  fecha        DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  monto        DECIMAL(12,2) NOT NULL,
  forma_pago   VARCHAR(30)   NOT NULL DEFAULT 'EFECTIVO',
  usuario_id   INT           NOT NULL,
  observacion  VARCHAR(255)  NULL,
  INDEX ix_pagos_cliente (cliente_id),
  INDEX ix_pagos_fecha (fecha),
  CONSTRAINT fk_pagos_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
  CONSTRAINT fk_pagos_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- ---------- Movimientos de stock ----------
CREATE TABLE IF NOT EXISTS movimientos_stock (
  id                INT AUTO_INCREMENT PRIMARY KEY,
  producto_id       INT          NOT NULL,
  fecha             DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  tipo              ENUM('INGRESO','VENTA','AJUSTE','BAJA','ANULACION') NOT NULL,
  cantidad          DECIMAL(12,3) NOT NULL,         -- positivo suma, negativo resta
  stock_resultante  DECIMAL(12,3) NOT NULL,
  usuario_id        INT          NOT NULL,
  observacion       VARCHAR(255) NULL,
  INDEX ix_mov_fecha (fecha),
  CONSTRAINT fk_mov_producto FOREIGN KEY (producto_id) REFERENCES productos(id),
  CONSTRAINT fk_mov_usuario  FOREIGN KEY (usuario_id)  REFERENCES usuarios(id)
);

-- ---------- Historial de precios ----------
CREATE TABLE IF NOT EXISTS historial_precios (
  id               INT AUTO_INCREMENT PRIMARY KEY,
  producto_id      INT           NOT NULL,
  fecha            DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
  costo_anterior   DECIMAL(12,2) NOT NULL,
  precio_anterior  DECIMAL(12,2) NOT NULL,
  costo_nuevo      DECIMAL(12,2) NOT NULL,
  precio_nuevo     DECIMAL(12,2) NOT NULL,
  usuario_id       INT           NOT NULL,
  CONSTRAINT fk_hist_producto FOREIGN KEY (producto_id) REFERENCES productos(id)
);

-- ---------- Configuración ----------
CREATE TABLE IF NOT EXISTS configuracion (
  clave  VARCHAR(50)  PRIMARY KEY,
  valor  VARCHAR(255) NOT NULL
);

INSERT IGNORE INTO configuracion (clave, valor) VALUES
 ('nombre_negocio',      'Almacén La Tutti'),
 ('direccion',           ''),
 ('telefono',            ''),
 ('codigo_pais',         '549'),
 ('ancho_ticket',        '32'),
 ('pie_ticket',          '¡Gracias por su compra!'),
 ('alerta_stock_inicio', '1'),
 ('mostrar_ticket',      '0'),
 ('registro_requiere_aprobacion', '1');
