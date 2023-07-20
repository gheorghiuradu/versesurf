using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MusicServer.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetDirectory(this Assembly assembly)
        {
            var assemblyPath = Path.GetDirectoryName(assembly.CodeBase);
            var appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");

            return appPathMatcher.Match(assemblyPath).Value;
        }
    }
}