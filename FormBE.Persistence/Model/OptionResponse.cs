namespace FormBE.Persistence.Model;

public class OptionResponse
{
    public int Id { get; set; }
    public int OptionId { get; set; }
    public required string TelephoneNumber { get; set; }
    public Instant SubmittedAt { get; set; }
    public required Option Option { get; set; }
}
