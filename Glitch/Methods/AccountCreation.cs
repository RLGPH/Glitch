using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;
using Glitch.Methods;

namespace Glitch.Methods;

public class AccountCreation
{
    private readonly GlitchDbContext _context;

    public AccountCreation(GlitchDbContext context)
    {
        _context = context;
    }
    
    public async Task<RegistrationResult> RegisterNewUserAsync(string email, string password, DateTime birthDate, string displayName)
    {
        if (!Verify.IsValidAge(birthDate))
        {
            return new RegistrationResult 
            { 
                Success = false, 
                Message = "Registration failed. You must be at least 18 years old to join Glitch." 
            };
        }
        
        bool emailExists = Verify.IsValidEmail(email);
        if (emailExists)
        {
            return new RegistrationResult 
            { 
                Success = false, 
                Message = "This email address is already registered." 
            };
        }
        
        var rightNow = DateTime.UtcNow;

        // 3. Initialize the User and their nested Profile record 
        var newUser = new User
        {
            Email = email,
            Password = password, 
            BirthDate = birthDate,
            CreatedAt = rightNow,
            IsActive = true,
            Profile = new Profile
            {
                DisplayName = displayName,
                Bio = "Welcome to my profile!", 
                ImagePath = "images/default-avatar.png",
                CreatedAt = rightNow,
                LastUpdatedAt = rightNow
            }
        };

        try
        {
            // 4. Add to context and persist to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new RegistrationResult
            {
                Success = true,
                Message = "Account created successfully!",
                CreatedUser = newUser
            };
        }
        catch (Exception ex)
        {
            return new RegistrationResult
            {
                Success = false,
                Message = $"An error occurred while saving to the database: {ex.Message}"
            };
        }
    }
}
