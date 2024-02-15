using Avalonia;
using Avalonia.Input;
using Encryptor.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Encoder = EncoderLibrary.Encoder;

namespace Encryptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private Encoder encoder = new Encoder();

        private string _notFormattingString = string.Empty;

        private Dictionary<string, string> JsonValues = new();

        private ObservableCollection<Settings> Settings { get; set; } = new();

        private string _tte;
        public string TextToEncrypt
        {
            get => _tte;
            set
            {
                this.RaiseAndSetIfChanged(ref _tte, value);
                ReformatString(TextToEncrypt, _notFormattingString);
            }
        }
        
        private bool isBlocking;
        public bool IsBlocking
        {
            get => isBlocking;
            set
            {
                this.RaiseAndSetIfChanged(ref isBlocking, value);
                ReformatString(TextToEncrypt, _notFormattingString);
            }
        }

        private bool isShielding;
        public bool IsShielding
        {
            get => isShielding;
            set
            {
                this.RaiseAndSetIfChanged(ref isShielding, value);
                ReformatString(TextToEncrypt, _notFormattingString);
            }
        }

        private string _et;
        public string EncryptedText
        {
            get => _et;
            set
            {
                this.RaiseAndSetIfChanged(ref _et, value);
                DecryptedText = encoder.GetDataDecrypt(_et);
            }
        }
        private string _dt;
        public string DecryptedText
        {
            get => _dt;
            set => this.RaiseAndSetIfChanged(ref _dt, value);
        }

        public MainViewModel()
        {
            TextToEncrypt = string.Empty;
            Views.MainView.OnDropToEncrypt += ReadAndEncryptText;
            Views.MainView.OnDropToDecrypt += ReadAndDecryptText;
        }

        private void ReadAndEncryptText(object? sender, DragEventArgs args)
        {
            string path = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(path))
                return;

            string readText = File.ReadAllText(path);
            ParseText(readText);
            TextToEncrypt = readText;
        }

        private void ParseText(string readText)
        {
            JsonValues.Clear();
            foreach (var item in Settings)
            {
                item.PropertyChanged -= Setting_PropertyChanged;
            }
            Settings.Clear();
            TryDeserialize(readText);
        }

        private void TryDeserialize(string value)
        {
            try
            {
                var res = JsonSerializer.Deserialize<Dictionary<string, object>>(value);
                foreach (var item in res)
                {
                    JsonValues.Add(item.Key, item.Value.ToString());
                    var setting = new Settings(item.Key, item.Value.ToString());
                    setting.PropertyChanged += Setting_PropertyChanged;
                    Settings.Add(setting);
                    TryDeserialize(item.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void Setting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ReformatString(TextToEncrypt, _notFormattingString);
        }

        private void ReadAndDecryptText(object? sender, DragEventArgs args)
        {
            string path = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(path))
                return;

            string readText = File.ReadAllText(path);
            EncryptedText = readText;
        }

        private void ReformatString(string textToEncrypt, string notFormattingString)
        {
            string result = string.Empty;

            if (IsShielding)
            {
                _notFormattingString = textToEncrypt;
                textToEncrypt = textToEncrypt.Replace("\\\'", "\'");
                textToEncrypt = textToEncrypt.Replace("\\\"", "\"");
                textToEncrypt = textToEncrypt.Replace("\\\0", "\0");
                textToEncrypt = textToEncrypt.Replace("\\\a", "\a");
                textToEncrypt = textToEncrypt.Replace("\\\b", "\b");
                textToEncrypt = textToEncrypt.Replace("\\\f", "\f");
                textToEncrypt = textToEncrypt.Replace("\\\n", "\n");
                textToEncrypt = textToEncrypt.Replace("\\\r", "\r");
                textToEncrypt = textToEncrypt.Replace("\\\t", "\t");
                textToEncrypt = textToEncrypt.Replace("\\\v", "\v");
                textToEncrypt = textToEncrypt.Replace("\\\\", "\\");

                result = textToEncrypt;
            }
            else
            {
                result = string.IsNullOrEmpty(notFormattingString) ? textToEncrypt : notFormattingString;
            }

            if (IsBlocking)
            {
                foreach (var item in Settings.Where(w => w.IsUse))
                {
                    var value = item.Value.StartsWith('{') ? item.Value : $"\"{item.Value}\"";
                    var json_old = $"\"{item.Name}\": {value}";
                    var json_new = $"\"{item.Name}\": \"{encoder.GetDataEncrypt(item.Value)}\"";

                    result = result.Replace(json_old, json_new);
                }
                EncryptedText = result;
            }
            if (!IsBlocking)
            {
                EncryptedText = encoder.GetDataEncrypt(result);
            }
        }
    }
}
