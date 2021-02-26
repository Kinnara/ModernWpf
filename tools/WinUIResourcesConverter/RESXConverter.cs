using System.Collections;
using System.ComponentModel.Design;
using System.IO;
using System.Resources;

namespace WinUIResourcesConverter
{
    internal class RESXConverter
    {
        internal static bool TryConvertReswToResx(ResourcesFile resourcesFile, string sourceDirectory, string destinationDirectory)
        {
            var reswFile = @$"{sourceDirectory}\{resourcesFile.LanguageName}\{ResourcesFile.DefaultResourcesFileName}.resw";
            var resxFile = GetValidResxFileName(destinationDirectory, resourcesFile.LanguageName);

            if (File.Exists(reswFile))
            {
                return TryConvertReswToResxImpl(reswFile, resxFile);
            }

            return false;
        }

        internal static bool TryConvertReswToResxImpl(string reswFile, string resxFile)
        {
            ResXResourceReader resourceReader = new(reswFile) { UseResXDataNodes = true };
            ResXResourceWriter resourceWriter = new(resxFile);

            if (resourceReader != null)
            {
                foreach (DictionaryEntry entry in resourceReader)
                {
                    ResXDataNode readValue = entry.Value as ResXDataNode;

                    string value = readValue.GetValue((ITypeResolutionService)null).ToString();
                    ResXDataNode writeValue = new(readValue.Name, ConvertStringFormattingSpecifier(value))
                    {
                        Comment = ConvertStringFormattingSpecifier(readValue.Comment)
                    };

                    resourceWriter.AddResource(writeValue);
                }

                resourceReader.Close();

                resourceWriter.Generate();
                resourceWriter.Close();
                return true;
            }

            return false;
        }

        internal static string ConvertStringFormattingSpecifier(string value)
        {
            // These conversions are hard coded. After manually verifying all the RESW files from the WinUI codebase
            // there are no additional format specifiers other than these
            return value
                .Replace("%1!s!", "{0}")
                .Replace("%1!u!", "{0:d}")
                .Replace("%2!s!", "{1}")
                .Replace("%2!u!", "{1:d}")
                .Replace("%3!s!", "{2}")
                .Replace("%3!u!", "{2:d}")
                .Replace("%4!s!", "{3}")
                .Replace("%4!u!", "{3:d}")
                // extra steps to secure complete conversion
                .Replace("%1", "{0}")
                .Replace("%2", "{1}")
                .Replace("%3", "{2}")
                .Replace("%4", "{3}");
        }

        internal static string GetValidResxFileName(string destinationDirectory, string languageName)
        {
            return IsDefaultLanguage(languageName) ?
                @$"{destinationDirectory}\{ResourcesFile.DefaultResourcesFileName}.resx" :
                @$"{destinationDirectory}\{ResourcesFile.DefaultResourcesFileName}.{languageName}.resx";
        }

        internal static bool IsDefaultLanguage(string languageName)
        {
            return string.Equals(languageName, "en-us", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
