using System.ComponentModel;

namespace Encryptor.Models
{
    internal class Settings : INotifyPropertyChanged
    {
        internal string Name { get; set; }
        internal string Value { get; set; }
        private bool isUse;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal bool IsUse
        {
            get => isUse;
            set
            {
                isUse = value;
                OnPropertyChanged(Name);
            }
        }
        internal Settings(string name, string value)
        {
            Name = name;
            Value = value;
            IsUse = false;
        }
    }
}
