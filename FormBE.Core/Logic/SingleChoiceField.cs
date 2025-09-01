using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class SingleChoiceFieldExtension
{
    public static SingleChoiceFieldDto ToDto(this SingleChoiceField self) =>
        new SingleChoiceFieldDto()
        {
            Id = self.Id,
            Name = self.Name,
            Options = self.Options.Select(ToDto).ToList()
        };

    private static OptionDto ToDto(this Option self) =>
        new OptionDto()
        {
            Id = self.Id,
            Name = self.Name,
            SingleChoiceFieldId = self.SingleChoiceFieldId,
            Fields = self.OptionFields.Select(of => of.Field.ToDto()).ToList()
        };
}

public sealed class SingleChoiceFieldDto
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required List<OptionDto> Options { get; set; }
}

public sealed class OptionDto
{
    public long Id { get; set; }
    public long SingleChoiceFieldId { get; set; }
    public required string Name { get; set; }
    public required List<FieldDto> Fields { get; set; }
}