using System.Text;

namespace WinUIResourcesConverter
{
    internal class CodeGen
    {
        public CodeGen(string controlName)
        {
            ControlName = controlName;
        }

        public StringBuilder StringBuilder { get; } = new();

        public string ControlName { get; }

        public void AppendFirstPart()
        {
            StringBuilder.AppendLine("...");
            StringBuilder.AppendLine(@$"private const string {ControlName}ResourcesName = ""ModernWpf.Controls.{ControlName}.Strings.Resources"";");
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
