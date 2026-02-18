using APIAgroConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Application.Helpers
{
    /// <summary>
    /// Sets SQL Server SESSION_CONTEXT values before recipe status changes.
    /// The triggers TR_Recipes_StatusHistory and TR_Recipes_StatusHistory_Insert
    /// read these values to populate Source and Notes in RecipeStatusHistory.
    /// </summary>
    public static class StatusChangeContext
    {
        /// <summary>
        /// Sets SESSION_CONTEXT for the current DB connection before a status change.
        /// Must be called BEFORE SaveChangesAsync() when Recipe.Status is modified.
        /// </summary>
        /// <param name="db">The DbContext (must have an open connection or will open one)</param>
        /// <param name="source">Origin of the change: API_REVIEW, API_ASSIGN, IMPORT_PDF, API_MESSAGE</param>
        /// <param name="notes">Optional notes about the change (max 300 chars)</param>
        public static async Task SetAsync(AgroDbContext db, string source, string? notes = null)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            // sp_set_session_context persists for the lifetime of the connection
            await db.Database.ExecuteSqlRawAsync(
                "EXEC sp_set_session_context N'StatusChangeSource', {0}; " +
                "EXEC sp_set_session_context N'StatusChangeNotes', {1};",
                source,
                (object?)notes ?? DBNull.Value);
        }

        /// <summary>
        /// Clears SESSION_CONTEXT after SaveChanges to avoid leaking context
        /// to subsequent operations on the same connection.
        /// </summary>
        public static async Task ClearAsync(AgroDbContext db)
        {
            try
            {
                var sql = "EXEC sp_set_session_context N'StatusChangeSource', NULL; " +
                          "EXEC sp_set_session_context N'StatusChangeNotes', NULL;";
                await db.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // Non-critical: if clear fails, the next Set will overwrite
            }
        }

        // ── Source constants ──
        public const string SOURCE_REVIEW = "API_REVIEW";
        public const string SOURCE_ASSIGN = "API_ASSIGN";
        public const string SOURCE_IMPORT = "IMPORT_PDF";
        public const string SOURCE_MESSAGE = "API_MESSAGE";
    }
}