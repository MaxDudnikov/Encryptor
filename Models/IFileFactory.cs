using System;
using System.Collections.Generic;
using Encryptor.Models;
namespace Encryptor;

public interface IFileFactory
{
    public void Handle(List<Settings> settings, ref string readText);
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

public class Json_FileHandler : IFileFactory
{
    public void Handle(List<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            var value = item.GetValue();
            var json_old = $"\"{item.Name}\": {value}";
            var json_new = $"\"{item.Name}\": {item.ValueDecrypted}";
            readText = readText.Replace(json_old, json_new);
        }
    }
}

public class Ini_FileHandler : IFileFactory
{
    public void Handle(List<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            var ini_old = $"{item.Name}={item.Value}";
            var ini_new = $"{item.Name}={item.ValueDecrypted}";
            readText = readText.Replace(ini_old, ini_new);
        }
    }
}

public class Config_FileHandler : IFileFactory
{
    public void Handle(List<Settings> settings, ref string readText)
    {
        foreach (var item in settings)
        {
            var xml_old = $"name=\"{item.Name}\" connectionString=\"{item.Value}\"";
            var xml_new = $"name=\"{item.Name}\" connectionString=\"{item.ValueDecrypted}\"";
            readText = readText.Replace(xml_old, xml_new);
        }
    }
}