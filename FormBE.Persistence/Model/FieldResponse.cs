namespace FormBE.Persistence.Model;

public class FieldResponse
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public required string TelephoneNumber { get; set; }
    public required string Value { get; set; }
    public Instant SubmittedAt { get; set; }
    public required Field Field { get; set; }
}
