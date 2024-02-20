using Avalonia.Input;
using Encryptor.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Encoder = EncoderLibrary.Encoder;

namespace Encryptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Settings> Settings { get; set; } = new();
        public new event PropertyChangedEventHandler? PropertyChanged;
        private Encoder encoder = new Encoder();
        private string filePath = string.Empty;
        private string backup = string.Empty;

        internal ReactiveCommand<Unit, Unit> OnBtnSaveClick { get; }
        internal ReactiveCommand<Unit, Unit> OnBtnBackupClick { get; }
        internal ReactiveCommand<Unit, Unit> OnBtnQuestionClick { get; }


        private bool _tbAnimateSuccess = false;
        public bool tbAnimateSuccess
        {
            get => _tbAnimateSuccess;
            set
            {
                this.RaiseAndSetIfChanged(ref _tbAnimateSuccess, value);
            }
        }

        private bool _tbAnimateError = false;
        public bool tbAnimateError
        {
            get => _tbAnimateError;
            set
            {
                this.RaiseAndSetIfChanged(ref _tbAnimateError, value);
            }
        }

        private string _OperationError;
        public string OperationError
        {
            get => _OperationError;
            set
            {
                this.RaiseAndSetIfChanged(ref _OperationError, value);
            }
        }

        private string _tte;
        public string TextToEncrypt
        {
            get => _tte;
            set
            {
                this.RaiseAndSetIfChanged(ref _tte, value);
                ReformatString(TextToEncrypt);
            }
        }

        private bool isBlocking;
        public bool IsBlocking
        {
            get => isBlocking;
            set
            {
                this.RaiseAndSetIfChanged(ref isBlocking, value);
                ReformatString(TextToEncrypt);
            }
        }

        private bool isShielding;
        public bool IsShielding
        {
            get => isShielding;
            set
            {
                this.RaiseAndSetIfChanged(ref isShielding, value);
                ReformatString(TextToEncrypt);
            }
        }

        private string _et;
        public string EncryptedText
        {
            get => _et;
            set
            {
                this.RaiseAndSetIfChanged(ref _et, value);
                DecryptText(_et);
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
            //Views.MainView.OnDropToDecrypt += ReadAndDecryptText;
            OnBtnSaveClick = ReactiveCommand.Create(
                () => SaveFile());
            OnBtnBackupClick = ReactiveCommand.Create(
                () => BackupFile());
            OnBtnQuestionClick = ReactiveCommand.Create(
                () => OpenPDF());
        }

        private void OpenPDF()
        {
            var path = string.Empty;
#if DEBUG
            var catalog = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(
                        Directory.GetCurrentDirectory())!.ToString())!.ToString())!.ToString();

            path = Path.Combine(catalog, "EncryptorReference.pdf");
#else
            path = Path.Combine(Directory.GetCurrentDirectory(), "EncryptorReference.pdf");
#endif
            ProcessStartInfo startInfo = new ProcessStartInfo(path) { UseShellExecute = true };
            Process.Start(startInfo);
        }

        private async void BackupFile()
        {
            try
            {
                File.WriteAllText(filePath, backup);
                tbAnimateSuccess = true;
                await Task.Delay(2000);
                tbAnimateSuccess = false;
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                tbAnimateError = true;
                await Task.Delay(2000);
                tbAnimateError = false;
            }
        }

        private async void SaveFile()
        {
            try
            {
                File.WriteAllText(filePath, EncryptedText);
                tbAnimateSuccess = true;
                await Task.Delay(2000);
                tbAnimateSuccess = false;
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                tbAnimateError = true;
                await Task.Delay(2000);
                tbAnimateError = false;
            }
        }

        private void ReadAndEncryptText(object? sender, DragEventArgs args)
        {
            backup = string.Empty;
            filePath = string.Empty;

            filePath = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(filePath))
                return;

            string readText = File.ReadAllText(filePath);
            backup = readText;
            ParseText(readText);
            TextToEncrypt = readText;
        }

        private void ParseText(string readText)
        {
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
                    var setting = new Settings(item, encoder);
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
            ReformatString(TextToEncrypt);
        }

        private void ReadAndDecryptText(object? sender, DragEventArgs args)
        {
            string path = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(path))
                return;

            string readText = File.ReadAllText(path);
            EncryptedText = readText;
        }

        private void ReformatString(string textToEncrypt)
        {
            string result = string.Empty;

            if (IsShielding)
            {
                textToEncrypt = textToEncrypt.Replace("\\\'", "\'");
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
                result = textToEncrypt;
            }

            if (IsBlocking)
            {
                foreach (var item in Settings.Where(w => w.IsUse))
                {
                    var value = GetValue(item);
                    var json_old = $"\"{item.Name}\": {value}";
                    var json_new = $"\"{item.Name}\": {item.ValueEncrypted}";

                    result = result.Replace(json_old, json_new);
                }
                EncryptedText = result;
            }
            else
            {
                EncryptedText = encoder.GetDataEncrypt(result);
            }
        }

        private void DecryptText(string et)
        {
            if (IsBlocking)
            {
                foreach (var item in Settings.Where(w => w.IsUse))
                {
                    var value = GetValue(item);
                    var json_old = $"\"{item.Name}\": {item.ValueEncrypted}";
                    var json_new = $"\"{item.Name}\": {value}";

                    et = et.Replace(json_old, json_new);
                }
                DecryptedText = et;
            }
            if (!IsBlocking)
            {
                DecryptedText = encoder.GetDataDecrypt(et);
            }
        }

        private string GetValue(Settings value)
        {
            string result = string.Empty;
            switch (value.TypeValue)
            {
                case JsonValueKind.Undefined:
                    result = value.Value.ToString();
                    break;
                case JsonValueKind.Object:
                    result = value.Value.ToString();
                    break;
                case JsonValueKind.Array:
                    result = value.Value.ToString();
                    break;
                case JsonValueKind.String:
                    result = $"\"{value.Value}\"";
                    break;
                case JsonValueKind.Number:
                    result = value.Value.ToString();
                    break;
                case JsonValueKind.True:
                    result = value.Value.ToString().ToLower();
                    break;
                case JsonValueKind.False:
                    result = value.Value.ToString().ToLower();
                    break;
                case JsonValueKind.Null:
                    result = value.Value.ToString();
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
