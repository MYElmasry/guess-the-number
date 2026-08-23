namespace GuessNumber.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int? BestScore { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
