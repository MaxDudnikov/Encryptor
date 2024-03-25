namespace Encryptor.Models
{
    internal static class FileExtensions
    {
        internal static eFileExtensions GetExtension(this string extension)
        {
            return extension switch
            {
                ".json" => eFileExtensions.JSON,
                ".ini" => eFileExtensions.INI,
                _ => eFileExtensions.NONE
            };
        }
    }

    internal enum eFileExtensions
    {
        JSON,
        INI,
        NONE
    }
}
