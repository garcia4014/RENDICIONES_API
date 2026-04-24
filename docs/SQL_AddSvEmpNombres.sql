-- ==============================================================================
-- SQL_AddSvEmpNombres.sql
-- Agrega la columna SV_EMP_NOMBRES a la tabla SVIATICOS_CABECERA
-- Ejecutar UNA SOLA VEZ en la BD de producción/desarrollo
-- ==============================================================================

USE [SVRENDICIONES]; -- cambiar si el nombre de la BD es diferente
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.SVIATICOS_CABECERA')
      AND name = 'SV_EMP_NOMBRES'
)
BEGIN
    ALTER TABLE dbo.SVIATICOS_CABECERA
    ADD SV_EMP_NOMBRES NVARCHAR(200) NULL;

    PRINT '[OK] Columna SV_EMP_NOMBRES agregada a SVIATICOS_CABECERA.';
END
ELSE
    PRINT '[--] Columna SV_EMP_NOMBRES ya existe.';
GO
