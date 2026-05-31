namespace FoodDelivery.Web.Validation;

public enum PasswordStrengthLevel
{
    Empty,
    Weak,
    Fair,
    Strong
}

public readonly record struct PasswordStrengthResult(
    PasswordStrengthLevel Level,
    int Score,
    int MaxScore,
    bool IsAcceptable,
    IReadOnlyList<string> UnmetRequirements)
{
    public static PasswordStrengthResult Evaluate(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return new(PasswordStrengthLevel.Empty, 0, 5, false, ["Enter a password."]);

        var unmet = new List<string>();
        var score = 0;

        if (password.Length >= 8)
            score++;
        else
            unmet.Add("At least 8 characters");

        if (password.Any(char.IsUpper))
            score++;
        else
            unmet.Add("One uppercase letter");

        if (password.Any(char.IsLower))
            score++;
        else
            unmet.Add("One lowercase letter");

        if (password.Any(char.IsDigit))
            score++;
        else
            unmet.Add("One number");

        if (password.Any(c => !char.IsLetterOrDigit(c)))
            score++;
        else
            unmet.Add("One special character");

        var level = score switch
        {
            0 or 1 or 2 => PasswordStrengthLevel.Weak,
            3 or 4 => PasswordStrengthLevel.Fair,
            _ => PasswordStrengthLevel.Strong
        };

        const int requiredScore = 5;
        return new(level, score, requiredScore, score >= requiredScore, unmet);
    }
}
