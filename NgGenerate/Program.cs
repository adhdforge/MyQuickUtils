using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NgGenerate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3) {
                Console.Error.WriteLine("Nothing to do?");
                return;
            }

            string name = args[0];
            string template = args[1];
            string target = args[2];
            if (!target.EndsWith(@"\")) target += @"\";

            string[] words = Regex.Matches(name, "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)")
                .OfType<Match>()
                .Select(m => m.Value.ToLowerInvariant())
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToArray();

            string result = string.Join("-", words);

            string targetDir = System.IO.Path.Combine(target, result) + @"\";
            System.IO.Directory.CreateDirectory(targetDir);
            foreach (string src in System.IO.Directory.GetFiles(System.IO.Path.Combine(Properties.Settings.Default.TemplateDir, template))) {
                string file = System.IO.Path.GetFileName(src).Replace(template, result);
                string targetFile = System.IO.Path.Combine(targetDir, file);
                string txt = System.IO.File.ReadAllText(src);
                txt = txt.Replace("%NAME%", name).Replace("%name%", result);
                System.IO.File.WriteAllText(targetFile, txt);
            }
        }
    }
}
