using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayLight.NintendoFormats;

namespace RayLight
{
    internal class SupportedFormats
    {
        static String[] supportedFormats = new String[] { ".szs", ".msbt" };

        public static bool isSupported(String format)
        {
            if (supportedFormats.Contains(format.ToLower()))
                return true;
            if (format.ToLower().StartsWith(".bagl"))
                return true;

            return false;
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
                    windowState.SZSEditorState.Open = true;
                    break;
                case (".msbt"):

                    foreach(MSBTFile msbtFile in windowState.MSBTEditorState.loadedMSBTs)
                    {
                        if (msbtFile.FilePath == null) continue;
                        if (msbtFile.FilePath.ToLower() == path.ToLower()) break;
                    }


                    windowState.MSBTEditorState.loadedMSBTs.Add(new MSBTFile(path));
                    break;


            }
        }

        public static void HandleFile(string type, byte[] data, string name, SZSArchive OriginArchive, WindowState windowState)
        {
            Console.WriteLine($"loading {name}{type}");

            switch (type.ToLower())
            {
                case (".msbt"):
                    windowState.MSBTEditorState.loadedMSBTs.Add(new MSBTFile(data,OriginArchive,name));
                    break;

                default:
                    //At minimum, in 3D world, its safe to assume a file beginning with "bagl" is an AAMP.
                    if (type.ToLower().StartsWith(".bagl"))
                    {
                        AampContainer newAamp = new AampContainer(data, OriginArchive, name);
                        windowState.AampEditorState.loadedAamps.Add(newAamp);
                        //newAamp.test();
                    }
                    else Console.WriteLine($"Unknown filetype {type} for file {name}");
                    break;
            }
        }
    }
}
