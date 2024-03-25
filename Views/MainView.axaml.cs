using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Encryptor.Views
{
    public partial class MainView : UserControl
    {
        public static Action<object?, DragEventArgs> OnDropJSON;
        public static Action<object?, DragEventArgs> OnDropFile;

        public MainView()
        {
            InitializeComponent();
            tb_DecryptedText_FILE.AddHandler(DragDrop.DropEvent, (sender, args) => OnDropJSON?.Invoke(sender, args));
            tb_Original_TEXT.AddHandler(DragDrop.DropEvent, (sender, args) => OnDropFile?.Invoke(sender, args));
        }
    }
}
