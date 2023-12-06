using Avalonia.Input;
using ReactiveUI;
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
        private string _tte;
        public string TextToEncrypt
        {
            get => _tte;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref _tte, value);
                    EncryptedText = encoder.GetDataEncrypt(_tte);
                }
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
    }
}
