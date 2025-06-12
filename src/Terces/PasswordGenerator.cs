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
    private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
    private const string AlphaNumerics = UpperChars + LowerChars + Numbers;
    private const string AllChars = UpperChars + LowerChars + Numbers + SpecialChars;
    
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

        var indices = new byte[length];
        // generate random numbers for password characters
        Generator.GetBytes(indices);
        
        var passwordChars = new char[length];
        passwordChars[0] = UpperChars[indices[0] % UpperChars.Length];
        passwordChars[1] = UpperChars[indices[1] % UpperChars.Length];
        passwordChars[2] = LowerChars[indices[2] % LowerChars.Length];
        passwordChars[3] = LowerChars[indices[3] % LowerChars.Length];
        passwordChars[4] = Numbers[indices[4] % Numbers.Length];
        passwordChars[5] = Numbers[indices[5] % Numbers.Length];
        passwordChars[6] = SpecialChars[indices[6] % SpecialChars.Length];
        
        for (var i = 7; i < length; i++)
        {
            passwordChars[i] = AllChars[indices[i] % AllChars.Length];
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

    /// <summary>
    /// Generate a random username with the specified prefix and total length.
    /// </summary>
    /// <param name="prefix">The prefix to be added at the beginning of the username.
    /// If null or empty, a default prefix "u" will be used.</param>
    /// <param name="length">The total length of the username to be generated, including the prefix.
    /// Must be at least 8 characters.</param>
    /// <returns>A randomly generated username with the specified prefix and total length.</returns>
    /// <remarks>
    /// The generated username will consist of the prefix followed by random alphanumeric characters.
    /// If the specified length is less than 8, the default minimum length of 8 will be enforced.
    /// </remarks>
    public static string GenerateUsername(string prefix, int length)
    {
        if (length < 8) length = 8;
        if (string.IsNullOrEmpty(prefix))
            prefix = "u";
        
        var indices = new byte[length - prefix.Length];
        Generator.GetBytes(indices);
        
        var username = prefix.Concat(
            indices.Select(i => AlphaNumerics[i % AlphaNumerics.Length]))
            .ToString();

        return username!;
    }
}