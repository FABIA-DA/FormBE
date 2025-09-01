using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class FieldExtension
{
    public static FieldDto ToDto(this Field self) =>
        new FieldDto()
        {
            Id = self.Id,
            FieldTypeId = self.FieldTypeId,
            Name = self.Name,
            Description = self.Description,
            IsOptional = self.IsOptional,
            Type = self.FieldType.ToDto()
        };
}

public sealed class FieldDto
{
    public long Id { get; set; }
    public long FieldTypeId { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public bool IsOptional { get; set; }
    public required FieldTypeDto Type { get; set; }
}
