namespace FormBE.Persistence.Model;

public class OptionResponse
{
    public long Id { get; set; }
    public long OptionId { get; set; }
    public required string TelephoneNumber { get; set; }
    public Instant SubmittedAt { get; set; }
    public required Option Option { get; set; }
}
