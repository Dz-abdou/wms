using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Globalization;
using Warehouse.Application.Common.Numbering;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Numbering;

public sealed class DocumentNumberService(WarehouseDbContext dbContext) : IDocumentNumberService
{
    public async Task<string> AllocateAsync(string definitionCode, DateTime occurredAtUtc, CancellationToken cancellationToken)
    {
        if (occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The document-number allocation timestamp must be UTC.", nameof(occurredAtUtc));
        }

        var code = definitionCode.Trim().ToUpperInvariant();
        var definition = await dbContext.DocumentNumberDefinitions.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Code == code && candidate.IsActive, cancellationToken)
            ?? throw new DocumentNumberDefinitionUnavailableException(code);

        var year = occurredAtUtc.Year;
        var connection = dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection) await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = """
            INSERT INTO "DocumentNumberSeries" ("Id", "DefinitionCode", "Year", "NextValue")
            VALUES (@id, @definitionCode, @year, 2)
            ON CONFLICT ("DefinitionCode", "Year")
            DO UPDATE SET "NextValue" = "DocumentNumberSeries"."NextValue" + 1
            RETURNING "NextValue" - 1 AS "Value"
            """;
        AddParameter(command, "@id", Guid.NewGuid());
        AddParameter(command, "@definitionCode", code);
        AddParameter(command, "@year", year);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var allocatedValue = Convert.ToInt64(result, CultureInfo.InvariantCulture);

            var maximumValue = Pow10(definition.DigitCount) - 1;
            if (allocatedValue > maximumValue)
            {
                throw new DocumentNumberCapacityExceededException(code, year);
            }

            var formattedValue = allocatedValue.ToString($"D{definition.DigitCount}", CultureInfo.InvariantCulture);
            return $"{definition.Prefix}-{year}-{formattedValue}";
        }
        finally
        {
            if (shouldCloseConnection) await connection.CloseAsync();
        }
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static long Pow10(int exponent)
    {
        var value = 1L;
        for (var index = 0; index < exponent; index++) value *= 10;
        return value;
    }
}
