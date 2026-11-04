using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace Glitch.ViewModels
{
    public class ProfileFormModel
    {
        [Required(ErrorMessage = "Skriv dit profilnavn.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Skriv lidt om dig selv.")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vælg en fil et ny billede.")]
        public IBrowserFile? ProfileImage { get; set; }
    }
    
}