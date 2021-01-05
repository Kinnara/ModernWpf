using System.Text;

namespace WinUIResourcesConverter
{
    internal class CodeGen
    {
        public CodeGen(string controlName, string relativePath)
        {
            ControlName = controlName;
            RelativePath = relativePath;
        }

        public StringBuilder StringBuilder { get; } = new();

        public string ControlName { get; }

        public string RelativePath { get; }

        public void AppendFirstPart()
        {
            StringBuilder.AppendLine("...");
            StringBuilder.AppendLine(@$"private const string {ControlName}ResourcesName = ""{RelativePath}{ControlName}.Strings.Resources"";");
            StringBuilder.AppendLine("...");
            StringBuilder.AppendLine();
            StringBuilder.AppendLine($"// {ControlName} resources");
        }

        public void AppendResourceMap(string resName)
        {
            StringBuilder.Append("{ SR_");
            StringBuilder.AppendFormat("{0}, {1}ResourcesName", resName, ControlName);
            StringBuilder.Append(" },");
            StringBuilder.AppendLine();
        }

        public string GeneratedCode => StringBuilder.ToString();
    }
}
