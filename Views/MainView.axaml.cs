using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Encryptor.Views
{
    public partial class MainView : UserControl
    {
        public static Action<object?, DragEventArgs> OnDropToEncrypt;
        public static Action<object?, DragEventArgs> OnDropToDecrypt;

        public MainView()
        {
            InitializeComponent();
            tb_ToEncrypt.AddHandler(DragDrop.DropEvent, (sender, args) => OnDropToEncrypt?.Invoke(sender, args));
            //tb_ToDecrypt.AddHandler(DragDrop.DropEvent, (sender, args) => OnDropToDecrypt?.Invoke(sender, args));
        }
    }
}
