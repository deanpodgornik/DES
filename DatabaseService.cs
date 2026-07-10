using Microsoft.Data.SqlClient;

namespace ScreenAutoClicker;

record UserEntryInfo(string Name, string FullCode, decimal UsedEntries, decimal TotalEntries, bool Unlimited, DateTime ValidTo);

class DatabaseService
{
    private readonly string _connectionString;
    private readonly int _codeSearchLength;

    /// <param name="connectionString">ADO.NET SQL Server connection string.</param>
    /// <param name="codeSearchLength">
    ///   How many characters from the END of the captured keyboard code to use
    ///   when searching <c>Contact.Code LIKE '%' + @suffix</c>.
    ///   Set to 0 to use the full captured string.
    /// </param>
    public DatabaseService(string connectionString, int codeSearchLength = 6)
    {
        _connectionString = connectionString;
        _codeSearchLength = codeSearchLength;
    }

    /// <summary>
    /// Looks up remaining visit entries for a user identified by their
    /// keyboard-captured code.  Returns null if no matching active subscription found.
    /// </summary>
    public async Task<UserEntryInfo?> GetUserEntriesAsync(string capturedCode)
    {
        // DbCodeSearchLength = 0 → exact match on the full captured code (uses index)
        // DbCodeSearchLength > 0 → match on the last N characters via LIKE '%suffix'
        bool exactMatch = _codeSearchLength == 0 || capturedCode.Length <= _codeSearchLength;
        string searchValue = (!exactMatch)
            ? capturedCode[^_codeSearchLength..]
            : capturedCode;

        // Query:
        //   Contact  (Code = @code  OR  Code LIKE '%suffix')
        //     -> TaskCard   (Active=1, idContactUse)
        //       -> TaskScheduleCard  (DoCount > 0)
        // Remaining = DoCount - DoAlready
        // Pick the row with the most recent LastDateUse (most recently used subscription).
        // DoCount = 0 pomeni časovna kartica (mesečna/letna) — brez omejitve vstopov.
        // Takšnih kartic NE izključujemo, a prikaz je drugačen (veljavna do ...).
        // tsc.Active = 1  → izloči porabljene vstopne kartice (DoAlready >= DoCount)
        // tsc.DateTo >= GETDATE()  → izloči časovne kartice s pretekło veljavnostjo
        string whereClause = exactMatch ? "c.Code = @code" : "c.Code LIKE '%' + @code";
        string sql = $@"
            SELECT TOP 1
                c.Contact            AS Name,
                c.Code               AS FullCode,
                tsc.DoAlready        AS UsedEntries,
                tsc.DoCount          AS TotalEntries,
                tsc.DateTo           AS ValidTo
            FROM   Contact c
            JOIN   TaskCard           tc  ON tc.idContactUse = c.idContact
                                         AND tc.Active = 1
            JOIN   TaskScheduleCard   tsc ON tsc.idTaskCard  = tc.idTaskCard
                                         AND tsc.Active = 1
                                         AND tsc.DateTo >= GETDATE()
            WHERE  {whereClause}
            ORDER  BY tc.LastDateUse DESC, tsc.idTaskScheduleCard DESC";

        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@code", System.Data.SqlDbType.NVarChar, 100)
            {
                Value = searchValue
            });

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string name     = reader.IsDBNull(0) ? "" : reader.GetString(0);
                string fullCode = reader.IsDBNull(1) ? "" : reader.GetString(1);
                decimal used    = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                decimal total   = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                DateTime validTo = reader.IsDBNull(4) ? DateTime.MaxValue : reader.GetDateTime(4);
                bool unlimited  = total == 0;
                return new UserEntryInfo(name, fullCode, used, total, unlimited, validTo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] Napaka pri poizvedbi: {ex.Message}");
        }

        return null;
    }
}
