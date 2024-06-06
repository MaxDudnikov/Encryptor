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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        internal ReactiveCommand<Unit, Unit> OnClickBtnDecrypt_TEXT { get; }

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
                () => EncryptFILE());
            OnClickBtnEncrypt_TEXT = ReactiveCommand.Create(
                () => EncryptTEXT());
            OnClickBtnDecrypt_TEXT = ReactiveCommand.Create(
                () => DecryptTEXT());
        }

        #region Текст

        private string _original = string.Empty;
        public string Original_TEXT
        {
            get => _original;
            set
            {
                this.RaiseAndSetIfChanged(ref _original, value);
            }
        }

        private string _result_text = string.Empty;
        public string Result_TEXT
        {
            get => _result_text;
            set
            {
                this.RaiseAndSetIfChanged(ref _result_text, value);
            }
        }

        private void ReadAndEncryptTEXT(object? sender, DragEventArgs args)
        {
            var path = args.Data.GetFileNames()?.ToArray()[0];

            if (!File.Exists(path))
                return;

            Original_TEXT = File.ReadAllText(path);
        }

        private void EncryptTEXT()
        {
            Result_TEXT = encoder.GetDataEncrypt(Original_TEXT);
        }

        private void DecryptTEXT()
        {
            Result_TEXT = encoder.GetDataDecrypt(Original_TEXT) ?? Original_TEXT;
        }

        #endregion

        #region FILE

        private ObservableCollection<Settings> Settings { get; set; } = new();
        private Encoder encoder = new Encoder();
        private string backup = string.Empty;
        private eFileExtensions currentFileExtension = eFileExtensions.NONE;

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

        private string _OperationError = string.Empty;
        public string OperationError
        {
            get => _OperationError;
            set => this.RaiseAndSetIfChanged(ref _OperationError, value);
        }

        private string _path_JSON = string.Empty;
        public string Path_FILE
        {
            get => _path_JSON;
            set => this.RaiseAndSetIfChanged(ref _path_JSON, value);
        }

        private string _dt_file = string.Empty;
        public string DecryptedText_FILE
        {
            get => _dt_file;
            set
            {
                this.RaiseAndSetIfChanged(ref _dt_file, value);
                Reset();
            }
        }
        private string _et_file = string.Empty;
        public string EncryptedText_FILE
        {
            get => _et_file;
            set => this.RaiseAndSetIfChanged(ref _et_file, value);
        }

        private void Reset()
        {
            Settings.Clear();
            IsBtnEncryptEnabled = false;
        }

        #region Первая обработка файла

        private async void OpenFileButton_Click()
        {
            var window = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл для шифрования",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "JSON Files", Extensions = new List<string> { "json" } },
                    new FileDialogFilter { Name = "Ini Files", Extensions = new List<string> { "ini" } },
                    new FileDialogFilter { Name = "XML Files", Extensions = new List<string> { "xml" } },
                    new FileDialogFilter { Name = "Config Files", Extensions = new List<string> { "config" } },
                    new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } }
                }
            };
            var result = await openFileDialog.ShowAsync(window!.MainWindow!);

            if (result == null || result.Length == 0)
            {
                backup = string.Empty;
                Path_FILE = string.Empty;
                IsFileExists = false;
            }
            Path_FILE = result![0];

            ParseFile();
        }

        private void ReadAndEncryptJSON(object? sender, DragEventArgs args)
        {
            Path_FILE = args.Data.GetFileNames()!.ToArray()[0];

            if (!File.Exists(Path_FILE))
            {
                backup = string.Empty;
                Path_FILE = string.Empty;
                IsFileExists = false;
                return;
            }

            ParseFile();
        }

        private void ParseFile()
        {
            IsFileExists = true;
            string readText = File.ReadAllText(Path_FILE);
            backup = readText;

            var settings_temp = new List<Settings>();
            currentFileExtension = Path.GetExtension(Path_FILE).GetExtension();

            TryDeserialize(readText, settings_temp);
            EnterText(readText, settings_temp);
        }

        private void TryDeserialize(string readText, IList<Settings> settings_temp)
        {
            switch (currentFileExtension)
            {
                case eFileExtensions.JSON:
                    try
                    {
                        var res = JsonConvert.DeserializeObject<JObject>(readText);
                        var properties = res?.Properties();
                        if (properties == null)
                            return;

                        foreach (var property in properties)
                        {
                            settings_temp.Add(new Settings(property, encoder));
                            TryDeserialize(property.Value.ToString(), settings_temp);
                        }
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case eFileExtensions.INI:
                    using (var streamReader = new StringReader(readText))
                    {
                        string? line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(";") && !(line.StartsWith("[") && line.EndsWith("]")))
                            {
                                string[] parts = line.Split('=');
                                settings_temp.Add(new Settings(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : string.Empty, encoder));
                            }
                        }
                    }
                    break;
                case eFileExtensions.XML:
                    try
                    {
                        var xdoc = XDocument.Parse(readText);
                        foreach (var element in xdoc.Descendants("add"))
                        {
                            var key = element.Attribute("name")?.Value;
                            var value = element.Attribute("connectionString")?.Value;

                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            {
                                settings_temp.Add(new Settings(key, value, encoder));
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case eFileExtensions.NONE:
                    break;
                default:
                    break;
            }
        }

        private void EnterText(string readText, List<Settings> settings_temp)
        {
            switch (currentFileExtension)
            {
                case eFileExtensions.JSON:
                    foreach (var item in settings_temp)
                    {
                        var value = GetValue(item);
                        var json_old = $"\"{item.Name}\": {value}";
                        var json_new = $"\"{item.Name}\": {item.ValueDecrypted}";
                        readText = readText.Replace(json_old, json_new);
                    }
                    break;
                case eFileExtensions.INI:
                    foreach (var item in settings_temp)
                    {
                        var ini_old = $"{item.Name}={item.Value}";
                        var ini_new = $"{item.Name}={item.ValueDecrypted}";
                        readText = readText.Replace(ini_old, ini_new);
                    }
                    break;
                case eFileExtensions.XML:
                    foreach (var item in settings_temp)
                    {
                        var xml_old = $"name=\"{item.Name}\" connectionString=\"{item.Value}\"";
                        var xml_new = $"name=\"{item.Name}\" connectionString=\"{item.ValueDecrypted}\"";
                        readText = readText.Replace(xml_old, xml_new);
                    }
                    break;
                case eFileExtensions.NONE:
                    break;
                default:
                    break;
            }
            DecryptedText_FILE = readText;
        }

        #endregion

        #region Подготовка к шифрованию
        private void ParseAndGetBlocks()
        {
            Reset();
            TryDeserialize(DecryptedText_FILE, Settings);
            IsBtnEncryptEnabled = true;
        }

        #endregion

        #region Шифрование
        private void EncryptFILE()
        {
            var temp = DecryptedText_FILE;

            switch (currentFileExtension)
            {
                case eFileExtensions.JSON:
                    foreach (var item in Settings.Where(w => w.IsUse))
                    {
                        var value = GetValue(item);
                        var json_old = $"\"{item.Name}\": {item.ValueDecrypted}";
                        var json_new = $"\"{item.Name}\": {item.ValueEncrypted}";
                        temp = temp.Replace(json_old, json_new);
                    }
                    break;
                case eFileExtensions.INI:
                    foreach (var item in Settings.Where(w => w.IsUse))
                    {
                        var ini_old = $"{item.Name}={item.ValueDecrypted}";
                        var ini_new = $"{item.Name}={item.ValueEncrypted}";
                        temp = temp.Replace(ini_old, ini_new);
                    }
                    break;
                case eFileExtensions.XML:
                    foreach (var item in Settings.Where(w => w.IsUse))
                    {
                        var xml_old = $"name=\"{item.Name}\" connectionString=\"{item.ValueDecrypted}\"";
                        var xml_new = $"name=\"{item.Name}\" connectionString={item.ValueEncrypted}";
                        temp = temp.Replace(xml_old, xml_new);
                    }
                    break;
                case eFileExtensions.NONE:
                    break;
                default:
                    break;
            }
            EncryptedText_FILE = temp;
        }

        #endregion

        private async void BackupFile()
        {
            try
            {
                File.WriteAllText(Path_FILE, backup);
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
                File.WriteAllText(Path_FILE, EncryptedText_FILE);
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
