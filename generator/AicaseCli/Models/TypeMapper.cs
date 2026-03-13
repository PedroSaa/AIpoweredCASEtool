namespace AicaseCli.Models;

/// <summary>
/// Mapea tipos del JSON schema a tipos C#, TypeScript y SQL Server.
/// </summary>
public static class TypeMapper
{
    /// <summary>Convierte un tipo JSON a su equivalente en C#.</summary>
    public static string ToCSharp(string jsonType, bool isRequired, int? maxLength)
    {
        return jsonType switch
        {
            "string"  => "string" + (isRequired ? "" : "?"),
            "int"     => isRequired ? "int" : "int?",
            "decimal" => isRequired ? "decimal" : "decimal?",
            "double"  => isRequired ? "double" : "double?",
            "float"   => isRequired ? "float" : "float?",
            "long"    => isRequired ? "long" : "long?",
            "date"    => isRequired ? "DateOnly" : "DateOnly?",
            "datetime"=> isRequired ? "DateTime" : "DateTime?",
            "bool"    => isRequired ? "bool" : "bool?",
            "guid"    => isRequired ? "Guid" : "Guid?",
            "byte[]"  => "byte[]",
            _         => "string"
        };
    }

    /// <summary>Convierte un tipo JSON a su equivalente en TypeScript.</summary>
    public static string ToTypeScript(string jsonType)
    {
        return jsonType switch
        {
            "string"   => "string",
            "int"      => "number",
            "decimal"  => "number",
            "double"   => "number",
            "float"    => "number",
            "long"     => "number",
            "date"     => "Date",
            "datetime" => "Date",
            "bool"     => "boolean",
            "guid"     => "string",
            "byte[]"   => "string",
            _          => "string"
        };
    }

    /// <summary>Convierte un tipo JSON a su equivalente en SQL Server.</summary>
    public static string ToSql(string jsonType, int? maxLength, int? precision, int? scale)
    {
        return jsonType switch
        {
            "string"   => maxLength.HasValue ? $"NVARCHAR({maxLength})" : "NVARCHAR(MAX)",
            "int"      => "INT",
            "decimal"  => $"DECIMAL({precision ?? 18},{scale ?? 2})",
            "double"   => "FLOAT",
            "float"    => "REAL",
            "long"     => "BIGINT",
            "date"     => "DATE",
            "datetime" => "DATETIME2",
            "bool"     => "BIT",
            "guid"     => "UNIQUEIDENTIFIER",
            "byte[]"   => "VARBINARY(MAX)",
            _          => "NVARCHAR(MAX)"
        };
    }
}
