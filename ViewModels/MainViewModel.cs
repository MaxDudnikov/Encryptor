
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Encriptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private readonly string[] salt = { "pass", "word" };
        private readonly int key = 0x7B7;

        private string _tte;
        public string TextToEncrypt
        {
            get => _tte;
            set
            {
                _tte = value;
                if(_tte != null)
                {
                    EncryptedText = GetDataEncrypt(_tte);
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
                DecryptedText = GetDataDecrypt(_et);
            }
        }
        private string _dt;
        public string DecryptedText
        {
            get => _dt;
            set => this.RaiseAndSetIfChanged(ref _dt, value);
        }

        private string GetDataEncrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;


            byte[] encData_byte = Encoding.UTF8.GetBytes($"{salt[0]}{data}{salt[1]}");
            byte[] lowEncrypt = new byte[encData_byte.Length];

            for (int i = 0; i < encData_byte.Length; i++)
            {
                lowEncrypt[i] = (byte)(encData_byte[i] ^ key);
            }

            string encodedData = Convert.ToBase64String(lowEncrypt);
            return encodedData;
        }

        private string GetDataDecrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            UTF8Encoding encoder = new UTF8Encoding();
            Decoder utf8Decode = encoder.GetDecoder();

            var lowEncrypt = Convert.FromBase64String(data);
            var lengthOfData = lowEncrypt.Length;
            var todecode_byte = new byte[lengthOfData];

            for (int i = 0; i < lengthOfData; i++)
                todecode_byte[i] = (byte)(lowEncrypt[i] ^ key);

            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, lengthOfData);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, lengthOfData, decoded_char, 0);
            string result = new string(decoded_char);
            return result.Substring(salt[1].Length, result.Length - salt[0].Length - salt[1].Length);
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
