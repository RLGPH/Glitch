
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace Glitch.ViewModels;

public class ProfileFormModel
{
    [Required(ErrorMessage = "Skriv dit profilnavn.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skriv lidt om dig selv.")]
    public string Bio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vælg et profilbillede.")]
    public IBrowserFile? ProfileImage { get; set; }

    // Interesser brugeren har valgt
    public List<string> SelectedInterests { get; set; } = new();

    // Diagnoser/handicap brugeren har valgt
    public List<string> SelectedConditions { get; set; } = new();

    // Brugerdefineret diagnose/handicap under "Andet"
    public string CustomCondition { get; set; } = string.Empty;
}
