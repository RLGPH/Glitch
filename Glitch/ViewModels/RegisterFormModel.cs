
using System.ComponentModel.DataAnnotations;

namespace Glitch.ViewModels;

public class RegisterUserFormModel : IValidatableObject
{
    [Required(ErrorMessage = "Email skal udfyldes.")]
    [EmailAddress(ErrorMessage = "Indtast en gyldig email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adgangskode skal udfyldes.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gentag din adgangskode.")]
    [Compare(nameof(Password),
        ErrorMessage = "Adgangskoderne er ikke ens.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vælg din fødselsdato.")]
    public DateTime? BirthDate { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (BirthDate is null)
            yield break;

        var today = DateTime.Today;

        if (BirthDate.Value.Date > today)
        {
            yield return new ValidationResult(
                "Fødselsdato kan ikke ligge i fremtiden.",
                new[] { nameof(BirthDate) });
        }
        else if (BirthDate.Value.Date > today.AddYears(-18))
        {
            yield return new ValidationResult(
                "Du skal være mindst 18 år.",
                new[] { nameof(BirthDate) });
        }
    }
}
