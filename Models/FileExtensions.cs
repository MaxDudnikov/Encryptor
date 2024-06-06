using System.Collections.Generic;
using Avalonia.Platform.Storage;
using Newtonsoft.Json.Linq;

namespace Encryptor.Models
{
    internal static class FileExtensions
    {
        internal static EFileExtensions GetExtension(this string extension)
        {
            return extension switch
            {
                ".json" => EFileExtensions.JSON,
                ".ini" => EFileExtensions.INI,
                ".config" => EFileExtensions.CONFIG,
                _ => EFileExtensions.NONE
            };
        }

        internal static FilePickerOpenOptions filePickerOpenOptions = new()
        {
            FileTypeFilter = new List<FilePickerFileType>(){
                new("json")
                {
                    Patterns = new List<string>() { "*.json"}
                },
                new("ini")
                {
                    Patterns = new List<string>() { "*.ini"}
                },
                new("config")
                {
                    Patterns = new List<string>() { "*.config"}
                }
            }
        };

        public static string GetValue(this Settings value)
        {
            string result = string.Empty;
            switch (value.JTypeValue)
            {
                case JTokenType.None:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Object:
                    result = value.Value.ToString().Replace("\r\n  ", "\r\n    ").Replace("\r\n}", "\r\n  }");
                    break;
                case JTokenType.Array:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Constructor:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Property:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Comment:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Integer:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Float:
                    result = value.Value.ToString();
                    break;
                case JTokenType.String:
                    result = $"\"{value.Value}\"";
                    break;
                case JTokenType.Boolean:
                    result = value.Value.ToString().ToLower();
                    break;
                case JTokenType.Null:
                    result = "null";
                    break;
                case JTokenType.Undefined:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Date:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Raw:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Bytes:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Guid:
                    result = value.Value.ToString();
                    break;
                case JTokenType.Uri:
                    result = value.Value.ToString();
                    break;
                case JTokenType.TimeSpan:
                    result = value.Value.ToString();
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public enum EFileExtensions
    {
        JSON,
        INI,
        CONFIG,
        NONE
    }
}
