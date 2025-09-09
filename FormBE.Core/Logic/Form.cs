using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class FormExtension
{
    public static FormDto ToDto(this Form self) =>
        new FormDto()
        {
            Id = self.Id,
            GroupId = self.GroupId,
            Name = self.Name,
            Group =  self.Group?.ToDto(),
            FieldGroups = self.FormFieldGroups.Select<FormFieldGroup, FieldGroupDto>(ffg => ffg.FieldGroup.ToDto())
                                  .ToList()
        };

    public static FormListDto ToListDto(this Form self) =>
        new FormListDto()
        {
            Id = self.Id,
            Name = self.Name,
            GroupId = self.GroupId,
            GroupName = self.Group?.Name,
            FieldGroupCount = self.FormFieldGroups.Count
        };
}

public sealed class FormDto
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public required string Name { get; set; }
    public required GroupDto? Group { get; set; }
    public required List<FieldGroupDto> FieldGroups { get; set; }
}

public sealed class FormListDto
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public required string Name { get; set; }
    public required string? GroupName { get; set; }
    public int FieldGroupCount { get; set; }
}