using Glitch.Classes;

namespace Glitch.Methods
{
    public class Verify
    {
        public static bool IsValidEmail(string email)
        {
            //missing list of emails from database to actaullally compare with
            List<User> Users = new List<User>();
            foreach (var User in Users)
            {
                
                if (string.Equals(User.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age >= 18;
        }
    }
}
