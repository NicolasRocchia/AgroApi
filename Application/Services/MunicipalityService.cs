using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Contracts.Requests;
using APIAgroConnect.Contracts.Responses;
using APIAgroConnect.Domain.Entities;
using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Services
{
    public class MunicipalityService : IMunicipalityService
    {
        private readonly AgroDbContext _context;

        public MunicipalityService(AgroDbContext context)
        {
            _context = context;
        }

        private static void ValidateCoords(decimal? lat, decimal? lng)
        {
            if (lat is not null && (lat < -90 || lat > 90))
                throw new ArgumentException("Latitud inválida. Debe estar entre -90 y 90.");

            if (lng is not null && (lng < -180 || lng > 180))
                throw new ArgumentException("Longitud inválida. Debe estar entre -180 y 180.");
        }

        // ─────────────────────────────────────────
        // CRUD
        // ─────────────────────────────────────────

        public async Task<List<MunicipalityDto>> GetAllAsync()
        {
            return await _context.Municipalities
                .Include(m => m.User)
                .OrderBy(m => m.Name)
                .Select(m => MapToDto(m, null, null))
                .ToListAsync();
        }

        public async Task<List<MunicipalityDto>> GetNearbyAsync(decimal latitude, decimal longitude, int limit = 10)
        {
            // validar input
            ValidateCoords(latitude, longitude);

            // Fórmula simplificada de distancia (Haversine aproximado para distancias cortas)
            var lat = (double)latitude;
            var lng = (double)longitude;

            var municipalities = await _context.Municipalities
                .Include(m => m.User)
                .Where(m => m.Latitude != null && m.Longitude != null)
                .ToListAsync();

            return municipalities
                .Select(m =>
                {
                    var dLat = ((double)m.Latitude! - lat) * Math.PI / 180;
                    var dLng = ((double)m.Longitude! - lng) * Math.PI / 180;
                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                            Math.Cos(lat * Math.PI / 180) * Math.Cos((double)m.Latitude! * Math.PI / 180) *
                            Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var distKm = 6371 * c;

                    return new MunicipalityDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Province = m.Province,
                        Department = m.Department,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        UserId = m.UserId,
                        UserName = m.User?.UserName,
                        DistanceKm = Math.Round(distKm, 1)
                    };
                })
                .OrderBy(m => m.DistanceKm)
                .Take(limit)
                .ToList();
        }

        public async Task<MunicipalityDto?> GetByIdAsync(long id)
        {
            var m = await _context.Municipalities
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null) return null;

            return new MunicipalityDto
            {
                Id = m.Id,
                Name = m.Name,
                Province = m.Province,
                Department = m.Department,
                Latitude = m.Latitude,
                Longitude = m.Longitude,
                UserId = m.UserId,
                UserName = m.User?.UserName
            };
        }

        public async Task<MunicipalityDto> CreateAsync(CreateMunicipalityRequest request, long userId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("El nombre del municipio es obligatorio.");

            ValidateCoords(request.Latitude, request.Longitude);

            var municipality = new Municipality
            {
                Name = request.Name.Trim(),
                Province = request.Province?.Trim(),
                Department = request.Department?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _context.Municipalities.Add(municipality);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Esto captura, por ejemplo, overflow de DECIMAL(10,7) cuando te llega -32692014
                throw new InvalidOperationException("No se pudo guardar el municipio. Verificá las coordenadas (lat/lng) y los datos enviados.", ex);
            }

            return (await GetByIdAsync(municipality.Id))!;
        }

        public async Task UpdateAsync(long id, UpdateMunicipalityRequest request, long userId)
        {
            var m = await _context.Municipalities.FindAsync(id)
                ?? throw new InvalidOperationException($"No se encontró el municipio con ID {id}.");

            // validar coords SOLO si vienen en el request (por tu lógica actual)
            var latToValidate = request.Latitude.HasValue ? request.Latitude : null;
            var lngToValidate = request.Longitude.HasValue ? request.Longitude : null;
            ValidateCoords(latToValidate, lngToValidate);

            if (request.Name != null) m.Name = request.Name.Trim();
            if (request.Province != null) m.Province = request.Province.Trim();
            if (request.Department != null) m.Department = request.Department.Trim();

            if (request.Latitude.HasValue) m.Latitude = request.Latitude;
            if (request.Longitude.HasValue) m.Longitude = request.Longitude;

            if (request.UserId.HasValue) m.UserId = request.UserId;

            m.UpdatedAt = DateTime.UtcNow;
            m.UpdatedByUserId = userId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("No se pudo actualizar el municipio. Verificá las coordenadas (lat/lng) y los datos enviados.", ex);
            }
        }

        // ─────────────────────────────────────────
        // FLUJO DE ASIGNACIÓN
        // ─────────────────────────────────────────

        public async Task AssignToMunicipalityAsync(long recipeId, long municipalityId, long userId)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId)
                ?? throw new InvalidOperationException("Receta no encontrada.");

            if (recipe.Status != "ABIERTA")
                throw new InvalidOperationException("Solo se pueden enviar a municipio las recetas con estado ABIERTA.");

            var municipality = await _context.Municipalities.FindAsync(municipalityId)
                ?? throw new InvalidOperationException("Municipio no encontrado.");

            recipe.AssignedMunicipalityId = municipalityId;
            recipe.AssignedAt = DateTime.UtcNow;
            recipe.Status = "PENDIENTE";
            recipe.UpdatedAt = DateTime.UtcNow;
            recipe.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        public async Task ReviewRecipeAsync(long recipeId, ReviewRecipeRequest request, long municipalityId, long userId)
        {
            var validActions = new[] { "APROBADA", "RECHAZADA", "OBSERVADA", "REDIRIGIDA" };
            var action = request.Action.ToUpper().Trim();

            if (!validActions.Contains(action))
                throw new ArgumentException($"Acción inválida: {action}. Acciones válidas: {string.Join(", ", validActions)}");

            var recipe = await _context.Recipes.FindAsync(recipeId)
                ?? throw new InvalidOperationException("Receta no encontrada.");

            if (recipe.AssignedMunicipalityId != municipalityId)
                throw new UnauthorizedAccessException("Esta receta no está asignada a tu municipio.");

            if (recipe.Status != "PENDIENTE")
                throw new InvalidOperationException($"No se puede revisar una receta con estado {recipe.Status}. Solo se pueden revisar recetas PENDIENTES.");

            if (action == "RECHAZADA" && string.IsNullOrWhiteSpace(request.Observation))
                throw new ArgumentException("Debe incluir una observación al rechazar una receta.");

            if (action == "OBSERVADA" && string.IsNullOrWhiteSpace(request.Observation))
                throw new ArgumentException("Debe incluir una observación al solicitar más información.");

            if (action == "REDIRIGIDA")
            {
                if (!request.TargetMunicipalityId.HasValue)
                    throw new ArgumentException("Debe indicar el municipio destino al redirigir.");

                if (request.TargetMunicipalityId == municipalityId)
                    throw new ArgumentException("No se puede redirigir al mismo municipio.");

                var target = await _context.Municipalities.FindAsync(request.TargetMunicipalityId)
                    ?? throw new InvalidOperationException("Municipio destino no encontrado.");
            }

            var log = new RecipeReviewLog
            {
                RecipeId = recipeId,
                MunicipalityId = municipalityId,
                Action = action,
                TargetMunicipalityId = action == "REDIRIGIDA" ? request.TargetMunicipalityId : null,
                Observation = request.Observation,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            _context.RecipeReviewLogs.Add(log);

            switch (action)
            {
                case "APROBADA":
                    recipe.Status = "APROBADA";
                    break;
                case "RECHAZADA":
                    recipe.Status = "RECHAZADA";
                    break;
                case "OBSERVADA":
                    recipe.Status = "OBSERVADA";
                    break;
                case "REDIRIGIDA":
                    recipe.AssignedMunicipalityId = request.TargetMunicipalityId;
                    recipe.AssignedAt = DateTime.UtcNow;
                    break;
            }

            recipe.UpdatedAt = DateTime.UtcNow;
            recipe.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // MENSAJES
        // ─────────────────────────────────────────

        public async Task<RecipeMessageDto> SendMessageAsync(long recipeId, string message, long userId)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId)
                ?? throw new InvalidOperationException("Receta no encontrada.");

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new InvalidOperationException("Usuario no encontrado.");

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Desconocido";

            // Si es Aplicador respondiendo a OBSERVADA, volver a PENDIENTE
            if (roleName == "Aplicador" && recipe.Status == "OBSERVADA")
            {
                recipe.Status = "PENDIENTE";
                recipe.UpdatedAt = DateTime.UtcNow;
                recipe.UpdatedByUserId = userId;
            }

            var msg = new RecipeMessage
            {
                RecipeId = recipeId,
                SenderUserId = userId,
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.RecipeMessages.Add(msg);
            await _context.SaveChangesAsync();

            return new RecipeMessageDto
            {
                Id = msg.Id,
                SenderUserId = userId,
                SenderName = user.UserName,
                SenderRole = roleName,
                Message = msg.Message,
                CreatedAt = msg.CreatedAt
            };
        }

        public async Task<List<RecipeMessageDto>> GetMessagesAsync(long recipeId)
        {
            return await _context.RecipeMessages
                .Where(m => m.RecipeId == recipeId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new RecipeMessageDto
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    SenderName = m.Sender.UserName,
                    SenderRole = m.Sender.UserRoles
                        .Select(ur => ur.Role.Name)
                        .FirstOrDefault() ?? "Desconocido",
                    Message = m.Message,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // UTILIDADES
        // ─────────────────────────────────────────

        public async Task<long?> GetMunicipalityIdByUserIdAsync(long userId)
        {
            return await _context.Municipalities
                .Where(m => m.UserId == userId)
                .Select(m => (long?)m.Id)
                .FirstOrDefaultAsync();
        }

        // ---------------------------------------------------------
        private static MunicipalityDto MapToDto(Municipality m, decimal? lat, decimal? lng)
        {
            // Tu implementación original (si ya la tenías)
            return new MunicipalityDto
            {
                Id = m.Id,
                Name = m.Name,
                Province = m.Province,
                Department = m.Department,
                Latitude = m.Latitude,
                Longitude = m.Longitude,
                UserId = m.UserId,
                UserName = m.User?.UserName
            };
        }
    }
}
