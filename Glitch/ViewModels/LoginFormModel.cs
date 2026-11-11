
using System.ComponentModel.DataAnnotations;

namespace Glitch.ViewModels;

public class LoginFormModel
{
    [Required(ErrorMessage = "Skriv din email.")]
    [EmailAddress(ErrorMessage = "Skriv en gyldig email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skriv din adgangskode.")]
    public string Password { get; set; } = string.Empty;
}
