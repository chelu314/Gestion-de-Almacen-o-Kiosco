-- =============================================================
--  Actualización v2: venta por kilo, botones de venta rápida
--  Ejecutar SOLO si ya habías creado la base con la versión anterior.
--  (Si la creás de cero con 01_crear_base.sql no hace falta.)
-- =============================================================
USE kiosco_la_familia;

ALTER TABLE productos
  ADD COLUMN unidad ENUM('UNIDAD','KG') NOT NULL DEFAULT 'UNIDAD' AFTER precio_venta,
  MODIFY stock DECIMAL(12,3) NOT NULL DEFAULT 0,
  MODIFY stock_minimo DECIMAL(12,3) NOT NULL DEFAULT 5,
  ADD COLUMN controla_stock TINYINT(1) NOT NULL DEFAULT 1 AFTER stock_minimo,
  ADD COLUMN venta_rapida TINYINT(1) NOT NULL DEFAULT 0 AFTER controla_stock;

ALTER TABLE venta_detalle
  MODIFY cantidad DECIMAL(12,3) NOT NULL,
  ADD COLUMN unidad VARCHAR(10) NOT NULL DEFAULT 'UNIDAD' AFTER cantidad;

ALTER TABLE movimientos_stock
  MODIFY cantidad DECIMAL(12,3) NOT NULL,
  MODIFY stock_resultante DECIMAL(12,3) NOT NULL;
