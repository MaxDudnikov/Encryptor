using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
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
            Views.MainView.OnDrop_FileTab += GetInfoFromDropedFile;
            Views.MainView.OnDropFile_TextTab += ReadAndEncryptTEXT;
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
        private readonly Encoder encoder = new();
        private string backup = string.Empty;
        private EFileExtensions currentFileExtension = EFileExtensions.NONE;

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
        public bool TbAnimateSuccess
        {
            get => _tbAnimateSuccess;
            set => this.RaiseAndSetIfChanged(ref _tbAnimateSuccess, value);
        }

        private bool _tbAnimateError = false;
        public bool TbAnimateError
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
            EncryptedText_FILE = string.Empty;
        }

        #region Первая обработка файла

        private async void OpenFileButton_Click()
        {
            var window = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
            var storageProvider = window.MainWindow!.StorageProvider;
            var fileOpenPicker = await storageProvider.OpenFilePickerAsync(FileExtensions.filePickerOpenOptions);

            if (fileOpenPicker == null || fileOpenPicker.Count == 0 || !fileOpenPicker[0].TryGetUri(out var uri))
            {
                backup = string.Empty;
                Path_FILE = string.Empty;
                IsFileExists = false;
                return;
            }

            Path_FILE = uri.LocalPath;
            ParseFile();
        }

        private void GetInfoFromDropedFile(object? sender, DragEventArgs args)
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
            FillDecryptedTextBox(readText, settings_temp);
        }

        private void TryDeserialize(string readText, IList<Settings> settings_temp)
        {
            var handler = DeserializeFactory.GetHandler(currentFileExtension);
            handler.Handle(settings_temp, readText, encoder);
        }

        private void FillDecryptedTextBox(string readText, List<Settings> settings_temp)
        {
            var handler = FileHandlerFactory.GetHandler(currentFileExtension);
            handler.Handle_Decrypt(settings_temp, ref readText);
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
            var handler = FileHandlerFactory.GetHandler(currentFileExtension);
            handler.Handle_Encrypt(Settings, ref temp);
            EncryptedText_FILE = temp;
        }

        #endregion

        private void BackupFile()
        {
            ReWriteFile(backup);
        }

        private void SaveFile()
        {
            ReWriteFile(EncryptedText_FILE);
        }

        private async void ReWriteFile(string textFile)
        {
            try
            {
                File.WriteAllText(Path_FILE, textFile);
                TbAnimateSuccess = true;
                await Task.Delay(2000);
                TbAnimateSuccess = false;
            }
            catch (Exception ex)
            {
                OperationError = ex.ToString();
                TbAnimateError = true;
                await Task.Delay(2000);
                TbAnimateError = false;
            }
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
            ProcessStartInfo startInfo = new(path) { UseShellExecute = true };
            Process.Start(startInfo);
        }
    }
}
