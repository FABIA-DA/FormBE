using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class FieldResponseExtension
{
    public static FieldResponseDto ToDto(this FieldResponse self) =>
        new FieldResponseDto()
        {
            Id = self.Id,
            FieldId = self.FieldId,
            TelephoneNumber = self.TelephoneNumber,
            Value = self.Value,
            SubmittedAt = self.SubmittedAt
        };
}

public sealed class FieldResponseDto
{
    public long Id { get; set; }
    public long FieldId { get; set; }
    public required string TelephoneNumber { get; set; }
    public required string Value { get; set; }
    public Instant SubmittedAt { get; set; }
}
