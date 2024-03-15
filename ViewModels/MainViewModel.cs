using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Encryptor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Encoder = EncoderLibrary.Encoder;

namespace Encryptor.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        internal ReactiveCommand<Unit, Unit> OnBtnSaveClick { get; }
        internal ReactiveCommand<Unit, Unit> OnBtnBackupClick { get; }
        internal ReactiveCommand<Unit, Unit> OnBtnQuestionClick { get; }
        internal ReactiveCommand<Unit, Unit> OnClickBtnFindFile { get; }
        internal ReactiveCommand<Unit, Unit> OnClickBtnPrepare { get; }
        internal ReactiveCommand<Unit, Unit> OnClickBtnEncrypt { get; }
        internal ReactiveCommand<Unit, Unit> OnClickBtnEncrypt_TEXT { get; }

        public MainViewModel()
        {
            Views.MainView.OnDropJSON += ReadAndEncryptJSON;
            Views.MainView.OnDropFile += ReadAndEncryptTEXT;
            OnBtnSaveClick = ReactiveCommand.Create(
                () => SaveFile());
            OnBtnBackupClick = ReactiveCommand.Create(
                () => BackupFile());
            OnBtnQuestionClick = ReactiveCommand.Create(
                () => OpenPDF());
            OnClickBtnFindFile = ReactiveCommand.Create(
                () => OpenFileButton_Click());
            OnClickBtnPrepare = ReactiveCommand.Create(
                () => ParseAndGetBlocks());
            OnClickBtnEncrypt = ReactiveCommand.Create(
                () => EncryptJSON());
            OnClickBtnEncrypt_TEXT = ReactiveCommand.Create(
                () => EncryptTEXT());
        }

        #region Текст

        private string _original;
        public string Original_TEXT
        {
            get => _original;
            set
            {
                this.RaiseAndSetIfChanged(ref _original, value);
            }
        }

        private string _et_text;
        public string EncryptedText_TEXT
        {
            get => _et_text;
            set
            {
                this.RaiseAndSetIfChanged(ref _et_text, value);
            }
        }

        private void ReadAndEncryptTEXT(object? sender, DragEventArgs args)
        {
            var path = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(path))
                return;

            Original_TEXT = File.ReadAllText(path);
        }

        private void EncryptTEXT()
        {
            EncryptedText_TEXT = encoder.GetDataEncrypt(Original_TEXT);
        }

        #endregion

        #region JSON

        private ObservableCollection<Settings> Settings { get; set; } = new();
        public new event PropertyChangedEventHandler? PropertyChanged;
        private Encoder encoder = new Encoder();
        private string backup = string.Empty;

        private bool _isBtnEncryptEnabled = false;
        public bool IsBtnEncryptEnabled
        {
            get => _isBtnEncryptEnabled;
            set => this.RaiseAndSetIfChanged(ref _isBtnEncryptEnabled, value);
        }

        private bool _isFileExists = false;
        public bool IsFileExists
        {
            get => _isFileExists;
            set => this.RaiseAndSetIfChanged(ref _isFileExists, value);
        }

        private bool _tbAnimateSuccess = false;
        public bool tbAnimateSuccess
        {
            get => _tbAnimateSuccess;
            set => this.RaiseAndSetIfChanged(ref _tbAnimateSuccess, value);
        }

        private bool _tbAnimateError = false;
        public bool tbAnimateError
        {
            get => _tbAnimateError;
            set => this.RaiseAndSetIfChanged(ref _tbAnimateError, value);
        }

        private string _OperationError;
        public string OperationError
        {
            get => _OperationError;
            set => this.RaiseAndSetIfChanged(ref _OperationError, value);
        }

        private string _path_JSON = string.Empty;
        public string Path_JSON
        {
            get => _path_JSON;
            set => this.RaiseAndSetIfChanged(ref _path_JSON, value);
        }

        private string _dt_json;
        public string DecryptedText_JSON
        {
            get => _dt_json;
            set
            {
                this.RaiseAndSetIfChanged(ref _dt_json, value);
                Reset();
            }
        }
        private string _et_json;
        public string EncryptedText_JSON
        {
            get => _et_json;
            set => this.RaiseAndSetIfChanged(ref _et_json, value);
        }

        private void Reset()
        {
            Settings.Clear();
            IsBtnEncryptEnabled = false;
        }

        #region Подготовка к шифрованию
        private void ParseAndGetBlocks()
        {
            Reset();
            TryDeserialize(DecryptedText_JSON);
            IsBtnEncryptEnabled = true;
        }

        private void TryDeserialize(string value)
        {
            try
            {
                var res = JsonConvert.DeserializeObject<JObject>(value);
                var properties = res?.Properties();
                if (properties == null)
                    return;
                foreach (var property in properties)
                {
                    var setting = new Settings(property, encoder);
                    Settings.Add(setting);
                    TryDeserialize(property.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #endregion

        #region Шифрование
        private void EncryptJSON()
        {
            var temp = DecryptedText_JSON;

            foreach (var item in Settings.Where(w => w.IsUse))
            {
                var value = GetValue(item);
                var json_old = $"\"{item.Name}\": {item.ValueDecrypted}";
                var json_new = $"\"{item.Name}\": {item.ValueEncrypted}";
                temp = temp.Replace(json_old, json_new);
            }

            EncryptedText_JSON = temp;
        }

        #endregion

        private async void BackupFile()
        {
            try
            {
                File.WriteAllText(Path_JSON, backup);
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
                File.WriteAllText(Path_JSON, EncryptedText_JSON);
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

        #region Первая обработка файла

        private async void OpenFileButton_Click()
        {
            backup = string.Empty;
            Path_JSON = string.Empty;
            IsFileExists = false;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл для шифрования",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "JSON Files", Extensions = new List<string> { "json" } }
                }
            };
            var window = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var result = await openFileDialog.ShowAsync(window.MainWindow);
            if (result != null && result.Count() > 0)
            {
                Path_JSON = result[0];
                IsFileExists = true;
                string readText = File.ReadAllText(Path_JSON);
                backup = readText;

                var settings_temp = new List<Settings>();
                DecryptText(readText, settings_temp);
                EnterText(readText, settings_temp);
            }
        }

        private void ReadAndEncryptJSON(object? sender, DragEventArgs args)
        {
            backup = string.Empty;
            Path_JSON = string.Empty;
            IsFileExists = false;

            Path_JSON = args.Data.GetFileNames().ToArray()[0];

            if (!File.Exists(Path_JSON))
                return;

            IsFileExists = true;
            string readText = File.ReadAllText(Path_JSON);
            backup = readText;

            var settings_temp = new List<Settings>();
            DecryptText(readText, settings_temp);
            EnterText(readText, settings_temp);
        }

        private void DecryptText(string readText, List<Settings> settings_temp)
        {
            try
            {
                var res = JsonConvert.DeserializeObject<JObject>(readText);
                var properties = res?.Properties();
                if (properties == null)
                    return;

                foreach (var property in properties)
                {
                    settings_temp.Add(new Settings(property, encoder));
                    DecryptText(property.Value.ToString(), settings_temp);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void EnterText(string readText, List<Settings> settings_temp)
        {
            foreach (var item in settings_temp)
            {
                var value = GetValue(item);
                var json_old = $"\"{item.Name}\": {value}";
                var json_new = $"\"{item.Name}\": {item.ValueDecrypted}";

                readText = readText.Replace(json_old, json_new);
            }
            DecryptedText_JSON = readText;
        }

        #endregion

        private string GetValue(Settings value)
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
        #endregion

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
    }
}
