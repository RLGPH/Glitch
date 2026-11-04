using Glitch.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

public class LoginService
{
    public LoginResult Authenticate(string email, string password)
    {
        
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResult { Success = false, Message = "Email and password are required." };
        }

        // 
        /*var user = "list of users gotten from db".FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return new LoginResult { Success = false, Message = "Invalid email or password." };
        }

        if (!user.IsActive)
        {
            return new LoginResult { Success = false, Message = "This account has been deactivated." };
        }

        if (user.Password != password)
        {
            return new LoginResult { Success = false, Message = "Invalid email or password." };
        }

        return new LoginResult
        {
            Success = true,
            Message = "Login successful.",
            UserProfile = user.Profile
        };*/

        // remove after db is able to be used
        return new LoginResult { Success = true, Message = "Login successful.", UserProfile = new Profile { DisplayName = "Test User" } };
    }
}