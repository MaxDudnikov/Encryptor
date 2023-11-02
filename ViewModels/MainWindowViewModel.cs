using ReactiveUI;
using System.IO;
using System;

namespace Encriptor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        internal MainWindowViewModel()
        {
            Content = new MainViewModel();
        }

        private ViewModelBase content;
        internal ViewModelBase Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }
    }
}