using System.Text;

namespace EverythingX.Services
{
    public class FileService
    {
        readonly DirectoryInfo root;

        public FileService(DirectoryInfo root)
        {
            this.root = root;

            if (!root.Exists)
            {
                throw new ArgumentNullException("FileService 'root' parameter");
            }
        }

        public void ForEach(string[] extensions, Action<string, string> action)
        {
            this.root
                .GetFiles("*", SearchOption.AllDirectories)
                .Where(item => extensions.Contains(item.Extension, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(item =>
                {
                    action(item.FullName, File.ReadAllText(item.FullName, Encoding.UTF8));
                });
        }
    }
}
