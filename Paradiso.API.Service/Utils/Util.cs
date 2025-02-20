using System.Security.Cryptography;

namespace Paradiso.API.Service.Utils;

public static class Util
{
    public static string GetHashCodeFromFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ExceptionDto() { Message = EException.FileNotSelected.DisplayName() };

        using var sha256 = SHA256.Create();
        using var stream = file.OpenReadStream();

        var hashBytes = sha256.ComputeHash(stream);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "");

        return hash;
    }

    public static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }
}
