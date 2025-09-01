using FormBE.Persistence.Model;

namespace FormBE.Core.Logic;

public static class OptionResponseExtension
{
    public static OptionResponseDto ToDto(this OptionResponse self) =>
        new OptionResponseDto()
        {
            Id = self.Id,
            OptionId = self.OptionId,
            TelephoneNumber = self.TelephoneNumber,
            SubmittedAt = self.SubmittedAt
        };
}

public sealed class OptionResponseDto
{
    public long Id  { get; set; }
    public long OptionId { get; set; }
    public required string TelephoneNumber { get; set; }
    public Instant SubmittedAt { get; set; }
}
