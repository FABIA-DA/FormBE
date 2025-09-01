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
}

public sealed class FieldGroupDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required List<SingleChoiceFieldDto> SingleChoiceFields { get; set; }
    public required List<FieldDto> Fields { get; set; }
}
