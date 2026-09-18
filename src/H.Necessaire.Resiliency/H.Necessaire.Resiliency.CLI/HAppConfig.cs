using H.Necessaire.Runtime;
using System.Reflection;

namespace H.Necessaire.Resiliency.CLI
{
    internal static class HAppConfig
    {
        const string srcFolderRelativePath = "/src/H.Necessaire.Resiliency/";

        public static ImAnApiWireup WithDefaultRuntimeConfig(this ImAnApiWireup wireup)
        {
            return
                wireup
                .With(x => x.Register<RuntimeConfig>(() => new RuntimeConfig
                {
                    Values = new[] {
                        "NuSpecRootFolderPath".ConfigWith(GetCodebaseFolderPath()),
                    },
                }));
            ;
        }

        static string ReadConnectionStringFromFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
                return null;

            if (!HSafe.Run(() => File.ReadAllText(fileInfo.FullName)).Ref(out var res, out var result))
                return null;

            return result.NullIfEmpty();
        }

        static string GetCodebaseFolderPath()
        {
            string codeBase = Assembly.GetExecutingAssembly()?.Location ?? string.Empty;
            UriBuilder uri = new UriBuilder(codeBase);
            string dllPath = Uri.UnescapeDataString(uri.Path);
            int srcFolderIndex = dllPath.ToLowerInvariant().IndexOf(srcFolderRelativePath, StringComparison.OrdinalIgnoreCase);
            if (srcFolderIndex < 0)
                return string.Empty;
            string srcFolderPath = Path.GetDirectoryName(dllPath.Substring(0, srcFolderIndex + srcFolderRelativePath.Length)) ?? string.Empty;
            return srcFolderPath;
        }
    }
}
