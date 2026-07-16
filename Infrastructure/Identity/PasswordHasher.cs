using Application.Interfaces;
using System.Security.Cryptography;

namespace Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600_000;
    private const int LegacyIterations = 100_000;
    private const int MaximumAcceptedIterations = 1_000_000;
    private const char Delimiter = '.';

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join(
            Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var parts = passwordHash.Split(Delimiter);

            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var iterations) ||
                iterations < LegacyIterations ||
                iterations > MaximumAcceptedIterations)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            if (salt.Length != SaltSize || expectedHash.Length != KeySize)
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return true;
        }

        var parts = passwordHash.Split(Delimiter);
        return parts.Length != 3 ||
               !int.TryParse(parts[0], out var iterations) ||
               iterations != Iterations;
    }
}
