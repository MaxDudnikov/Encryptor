using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace Encryptor.Models
{
    internal class Settings : INotifyPropertyChanged
    {
        internal string Name { get; set; }
        internal string Value { get; set; }
        internal string ValueEncrypted { get; set; }
        internal JsonValueKind TypeValue { get; set; }
        private bool isUse;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal bool IsUse
        {
            get => isUse;
            set
            {
                isUse = value;
                OnPropertyChanged(Name);
            }
        }

        internal Settings(KeyValuePair<string, object> keyValuePair, EncoderLibrary.Encoder encoder)
        {
            Name = keyValuePair.Key;
            Value = keyValuePair.Value.ToString();
            ValueEncrypted = $"\"{encoder.GetDataEncrypt(Value)}\"";
            isUse = false;
            TypeValue = ((JsonElement)keyValuePair.Value).ValueKind;
        }
    }
}
