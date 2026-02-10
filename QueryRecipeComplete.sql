DECLARE @RfdNumber BIGINT = 358532;

-------------------------------------------------------
-- 1️⃣ RECETA (HEADER)
-------------------------------------------------------
SELECT 
    'Recipe' AS Section,
    r.Id,
    r.RfdNumber,
    r.Status,
    r.IssueDate,
    r.PossibleStartDate,
    r.RecommendedDate,
    r.ExpirationDate,
    r.RequesterId,
    r.AdvisorId,
    r.ApplicationType,
    r.Crop,
    r.Diagnosis,
    r.Treatment,
    r.MachineToUse,   
    r.UnitSurfaceHa,
    r.TempMin,
    r.TempMax,
    r.HumidityMin,
    r.HumidityMax,
    r.WindMinKmh,
    r.WindMaxKmh,
    r.WindDirection,
    r.Notes,
    r.CreatedAt,
    r.DeletedAt,
	 r.MachinePlate,
    r.MachineLegalName,
    r.MachineType
FROM Recipes r
WHERE r.RfdNumber = @RfdNumber;


-------------------------------------------------------
-- 2️⃣ COUNTS GENERALES
-------------------------------------------------------
SELECT
    r.Id AS RecipeId,
    r.RfdNumber,
    (SELECT COUNT(*) FROM RecipeProducts rp WHERE rp.RecipeId = r.Id AND rp.DeletedAt IS NULL) AS ProductsCount,
    (SELECT COUNT(*) FROM RecipeLots rl WHERE rl.RecipeId = r.Id AND rl.DeletedAt IS NULL) AS LotsCount,
    (SELECT COUNT(*) FROM RecipeSensitivePoints sp WHERE sp.RecipeId = r.Id AND sp.DeletedAt IS NULL) AS SensitivePointsCount,
    (SELECT COUNT(*) 
     FROM RecipeLotVertices v 
     JOIN RecipeLots rl ON rl.Id = v.LotId 
     WHERE rl.RecipeId = r.Id AND v.DeletedAt IS NULL) AS VerticesCount
FROM Recipes r
WHERE r.RfdNumber = @RfdNumber;


-------------------------------------------------------
-- 3️⃣ REQUESTER
-------------------------------------------------------
SELECT 
    'Requester' AS Section,
    req.*
FROM Requesters req
JOIN Recipes r ON r.RequesterId = req.Id
WHERE r.RfdNumber = @RfdNumber;


-------------------------------------------------------
-- 4️⃣ ADVISOR
-------------------------------------------------------
SELECT 
    'Advisor' AS Section,
    adv.*
FROM Advisors adv
JOIN Recipes r ON r.AdvisorId = adv.Id
WHERE r.RfdNumber = @RfdNumber;


-------------------------------------------------------
-- 5️⃣ PRODUCTS (RecipeProducts + Product)
-------------------------------------------------------
SELECT 
    'RecipeProducts' AS Section,
    rp.Id,
    rp.RecipeId,
    rp.ProductId,
    rp.ProductType,
    rp.ProductName,
    rp.SenasaRegistry,
    rp.ToxicologicalClass,
    rp.DoseValue,
    rp.DoseUnit,
    rp.DosePerUnit,
    rp.TotalValue,
    rp.TotalUnit,
    rp.CreatedAt
FROM RecipeProducts rp
JOIN Recipes r ON r.Id = rp.RecipeId
WHERE r.RfdNumber = @RfdNumber
  AND rp.DeletedAt IS NULL;


-------------------------------------------------------
-- 6️⃣ LOTES
-------------------------------------------------------
SELECT 
    'RecipeLots' AS Section,
    rl.Id,
    rl.RecipeId,
    rl.LotName,
    rl.Locality,
    rl.Department,
    rl.SurfaceHa,
    rl.CreatedAt
FROM RecipeLots rl
JOIN Recipes r ON r.Id = rl.RecipeId
WHERE r.RfdNumber = @RfdNumber
  AND rl.DeletedAt IS NULL;


-------------------------------------------------------
-- 7️⃣ VÉRTICES (ORDEN + COORDENADAS)
-------------------------------------------------------
SELECT 
    'RecipeLotVertices' AS Section,
    rl.Id AS LotId,
    rl.LotName,
    v.Id AS VertexId,
    v.[Order],
    v.Latitude,
    v.Longitude,
    v.CreatedAt
FROM RecipeLotVertices v
JOIN RecipeLots rl ON rl.Id = v.LotId
JOIN Recipes r ON r.Id = rl.RecipeId
WHERE r.RfdNumber = @RfdNumber
  AND v.DeletedAt IS NULL
ORDER BY rl.Id, v.[Order];

SELECT
    h.Id,
    h.RecipeId,
    r.RfdNumber,
    h.OldStatus,
    h.NewStatus,
    h.ChangedAt,
    h.ChangedByUserId,
    u.UserName AS ChangedByUserName,
    h.Source,
    h.Notes
FROM dbo.RecipeStatusHistory h
JOIN dbo.Recipes r ON r.Id = h.RecipeId
LEFT JOIN dbo.Users u ON u.Id = h.ChangedByUserId
WHERE r.RfdNumber = @RfdNumber
ORDER BY h.ChangedAt ASC;
----------
