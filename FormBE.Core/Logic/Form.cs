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
}

public sealed class FormDto
{
    public long Id { get; set; }
    public long? GroupId { get; set; }
    public required string Name { get; set; }
    public required GroupDto? Group { get; set; }
    public required List<FieldGroupDto> FieldGroups { get; set; }
}
