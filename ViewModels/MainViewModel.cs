using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Encoder = EncoderLibrary.Encoder;

namespace Encryptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private Encoder encoder = new Encoder();

        private string _notFormattingString = string.Empty;
        private string _tte;
        public string TextToEncrypt
        {
            get => _tte;
            set
            {
                this.RaiseAndSetIfChanged(ref _tte, value);
                ReformatString(TextToEncrypt, _notFormattingString);
                //EncryptedText = encoder.GetDataEncrypt(_tte);
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
            TextToEncrypt = readText;
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
                textToEncrypt = textToEncrypt.Replace(@"\'", "\'");
                textToEncrypt = textToEncrypt.Replace("\\\"", "\"");
                textToEncrypt = textToEncrypt.Replace(@"\0", "\0");
                textToEncrypt = textToEncrypt.Replace(@"\a", "\a");
                textToEncrypt = textToEncrypt.Replace(@"\b", "\b");
                textToEncrypt = textToEncrypt.Replace(@"\f", "\f");
                textToEncrypt = textToEncrypt.Replace(@"\n", "\n");
                textToEncrypt = textToEncrypt.Replace(@"\r", "\r");
                textToEncrypt = textToEncrypt.Replace(@"\t", "\t");
                textToEncrypt = textToEncrypt.Replace(@"\v", "\v");
                textToEncrypt = textToEncrypt.Replace(@"\\", "\\");

                result = textToEncrypt;
            }
            else
            {
                result = string.IsNullOrEmpty(notFormattingString) ? textToEncrypt : notFormattingString;
            }

            EncryptedText = encoder.GetDataEncrypt(result);
        }
    }
}
