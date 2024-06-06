using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Encryptor.Views
{
    public partial class MainView : UserControl
    {
        public static Action<object?, DragEventArgs>? OnDrop_FileTab;
        public static Action<object?, DragEventArgs>? OnDropFile_TextTab;

        public MainView()
        {
            InitializeComponent();
            tb_DecryptedText_FILE.AddHandler(DragDrop.DropEvent, (sender, args) => OnDrop_FileTab?.Invoke(sender, args));
            tb_Original_TEXT.AddHandler(DragDrop.DropEvent, (sender, args) => OnDropFile_TextTab?.Invoke(sender, args));
        }
    }
}
