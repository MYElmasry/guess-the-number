namespace GuessNumber.Domain.Entities;

public class Game
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int SecretNumber { get; set; }
    public int AttemptCount { get; set; }
    public bool IsCompleted { get; set; }
    public int HintsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
