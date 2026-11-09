namespace Glitch.Classes;

public class ProfileMatchScore
{
    public Profile Profile { get; set; } = null!;
    public int SharedConditionsCount { get; set; }
    public int SharedInterestsCount { get; set; }
    public int MatchScore { get; set; }
}