using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Encoder = EncoderLibrary.Encoder;

namespace Encriptor.ViewModels
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
                _tte = value;
                if (_tte != null)
                {
                    EncryptedText = encoder.GetDataEncrypt(_tte);
                    this.RaiseAndSetIfChanged(ref _tte, value);
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
        }
    }
}
