namespace PasswordApi.Data;

public class PasswordEntry
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
