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
                ".xml" => eFileExtensions.XML,
                ".config" => eFileExtensions.XML,
                _ => eFileExtensions.NONE
            };
        }
    }

    internal enum eFileExtensions
    {
        JSON,
        INI,
        XML,
        NONE
    }
}
