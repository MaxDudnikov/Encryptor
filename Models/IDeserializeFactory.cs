using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using EncoderLibrary;
using Encryptor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Encryptor;

public interface IDeserializeFactory
{
    public void Handle(IList<Settings> settings, string readText, Encoder encoder);
}

public static class DeserializeFactory
{
    public static IDeserializeFactory GetHandler(EFileExtensions fileExtension)
    {
        return fileExtension switch
        {
            EFileExtensions.JSON => new JSON_Deserialize(),
            EFileExtensions.INI => new INI_Deserialize(),
            EFileExtensions.CONFIG => new CONFIG_Deserialize(),
            _ => throw new NotSupportedException("File extension not supported"),
        };
    }
}

public class JSON_Deserialize : IDeserializeFactory
{
    public void Handle(IList<Settings> settings, string readText, Encoder encoder)
    {
        try
        {
            var res = JsonConvert.DeserializeObject<JObject>(readText);
            var properties = res?.Properties();
            if (properties == null)
                return;

            foreach (var property in properties)
            {
                settings.Add(new Settings(property, encoder));
                Handle(settings, property.Value.ToString(), encoder);
            }
        }
        catch
        {
            return;
        }
    }
}

public class INI_Deserialize : IDeserializeFactory
{
    public void Handle(IList<Settings> settings, string readText, Encoder encoder)
    {
        using (var streamReader = new StringReader(readText))
        {
            string? line;
            while ((line = streamReader.ReadLine()) != null)
            {
                line = line.Trim();
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(";") && !(line.StartsWith("[") && line.EndsWith("]")))
                {
                    string[] parts = line.Split('=');
                    settings.Add(new Settings(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : string.Empty, encoder, "\"{0}\""));
                }
            }
        }
    }
}

public class CONFIG_Deserialize : IDeserializeFactory
{
    public void Handle(IList<Settings> settings, string readText, Encoder encoder)
    {
        try
        {
            var xdoc = XDocument.Parse(readText);
            foreach (var element in xdoc.Descendants("add"))
            {
                var key = element.Attribute("name")?.Value;
                var value = element.Attribute("connectionString")?.Value;

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    settings.Add(new Settings(key, value, encoder));
                }
            }
        }
        catch
        {
            return;
        }
    }
}