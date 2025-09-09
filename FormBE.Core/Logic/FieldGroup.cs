using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class FieldGroupExtension
{
    public static FieldGroupDto ToDto(this FieldGroup self) =>
        new()
        {
            Id = self.Id,
            Name = self.Name,
            SingleChoiceFields = self.FieldGroupSingleChoiceFields.Select(fgscf => fgscf.SingleChoiceField.ToDto())
                                     .ToList(),
            Fields = self.FieldGroupFields.Select(fgf => fgf.Field.ToDto()).ToList()
        };

    public static FieldGroupListDto ToListDto(this (long Id, string Name, int SingleChoiceFieldCount, int FieldCount) self) =>
        new FieldGroupListDto()
        {
            Id = self.Id,
            Name = self.Name,
            SingleChoiceFieldCount = self.SingleChoiceFieldCount,
            FieldCount = self.FieldCount
        };
}

public sealed class FieldGroupDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required List<SingleChoiceFieldDto> SingleChoiceFields { get; set; }
    public required List<FieldDto> Fields { get; set; }
}

public sealed class FieldGroupListDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public int SingleChoiceFieldCount { get; set; }
    public int FieldCount { get; set; }
}
