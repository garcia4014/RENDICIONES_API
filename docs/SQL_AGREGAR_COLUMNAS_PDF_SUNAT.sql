-- =============================================
-- Script para agregar columnas de descarga de PDF desde SUNAT
-- Fecha: 2026-02-04
-- Descripción: Agrega las columnas PDF_SUNAT y REINTENTOS_PDF_SUNAT 
--              a la tabla COMPROBANTE_PAGO
-- =============================================

USE SVRENDICIONES;
GO

-- Verificar si la columna PDF_SUNAT ya existe
IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.COMPROBANTE_PAGO') 
               AND name = 'PDF_SUNAT')
BEGIN
    ALTER TABLE dbo.COMPROBANTE_PAGO
    ADD PDF_SUNAT BIT NULL DEFAULT 0;
    
    PRINT 'Columna PDF_SUNAT agregada exitosamente';
END
ELSE
BEGIN
    PRINT 'La columna PDF_SUNAT ya existe';
END
GO

-- Verificar si la columna REINTENTOS_PDF_SUNAT ya existe
IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.COMPROBANTE_PAGO') 
               AND name = 'REINTENTOS_PDF_SUNAT')
BEGIN
    ALTER TABLE dbo.COMPROBANTE_PAGO
    ADD REINTENTOS_PDF_SUNAT INT NULL DEFAULT 0;
    
    PRINT 'Columna REINTENTOS_PDF_SUNAT agregada exitosamente';
END
ELSE
BEGIN
    PRINT 'La columna REINTENTOS_PDF_SUNAT ya existe';
END
GO

-- Actualizar registros existentes para que tengan valores por defecto
UPDATE dbo.COMPROBANTE_PAGO
SET PDF_SUNAT = 0
WHERE PDF_SUNAT IS NULL;

UPDATE dbo.COMPROBANTE_PAGO
SET REINTENTOS_PDF_SUNAT = 0
WHERE REINTENTOS_PDF_SUNAT IS NULL;

PRINT 'Valores por defecto actualizados en registros existentes';
GO

-- Verificar las columnas agregadas
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'COMPROBANTE_PAGO'
  AND COLUMN_NAME IN ('PDF_SUNAT', 'REINTENTOS_PDF_SUNAT');
GO

PRINT '==============================================';
PRINT 'Script ejecutado exitosamente';
PRINT 'Columnas PDF_SUNAT y REINTENTOS_PDF_SUNAT listas para usar';
PRINT '==============================================';
GO
