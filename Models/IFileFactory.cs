using System;
using System.Collections.Generic;
using System.Linq;
using Encryptor.Models;
namespace Encryptor;

public interface IFileFactory
{
    public void Handle_Decrypt(IList<Settings> settings, ref string readText);
    public void Handle_Encrypt(IList<Settings> settings, ref string readText);
}

public static class FileHandlerFactory
{
    public static IFileFactory GetHandler(EFileExtensions fileExtension)
    {
        return fileExtension switch
        {
            EFileExtensions.JSON => new Json_FileHandler(),
            EFileExtensions.INI => new Ini_FileHandler(),
            EFileExtensions.CONFIG => new Config_FileHandler(),
            _ => throw new NotSupportedException("File extension not supported"),
        };
    }
}

public class Json_FileHandler : FileHandler, IFileFactory
{
    private string _format = "\"{0}\": {1}";

    public void Handle_Decrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            Replace(ref readText, _format, item.Name, item.GetValue(), item.ValueDecrypted);
        }
    }

    public void Handle_Encrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings.Where(w => w.IsUse))
        {
            Replace(ref readText, _format, item.Name, item.ValueDecrypted, item.ValueEncrypted);
        }
    }
}

public class Ini_FileHandler : FileHandler, IFileFactory
{
    private string _format = "{0}={1}";

    public void Handle_Decrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            Replace(ref readText, _format, item.Name, item.Value, item.ValueDecrypted);
        }
    }

    public void Handle_Encrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings.Where(w => w.IsUse))
        {
            Replace(ref readText, _format, item.Name, item.ValueDecrypted, item.ValueEncrypted);
        }
    }
}

public class Config_FileHandler : FileHandler, IFileFactory
{
    private string _format = "name=\"{0}\" connectionString=\"{1}\"";

    public void Handle_Decrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            Replace(ref readText, _format, item.Name, item.Value, item.ValueDecrypted);
        }
    }

    public void Handle_Encrypt(IList<Settings> settings, ref string readText)
    {
        foreach (var item in settings.Where(w => w.IsUse))
        {
            Replace(ref readText, _format, item.Name, item.ValueDecrypted, item.ValueEncrypted);
        }
    }
}

public class FileHandler
{
    protected void Replace(ref string readText, string format, string name, string oldValue, string newValue)
    {
        var old_str = string.Format(format, name, oldValue);
        var new_str = string.Format(format, name, newValue);
        readText = readText.Replace(old_str, new_str);
    }
}