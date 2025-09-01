using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class FieldTypeExtension
{
    public static FieldTypeDto ToDto(this FieldType self) =>
        new FieldTypeDto()
        {
            Id = self.Id,
            Name = self.Name,
            Description = self.Description,
            Regex = self.Regex
        };
}

public sealed class FieldTypeDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required string Regex { get; set; }
}
