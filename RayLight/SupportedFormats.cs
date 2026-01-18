using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayLight.NintendoFormats;

namespace RayLight
{
    internal class SupportedFormats
    {
        static String[] supportedFormats = new String[] { ".szs" };

        public static bool isSupported(String format)
        {
            return supportedFormats.Contains(format.ToLower());
        }

        public static void HandleFile(String path, WindowState windowState)
        {
            Console.WriteLine($"loading {path}");
            switch (Path.GetExtension(path).ToLower())
            {
                case (".szs"):
                    Console.WriteLine($"loading {path} as szs");
                    
                    foreach (SZSArchive archive in windowState.SZSEditorState.loadedSZS)
                    {
                        if (archive.FilePath.ToLower() == path.ToLower()) break;
                    }
                    
                    windowState.SZSEditorState.loadedSZS.Add(new SZSArchive(path));
                    windowState.SZSViewerOpen = true;
                    break;

            }
        }
    }
}
