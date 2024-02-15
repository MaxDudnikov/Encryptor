namespace Encryptor.Models
{
    internal class Settings
    {
        internal string Name { get; set; }
        internal bool IsUse { get; set; }
        internal TypeSettings TypeSettings { get; set; }

        internal Settings(string name, TypeSettings typeSettings)
        {
            Name = name;
            IsUse = false;
            TypeSettings = typeSettings;
        }
    }
    internal enum TypeSettings
    {
        Block,
        String
    }
}
