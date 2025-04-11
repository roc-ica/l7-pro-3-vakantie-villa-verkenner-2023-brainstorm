using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace VillaVerkenerAPI.Services;

public class PasswordHasher
{
    /// <summary>
    /// Generates a hash from a password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hash in the PHC format</returns>
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

        const int iterations = 600000;

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iterations,
            numBytesRequested: 256 / 8));

        // format: $[ID]$[version]$[parameters]$[salt]$[hash]
        // (https://github.com/P-H-C/phc-string-format/blob/master/phc-sf-spec.md)
        return $"$pbkdf2-sha256$v=1$i={iterations}${Convert.ToBase64String(salt)}${hashed}";
    }

    /// <summary>
    /// Validates a password to a hash
    /// </summary>
    /// <param name="password">the password string</param>
    /// <param name="hash">the password hash to compare to (in PHC format)</param>
    /// <returns>true if the password is correct, false if it's incorrect</returns>
    /// <exception cref="FormatException"></exception>
    public static bool ValidatePassword(string password, string hash)
    {
        string[] parts = hash.Split('$');

        if (
            parts.Length != 6 ||
            parts[0] != string.Empty ||
            parts[1] == string.Empty ||
            parts[2] == string.Empty ||
            parts[3] == string.Empty ||
            parts[4] == string.Empty ||
            parts[5] == string.Empty
            ) throw new FormatException("Invalid Format");

        if (
            parts[1] != "pbkdf2-sha256" ||
            parts[2] != "v=1"
            ) throw new FormatException("Invalid or unsupported ID or Version");

        string[] parameters = parts[3].Split(',');
        if (parameters[0].Split("=")[0] != "i" || !int.TryParse(parameters[0].Split("=")[1], out int iterations))
            throw new FormatException("Invalid or missing parameter 'i'");

        byte[] salt;
        byte[] storedHash;

        try
        {
            salt = Convert.FromBase64String(parts[4]);
            storedHash = Convert.FromBase64String(parts[5]);
        }
        catch (FormatException)
        {
            throw new FormatException("Invalid Salt or Hash");
        }

        byte[] newHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iterations,
            numBytesRequested: storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(newHash, storedHash);
    }
}
