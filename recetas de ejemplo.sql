-- ============================================================
-- AGROCONNECT - Datos de prueba para GeoInsights
-- ============================================================
-- Usa: Aplicador2 (UserId=4), Municipio UserId=5, Municipality Id=1
-- Zona: alrededores de Marcos Juárez / San Francisco, Córdoba
-- Fechas: hoy a +15 días
-- Password de los usuarios ya existentes (no se tocan)
-- ============================================================
USE [AgroConnect]
GO
SET NOCOUNT ON;

-- ============================================================
-- VARIABLES
-- ============================================================
DECLARE @AppUserId BIGINT = 4;    -- Aplicador2
DECLARE @MuniId    BIGINT = 1;    -- Muni 1
DECLARE @Now       DATETIME2(0) = SYSDATETIME();

-- ============================================================
-- 1. ASESORES (3)
-- ============================================================
SET IDENTITY_INSERT dbo.Advisors ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Advisors WHERE Id = 100)
    INSERT INTO dbo.Advisors (Id, FullName, LicenseNumber, Contact, CreatedAt, CreatedByUserId)
    VALUES (100, 'Ing. Agr. María López', 'MP-4521', 'mlopez@agro.com.ar', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Advisors WHERE Id = 101)
    INSERT INTO dbo.Advisors (Id, FullName, LicenseNumber, Contact, CreatedAt, CreatedByUserId)
    VALUES (101, 'Ing. Agr. Carlos Rodríguez', 'MP-7833', 'crodriguez@campo.com.ar', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Advisors WHERE Id = 102)
    INSERT INTO dbo.Advisors (Id, FullName, LicenseNumber, Contact, CreatedAt, CreatedByUserId)
    VALUES (102, 'Ing. Agr. Laura Fernández', 'MP-2190', 'lfernandez@agri.com.ar', @Now, @AppUserId);

SET IDENTITY_INSERT dbo.Advisors OFF;

-- ============================================================
-- 2. SOLICITANTES (3)
-- ============================================================
SET IDENTITY_INSERT dbo.Requesters ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Requesters WHERE Id = 100)
    INSERT INTO dbo.Requesters (Id, LegalName, TaxId, Address, Contact, CreatedAt, CreatedByUserId)
    VALUES (100, 'Agropecuaria Los Alamos S.A.', '30-71234567-8', 'Ruta 9 Km 342, Marcos Juárez', '03472-421100', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Requesters WHERE Id = 101)
    INSERT INTO dbo.Requesters (Id, LegalName, TaxId, Address, Contact, CreatedAt, CreatedByUserId)
    VALUES (101, 'Campo El Trébol de Gómez Hnos.', '30-65432198-5', 'Camino rural s/n, Leones', '03472-498200', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Requesters WHERE Id = 102)
    INSERT INTO dbo.Requesters (Id, LegalName, TaxId, Address, Contact, CreatedAt, CreatedByUserId)
    VALUES (102, 'Estancia Santa Rosa S.R.L.', '30-70987654-1', 'Ruta 12 Km 58, San Francisco', '03564-445500', @Now, @AppUserId);

SET IDENTITY_INSERT dbo.Requesters OFF;

-- ============================================================
-- 3. PRODUCTOS (8 con distintas clases toxicológicas)
-- ============================================================
SET IDENTITY_INSERT dbo.Products ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 100)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (100, 'SENASA-38521', 'Glifosato 66.2%', 'IV', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 101)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (101, 'SENASA-40112', '2,4-D Amina 60%', 'II', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 102)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (102, 'SENASA-35780', 'Atrazina 50%', 'III', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 103)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (103, 'SENASA-41233', 'Clorpirifós 48% EC', 'Ib', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 104)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (104, 'SENASA-39087', 'Cipermetrina 25%', 'II', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 105)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (105, 'SENASA-42501', 'Metsulfurón Metil 60%', 'IV', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 106)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (106, 'SENASA-37890', 'Endosulfán 35% EC', 'Ia', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = 107)
    INSERT INTO dbo.Products (Id, SenasaRegistry, ProductName, ToxicologicalClass, CreatedAt, CreatedByUserId)
    VALUES (107, 'SENASA-43100', 'Lambda Cihalotrina 5%', 'III', @Now, @AppUserId);

SET IDENTITY_INSERT dbo.Products OFF;

-- ============================================================
-- 4. RECETAS (12) - fechas escalonadas hoy a +15 días
-- ============================================================
-- Desactivar triggers temporalmente para insertar con IDENTITY_INSERT
-- (los triggers insertan en RecipeStatusHistory)
DISABLE TRIGGER [dbo].[TR_Recipes_StatusHistory_Insert] ON [dbo].[Recipes];
DISABLE TRIGGER [dbo].[TR_Recipes_StatusHistory] ON [dbo].[Recipes];

SET IDENTITY_INSERT dbo.Recipes ON;

-- Receta 1: Soja - Glifosato (IV) - zona norte Marcos Juárez
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1000)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1000, 90001, 'APROBADA', CAST(DATEADD(DAY, 0, GETDATE()) AS DATE), CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), CAST(DATEADD(DAY, 30, GETDATE()) AS DATE),
    100, 100, 'Terrestre', 'Soja', 'Malezas gramíneas', 'Barbecho químico', 'Pulverizadora autopropulsada', 120.50,
    18.0, 28.0, 40.0, 70.0, 5.0, 15.0, 'Sur', @Now, @AppUserId, @MuniId, @Now);

-- Receta 2: Maíz - 2,4-D (II) + Atrazina (III) - zona sur
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1001)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1001, 90002, 'APROBADA', CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), CAST(DATEADD(DAY, 31, GETDATE()) AS DATE),
    100, 101, 'Terrestre', 'Maíz', 'Malezas de hoja ancha', 'Post-emergente', 'Pulverizadora de arrastre', 85.00,
    20.0, 32.0, 35.0, 65.0, 3.0, 12.0, 'Norte', @Now, @AppUserId, @MuniId, @Now);

-- Receta 3: Trigo - Cipermetrina (II) - zona este
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1002)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1002, 90003, 'PENDIENTE', CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), CAST(DATEADD(DAY, 3, GETDATE()) AS DATE), CAST(DATEADD(DAY, 32, GETDATE()) AS DATE),
    101, 100, 'Terrestre', 'Trigo', 'Isoca bolillera', 'Control de lepidópteros', 'Pulverizadora autopropulsada', 200.00,
    15.0, 25.0, 45.0, 75.0, 4.0, 10.0, 'Oeste', @Now, @AppUserId, @MuniId, @Now);

-- Receta 4: Soja - Clorpirifós (Ib) ⚠️ alta toxicidad - zona cercana a escuela
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1003)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1003, 90004, 'APROBADA', CAST(DATEADD(DAY, 3, GETDATE()) AS DATE), CAST(DATEADD(DAY, 4, GETDATE()) AS DATE), CAST(DATEADD(DAY, 33, GETDATE()) AS DATE),
    102, 102, 'Terrestre', 'Soja', 'Chinche verde', 'Control de hemípteros', 'Pulverizadora autopropulsada', 50.00,
    22.0, 30.0, 30.0, 60.0, 2.0, 8.0, 'Este', @Now, @AppUserId, @MuniId, @Now);

-- Receta 5: Girasol - Glifosato (IV) - zona oeste
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1004)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1004, 90005, 'ABIERTA', CAST(DATEADD(DAY, 4, GETDATE()) AS DATE), CAST(DATEADD(DAY, 5, GETDATE()) AS DATE), CAST(DATEADD(DAY, 34, GETDATE()) AS DATE),
    100, 100, 'Terrestre', 'Girasol', 'Malezas anuales', 'Pre-siembra', 'Pulverizadora de arrastre', 95.00,
    16.0, 26.0, 50.0, 80.0, 6.0, 18.0, 'Sur', @Now, @AppUserId, @MuniId, @Now);

-- Receta 6: Maíz - Endosulfán (Ia) ⚠️ EXTREMA toxicidad
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1005)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1005, 90006, 'APROBADA', CAST(DATEADD(DAY, 5, GETDATE()) AS DATE), CAST(DATEADD(DAY, 6, GETDATE()) AS DATE), CAST(DATEADD(DAY, 35, GETDATE()) AS DATE),
    101, 102, 'Terrestre', 'Maíz', 'Oruga cogollera', 'Control de lepidópteros', 'Pulverizadora autopropulsada', 70.00,
    19.0, 29.0, 38.0, 68.0, 3.0, 10.0, 'Norte', @Now, @AppUserId, @MuniId, @Now);

-- Receta 7: Soja - Lambda Cihalotrina (III) - grande
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1006)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1006, 90007, 'PENDIENTE', CAST(DATEADD(DAY, 7, GETDATE()) AS DATE), CAST(DATEADD(DAY, 8, GETDATE()) AS DATE), CAST(DATEADD(DAY, 37, GETDATE()) AS DATE),
    102, 101, 'Terrestre', 'Soja', 'Trips', 'Control de tisanópteros', 'Pulverizadora autopropulsada', 300.00,
    17.0, 27.0, 42.0, 72.0, 5.0, 14.0, 'Este', @Now, @AppUserId, @MuniId, @Now);

-- Receta 8: Trigo - Metsulfurón (IV) - lote chico
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1007)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1007, 90008, 'APROBADA', CAST(DATEADD(DAY, 8, GETDATE()) AS DATE), CAST(DATEADD(DAY, 9, GETDATE()) AS DATE), CAST(DATEADD(DAY, 38, GETDATE()) AS DATE),
    100, 100, 'Terrestre', 'Trigo', 'Malezas de hoja ancha', 'Post-emergente temprano', 'Pulverizadora de arrastre', 45.00,
    14.0, 22.0, 55.0, 85.0, 4.0, 12.0, 'Oeste', @Now, @AppUserId, @MuniId, @Now);

-- Receta 9: Soja - Glifosato (IV) + Cipermetrina (II) - doble producto
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1008)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1008, 90009, 'ABIERTA', CAST(DATEADD(DAY, 10, GETDATE()) AS DATE), CAST(DATEADD(DAY, 11, GETDATE()) AS DATE), CAST(DATEADD(DAY, 40, GETDATE()) AS DATE),
    101, 101, 'Terrestre', 'Soja', 'Malezas + chinches', 'Barbecho + insecticida', 'Pulverizadora autopropulsada', 150.00,
    21.0, 31.0, 33.0, 63.0, 2.0, 9.0, 'Sur', @Now, @AppUserId, @MuniId, @Now);

-- Receta 10: Maíz - Atrazina (III) - zona noroeste
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1009)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1009, 90010, 'APROBADA', CAST(DATEADD(DAY, 11, GETDATE()) AS DATE), CAST(DATEADD(DAY, 12, GETDATE()) AS DATE), CAST(DATEADD(DAY, 41, GETDATE()) AS DATE),
    102, 100, 'Terrestre', 'Maíz', 'Malezas gramíneas y hoja ancha', 'Pre-emergente', 'Pulverizadora de arrastre', 110.00,
    18.0, 28.0, 40.0, 70.0, 5.0, 15.0, 'Norte', @Now, @AppUserId, @MuniId, @Now);

-- Receta 11: Soja - Clorpirifós (Ib) - otro lote con alta toxicidad
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1010)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1010, 90011, 'PENDIENTE', CAST(DATEADD(DAY, 13, GETDATE()) AS DATE), CAST(DATEADD(DAY, 14, GETDATE()) AS DATE), CAST(DATEADD(DAY, 43, GETDATE()) AS DATE),
    100, 102, 'Terrestre', 'Soja', 'Arañuela', 'Control de ácaros', 'Pulverizadora autopropulsada', 80.00,
    23.0, 33.0, 28.0, 58.0, 3.0, 11.0, 'Este', @Now, @AppUserId, @MuniId, @Now);

-- Receta 12: Girasol - Glifosato (IV) + Lambda (III)
IF NOT EXISTS (SELECT 1 FROM dbo.Recipes WHERE Id = 1011)
INSERT INTO dbo.Recipes (Id, RfdNumber, [Status], IssueDate, PossibleStartDate, ExpirationDate, RequesterId, AdvisorId,
    ApplicationType, Crop, Diagnosis, Treatment, MachineToUse, UnitSurfaceHa, TempMin, TempMax, HumidityMin, HumidityMax,
    WindMinKmh, WindMaxKmh, WindDirection, CreatedAt, CreatedByUserId, AssignedMunicipalityId, AssignedAt)
VALUES (1011, 90012, 'APROBADA', CAST(DATEADD(DAY, 15, GETDATE()) AS DATE), CAST(DATEADD(DAY, 16, GETDATE()) AS DATE), CAST(DATEADD(DAY, 45, GETDATE()) AS DATE),
    102, 101, 'Terrestre', 'Girasol', 'Malezas + polilla', 'Barbecho + insecticida', 'Pulverizadora de arrastre', 175.00,
    16.0, 26.0, 45.0, 75.0, 4.0, 13.0, 'Oeste', @Now, @AppUserId, @MuniId, @Now);

SET IDENTITY_INSERT dbo.Recipes OFF;

-- Reactivar triggers
ENABLE TRIGGER [dbo].[TR_Recipes_StatusHistory_Insert] ON [dbo].[Recipes];
ENABLE TRIGGER [dbo].[TR_Recipes_StatusHistory] ON [dbo].[Recipes];

-- Insertar history manualmente para las recetas de seed
INSERT INTO dbo.RecipeStatusHistory (RecipeId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Source, Notes)
SELECT Id, NULL, [Status], @Now, @AppUserId, 'SEED', 'Datos de prueba'
FROM dbo.Recipes WHERE Id BETWEEN 1000 AND 1011
AND NOT EXISTS (SELECT 1 FROM dbo.RecipeStatusHistory h WHERE h.RecipeId = dbo.Recipes.Id AND h.Source = 'SEED');

-- ============================================================
-- 5. RECIPE PRODUCTS
-- ============================================================
-- R1000: Glifosato (IV)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1000 AND ProductId = 100)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1000, 100, 'Glifosato 66.2%', 'SENASA-38521', 'IV', 'Herbicida', 3.000000, 'l', 'ha', 361.500000, 'l', @Now, @AppUserId);

-- R1001: 2,4-D (II) + Atrazina (III)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1001 AND ProductId = 101)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1001, 101, '2,4-D Amina 60%', 'SENASA-40112', 'II', 'Herbicida', 0.800000, 'l', 'ha', 68.000000, 'l', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1001 AND ProductId = 102)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1001, 102, 'Atrazina 50%', 'SENASA-35780', 'III', 'Herbicida', 2.000000, 'l', 'ha', 170.000000, 'l', @Now, @AppUserId);

-- R1002: Cipermetrina (II)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1002 AND ProductId = 104)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1002, 104, 'Cipermetrina 25%', 'SENASA-39087', 'II', 'Insecticida', 0.200000, 'l', 'ha', 40.000000, 'l', @Now, @AppUserId);

-- R1003: Clorpirifós (Ib) ⚠️
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1003 AND ProductId = 103)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1003, 103, 'Clorpirifós 48% EC', 'SENASA-41233', 'Ib', 'Insecticida', 1.000000, 'l', 'ha', 50.000000, 'l', @Now, @AppUserId);

-- R1004: Glifosato (IV)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1004 AND ProductId = 100)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1004, 100, 'Glifosato 66.2%', 'SENASA-38521', 'IV', 'Herbicida', 3.000000, 'l', 'ha', 285.000000, 'l', @Now, @AppUserId);

-- R1005: Endosulfán (Ia) ⚠️ EXTREMA
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1005 AND ProductId = 106)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1005, 106, 'Endosulfán 35% EC', 'SENASA-37890', 'Ia', 'Insecticida', 1.500000, 'l', 'ha', 105.000000, 'l', @Now, @AppUserId);

-- R1006: Lambda Cihalotrina (III)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1006 AND ProductId = 107)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1006, 107, 'Lambda Cihalotrina 5%', 'SENASA-43100', 'III', 'Insecticida', 0.150000, 'l', 'ha', 45.000000, 'l', @Now, @AppUserId);

-- R1007: Metsulfurón (IV)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1007 AND ProductId = 105)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1007, 105, 'Metsulfurón Metil 60%', 'SENASA-42501', 'IV', 'Herbicida', 0.007000, 'kg', 'ha', 0.315000, 'kg', @Now, @AppUserId);

-- R1008: Glifosato (IV) + Cipermetrina (II)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1008 AND ProductId = 100)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1008, 100, 'Glifosato 66.2%', 'SENASA-38521', 'IV', 'Herbicida', 3.000000, 'l', 'ha', 450.000000, 'l', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1008 AND ProductId = 104)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1008, 104, 'Cipermetrina 25%', 'SENASA-39087', 'II', 'Insecticida', 0.200000, 'l', 'ha', 30.000000, 'l', @Now, @AppUserId);

-- R1009: Atrazina (III)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1009 AND ProductId = 102)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1009, 102, 'Atrazina 50%', 'SENASA-35780', 'III', 'Herbicida', 2.500000, 'l', 'ha', 275.000000, 'l', @Now, @AppUserId);

-- R1010: Clorpirifós (Ib) ⚠️
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1010 AND ProductId = 103)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1010, 103, 'Clorpirifós 48% EC', 'SENASA-41233', 'Ib', 'Insecticida', 1.200000, 'l', 'ha', 96.000000, 'l', @Now, @AppUserId);

-- R1011: Glifosato (IV) + Lambda (III)
IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1011 AND ProductId = 100)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1011, 100, 'Glifosato 66.2%', 'SENASA-38521', 'IV', 'Herbicida', 3.000000, 'l', 'ha', 525.000000, 'l', @Now, @AppUserId);

IF NOT EXISTS (SELECT 1 FROM dbo.RecipeProducts WHERE RecipeId = 1011 AND ProductId = 107)
INSERT INTO dbo.RecipeProducts (RecipeId, ProductId, ProductName, SenasaRegistry, ToxicologicalClass, ProductType, DoseValue, DoseUnit, DosePerUnit, TotalValue, TotalUnit, CreatedAt, CreatedByUserId)
VALUES (1011, 107, 'Lambda Cihalotrina 5%', 'SENASA-43100', 'III', 'Insecticida', 0.150000, 'l', 'ha', 26.250000, 'l', @Now, @AppUserId);

-- ============================================================
-- 6. LOTES con polígonos GPS reales (zona Marcos Juárez/San Francisco)
-- ============================================================
-- Coordenadas reales de lotes agrícolas en la zona rural

-- Lote R1000: Norte de Marcos Juárez (-32.68, -62.10)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1000, 'Lote 12 - La Esperanza', 'Marcos Juárez', 'Marcos Juárez', 120.50, @Now, @AppUserId);
DECLARE @Lot1 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot1, 1, -32.6750000, -62.1100000, @Now),
(@Lot1, 2, -32.6750000, -62.0950000, @Now),
(@Lot1, 3, -32.6870000, -62.0950000, @Now),
(@Lot1, 4, -32.6870000, -62.1100000, @Now);

-- Lote R1001: Sur de Marcos Juárez (-32.72, -62.08)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1001, 'Lote 5 - El Progreso', 'Marcos Juárez', 'Marcos Juárez', 85.00, @Now, @AppUserId);
DECLARE @Lot2 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot2, 1, -32.7200000, -62.0900000, @Now),
(@Lot2, 2, -32.7200000, -62.0750000, @Now),
(@Lot2, 3, -32.7300000, -62.0750000, @Now),
(@Lot2, 4, -32.7300000, -62.0900000, @Now);

-- Lote R1002: Este, camino a Leones (-32.66, -62.05)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1002, 'Lote 8 - San Martín', 'Leones', 'Marcos Juárez', 200.00, @Now, @AppUserId);
DECLARE @Lot3 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot3, 1, -32.6550000, -62.0600000, @Now),
(@Lot3, 2, -32.6550000, -62.0350000, @Now),
(@Lot3, 3, -32.6720000, -62.0350000, @Now),
(@Lot3, 4, -32.6720000, -62.0600000, @Now);

-- Lote R1003: Cerca del pueblo (zona periurbana) - ⚠️ Clorpirifós
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1003, 'Lote 2 - La Rinconada', 'Marcos Juárez', 'Marcos Juárez', 50.00, @Now, @AppUserId);
DECLARE @Lot4 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot4, 1, -32.6920000, -62.1050000, @Now),
(@Lot4, 2, -32.6920000, -62.0980000, @Now),
(@Lot4, 3, -32.6970000, -62.0980000, @Now),
(@Lot4, 4, -32.6970000, -62.1050000, @Now);

-- Lote R1004: Zona oeste (-32.70, -62.15)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1004, 'Lote 20 - Las Acacias', 'Marcos Juárez', 'Marcos Juárez', 95.00, @Now, @AppUserId);
DECLARE @Lot5 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot5, 1, -32.6950000, -62.1600000, @Now),
(@Lot5, 2, -32.6950000, -62.1450000, @Now),
(@Lot5, 3, -32.7060000, -62.1450000, @Now),
(@Lot5, 4, -32.7060000, -62.1600000, @Now);

-- Lote R1005: Sur-este, Endosulfán ⚠️ (-32.74, -62.06)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1005, 'Lote 15 - Campo Grande', 'Leones', 'Marcos Juárez', 70.00, @Now, @AppUserId);
DECLARE @Lot6 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot6, 1, -32.7350000, -62.0700000, @Now),
(@Lot6, 2, -32.7350000, -62.0550000, @Now),
(@Lot6, 3, -32.7450000, -62.0550000, @Now),
(@Lot6, 4, -32.7450000, -62.0700000, @Now);

-- Lote R1006: Lote grande al noreste (-32.64, -62.04)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1006, 'Lote 30 - Estancia del Sol', 'Leones', 'Marcos Juárez', 300.00, @Now, @AppUserId);
DECLARE @Lot7 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot7, 1, -32.6300000, -62.0550000, @Now),
(@Lot7, 2, -32.6300000, -62.0200000, @Now),
(@Lot7, 3, -32.6500000, -62.0200000, @Now),
(@Lot7, 4, -32.6500000, -62.0550000, @Now);

-- Lote R1007: Lote chico al sur-oeste (-32.71, -62.13)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1007, 'Lote 3 - El Molino', 'Marcos Juárez', 'Marcos Juárez', 45.00, @Now, @AppUserId);
DECLARE @Lot8 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot8, 1, -32.7100000, -62.1350000, @Now),
(@Lot8, 2, -32.7100000, -62.1260000, @Now),
(@Lot8, 3, -32.7160000, -62.1260000, @Now),
(@Lot8, 4, -32.7160000, -62.1350000, @Now);

-- Lote R1008: Centro-sur (-32.71, -62.09)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1008, 'Lote 18 - Santa María', 'Marcos Juárez', 'Marcos Juárez', 150.00, @Now, @AppUserId);
DECLARE @Lot9 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot9, 1, -32.7050000, -62.1000000, @Now),
(@Lot9, 2, -32.7050000, -62.0800000, @Now),
(@Lot9, 3, -32.7180000, -62.0800000, @Now),
(@Lot9, 4, -32.7180000, -62.1000000, @Now);

-- Lote R1009: Noroeste (-32.66, -62.14)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1009, 'Lote 22 - Los Paraísos', 'Marcos Juárez', 'Marcos Juárez', 110.00, @Now, @AppUserId);
DECLARE @Lot10 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot10, 1, -32.6550000, -62.1500000, @Now),
(@Lot10, 2, -32.6550000, -62.1350000, @Now),
(@Lot10, 3, -32.6670000, -62.1350000, @Now),
(@Lot10, 4, -32.6670000, -62.1500000, @Now);

-- Lote R1010: Este, cerca de ruta (-32.69, -62.07)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1010, 'Lote 9 - Don Julio', 'Marcos Juárez', 'Marcos Juárez', 80.00, @Now, @AppUserId);
DECLARE @Lot11 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot11, 1, -32.6850000, -62.0800000, @Now),
(@Lot11, 2, -32.6850000, -62.0680000, @Now),
(@Lot11, 3, -32.6950000, -62.0680000, @Now),
(@Lot11, 4, -32.6950000, -62.0800000, @Now);

-- Lote R1011: Suroeste (-32.73, -62.14)
INSERT INTO dbo.RecipeLots (RecipeId, LotName, Locality, Department, SurfaceHa, CreatedAt, CreatedByUserId)
VALUES (1011, 'Lote 25 - La Cautiva', 'Marcos Juárez', 'Marcos Juárez', 175.00, @Now, @AppUserId);
DECLARE @Lot12 BIGINT = SCOPE_IDENTITY();

INSERT INTO dbo.RecipeLotVertices (LotId, [Order], Latitude, Longitude, CreatedAt) VALUES
(@Lot12, 1, -32.7250000, -62.1500000, @Now),
(@Lot12, 2, -32.7250000, -62.1300000, @Now),
(@Lot12, 3, -32.7400000, -62.1300000, @Now),
(@Lot12, 4, -32.7400000, -62.1500000, @Now);

-- ============================================================
-- 7. PUNTOS SENSIBLES (en varias recetas)
-- ============================================================

-- Escuela Rural Nº 142 (cerca de Lote R1003 con Clorpirifós)
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1003, 'Escuela Rural Nº 142', 'Escuela', 'Marcos Juárez', 'Marcos Juárez', -32.6940000, -62.1020000, @Now, @AppUserId);

-- Hospital Municipal
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1003, 'Hospital Municipal Marcos Juárez', 'Hospital', 'Marcos Juárez', 'Marcos Juárez', -32.6980000, -62.1060000, @Now, @AppUserId);

-- Arroyo (curso de agua) cerca de lotes sur
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1001, 'Arroyo Tortugas', 'Curso de agua', 'Marcos Juárez', 'Marcos Juárez', -32.7250000, -62.0820000, @Now, @AppUserId);

INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1005, 'Arroyo Tortugas', 'Curso de agua', 'Leones', 'Marcos Juárez', -32.7250000, -62.0820000, @Now, @AppUserId);

-- Escuela Rural Nº 78 (zona noreste)
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1006, 'Escuela Rural Nº 78', 'Escuela', 'Leones', 'Marcos Juárez', -32.6420000, -62.0400000, @Now, @AppUserId);

-- Reserva natural
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1009, 'Reserva Natural Laguna del Monte', 'Área protegida', 'Marcos Juárez', 'Marcos Juárez', -32.6600000, -62.1420000, @Now, @AppUserId);

-- Club deportivo rural (zona periurbana)
INSERT INTO dbo.RecipeSensitivePoints (RecipeId, Name, Type, Locality, Department, Latitude, Longitude, CreatedAt, CreatedByUserId)
VALUES (1008, 'Club Atlético Rural', 'Centro deportivo', 'Marcos Juárez', 'Marcos Juárez', -32.7100000, -62.0900000, @Now, @AppUserId);

-- ============================================================
-- VERIFICACIÓN
-- ============================================================
PRINT '✅ Datos insertados correctamente';
PRINT '';

SELECT 'Recetas' AS Tabla, COUNT(*) AS Cant FROM dbo.Recipes WHERE Id BETWEEN 1000 AND 1011
UNION ALL
SELECT 'Productos por receta', COUNT(*) FROM dbo.RecipeProducts WHERE RecipeId BETWEEN 1000 AND 1011
UNION ALL
SELECT 'Lotes', COUNT(*) FROM dbo.RecipeLots WHERE RecipeId BETWEEN 1000 AND 1011
UNION ALL
SELECT 'Vértices GPS', COUNT(*) FROM dbo.RecipeLotVertices v INNER JOIN dbo.RecipeLots l ON v.LotId = l.Id WHERE l.RecipeId BETWEEN 1000 AND 1011
UNION ALL
SELECT 'Puntos sensibles', COUNT(*) FROM dbo.RecipeSensitivePoints WHERE RecipeId BETWEEN 1000 AND 1011;

PRINT '';
PRINT '📋 Recetas creadas:';
SELECT Id, RfdNumber, [Status], Crop, IssueDate, AssignedMunicipalityId
FROM dbo.Recipes WHERE Id BETWEEN 1000 AND 1011 ORDER BY Id;

PRINT '';
PRINT '🗺️ Para testear:';
PRINT '   - Login como Municipio (MUNI@MAIL.COM)';
PRINT '   - Ir a "Mapa Territorial"';
PRINT '   - Deberías ver 12 polígonos coloreados por toxicidad';
PRINT '   - 7 puntos sensibles (escuelas, hospital, arroyo, reserva, club)';
PRINT '   - Mapa de calor con concentración en zona Marcos Juárez';
GO




  UPDATE dbo.Recipes
SET RecommendedDate = '2026-02-23';