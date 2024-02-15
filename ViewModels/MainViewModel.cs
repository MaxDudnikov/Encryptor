using Avalonia;
using Avalonia.Input;
using Encryptor.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Encoder = EncoderLibrary.Encoder;

namespace Encryptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private Encoder encoder = new Encoder();

        private string _notFormattingString = string.Empty;

        private ObservableCollection<Settings> Settings { get; set; } = new();

        private Regex regexString = new Regex(@"""(.*)"": (""|\w|\d)");
        private Regex regexBlock = new Regex(@"""(\w*)"": {");

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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            MatchCollection matchesString = regexString.Matches(readText);
            MatchCollection matchesBlock = regexBlock.Matches(readText);
            foreach (Match match in matchesString)
            {
                Settings.Add(new Settings(match.Value.Split("\"")[1], TypeSettings.String));
            }
            foreach (Match match in matchesBlock)
            {
                Settings.Add(new Settings(match.Value.Split("\"")[1], TypeSettings.Block));
            }
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
                var filter = Settings.Where(w => w.IsUse == true);
                Regex tempRegex;

                foreach (var filterItem in filter)
                {
                    tempRegex = new Regex(filterItem.Name);
                }
            }
            if (!IsBlocking)
            {
                EncryptedText = encoder.GetDataEncrypt(result);
            }
        }
    }
}
