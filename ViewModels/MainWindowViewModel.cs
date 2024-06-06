using ReactiveUI;

namespace Encryptor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        internal MainWindowViewModel(){}
        private ViewModelBase content = new MainViewModel();
        internal ViewModelBase Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }
    }
}