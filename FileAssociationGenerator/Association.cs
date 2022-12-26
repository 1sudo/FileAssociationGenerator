using System.Text.Json.Serialization;

namespace FileAssociationGenerator;

internal class Association
{
    [JsonPropertyName("source_file")]
    public string? SourceFile {get;set;}
    [JsonPropertyName("associated_file")]
    public string? AssociatedFile { get;set;}
}
