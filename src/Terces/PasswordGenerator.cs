using System.Security.Cryptography;

namespace Terces;

/// <summary>
/// Provides functionality to generate secure random passwords.
/// </summary>
/// <remarks>
/// The PasswordGenerator class is designed to create passwords that meet common security requirements,
/// such as the inclusion of uppercase letters, lowercase letters, numbers, and special characters.
/// Passwords are generated using a cryptographically secure random number generator.
/// </remarks>
public static class PasswordGenerator
{
    private static readonly RandomNumberGenerator Generator = RandomNumberGenerator.Create();

    /// <summary>
    /// Generate a random password of the specified length.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <remarks>
    /// Implement password generation logic according to Azure PostgreSQL requirements
    /// PostgreSQL passwords typically require:
    /// - Minimum 8 characters
    /// - Include uppercase, lowercase, numbers, and special characters
    /// </remarks>
    public static string Generate(int length)
    {
        if (length < 8) length = 8;
        
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        const string allChars = upperChars + lowerChars + numbers + specialChars;

        var indices = new byte[length];
        // generate random numbers for password characters
        Generator.GetBytes(indices);
        
        var passwordChars = new char[length];
        passwordChars[0] = upperChars[indices[0] % upperChars.Length];
        passwordChars[1] = upperChars[indices[1] % upperChars.Length];
        passwordChars[2] = lowerChars[indices[2] % lowerChars.Length];
        passwordChars[3] = lowerChars[indices[3] % lowerChars.Length];
        passwordChars[4] = numbers[indices[4] % numbers.Length];
        passwordChars[5] = numbers[indices[5] % numbers.Length];
        passwordChars[6] = specialChars[indices[6] % specialChars.Length];
        
        for (var i = 7; i < length; i++)
        {
            passwordChars[i] = allChars[indices[i] % allChars.Length];
        }
        
        // regenerate random numbers for sorting
        Generator.GetBytes(indices);
        for (var i = 0; i < length; i++)
        {
            var j = indices[i] % length;
            (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
        }
        
        return new string(passwordChars);
    }
}