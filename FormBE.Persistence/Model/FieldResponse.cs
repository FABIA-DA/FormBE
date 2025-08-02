namespace FormBE.Persistence.Model;

public class FieldResponse
{
    public long Id { get; set; }
    public long FieldId { get; set; }
    public required string TelephoneNumber { get; set; }
    public required string Value { get; set; }
    public Instant SubmittedAt { get; set; }
    public required Field Field { get; set; }
}
