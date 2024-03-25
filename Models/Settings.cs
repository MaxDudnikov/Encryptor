using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Encryptor.Models
{
    internal class Settings
    {
        internal string Name { get; set; }
        internal string Value { get; set; }
        internal string ValueDecrypted { get; set; }
        internal string ValueEncrypted { get; set; }
        internal JTokenType JTypeValue { get; set; }
        internal bool IsUse { get; set; }

        internal Settings(JProperty jProperty, EncoderLibrary.Encoder encoder)
        {
            Name = jProperty.Name;
            JTypeValue = jProperty.Value.Type;
            IsUse = false;

            switch (JTypeValue)
            {
                case JTokenType.None:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Object:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = (encoder.GetDataDecrypt(Value) ?? Value).Replace("\r\n  ", "\r\n    ").Replace("\r\n}", "\r\n  }");
                    ValueEncrypted = $"\"{(Value.Replace("\r\n  ", "\r\n    ").Replace("\r\n}", "\r\n  }") == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Array:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Constructor:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Property:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Integer:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Float:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.String:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = ($"\"{encoder.GetDataDecrypt(Value) ?? Value}\"").Replace("\\", @"\\");
                    ValueEncrypted = $"\"{(($"\"{Value}\"").Replace("\\", @"\\") == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Boolean:
                    Value = jProperty.Value.ToString().ToLower();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Null:
                    Value = "null";
                    ValueDecrypted = "null";
                    ValueEncrypted = "null";
                    break;
                case JTokenType.Undefined:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Date:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Raw:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Bytes:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Guid:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.Uri:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                case JTokenType.TimeSpan:
                    Value = jProperty.Value.ToString();
                    ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
                    ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
                    break;
                default:
                    break;
            }
        }

        internal Settings(string key, string value, EncoderLibrary.Encoder encoder)
        {
            Name = key;
            Value = value;
            ValueDecrypted = encoder.GetDataDecrypt(Value) ?? Value;
            ValueEncrypted = $"\"{(Value == ValueDecrypted ? encoder.GetDataEncrypt(Value) : Value)}\"";
            IsUse = false;
        }
    }
}
