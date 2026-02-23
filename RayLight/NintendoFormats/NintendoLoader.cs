
using System.Diagnostics;
using System.IO;
using System.Runtime;
using Binary_Stream;
using Nintendo.Bfres;
using SarcLibrary;
using MsbtLib;


namespace RayLight.NintendoFormats
{
    internal class LoadedFile
    {
        public String Name;
        public byte[] Data;

        public LoadedFile(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

    }
    internal class SZSArchive
    {
        public String FilePath;
        public List<LoadedFile> files;
        public Revrs.Endianness endian;
        public SZSArchive(String path)
        {
            FilePath = path;
            files = new List<LoadedFile>();
            Console.WriteLine($"Loading: {FilePath}");
            this.Reload();
        }


        public void Save()
        {
            var sarc = new Sarc();
            sarc.Endianness = endian;
            foreach (var file in files)
            {
                sarc.Add(file.Name, file.Data);
            }

            //Write SARC to memory
            using MemoryStream ms = new();
            sarc.Write(ms, endian);

            //Clean up sarc
            sarc = null;

            //Compress with Yaz0
            byte[] compressed = Yaz0.Compress(ms.ToArray()).ToArray();

            File.WriteAllBytes(FilePath, compressed);
        }

        public void Reload()
        {
            //Discard loaded files
            files = new List<LoadedFile>();


            //Load them again from archive
            byte[] compressed = File.ReadAllBytes(FilePath);
            byte[] decompressed = Yaz0.Decompress(compressed);
            compressed = null; //unload the compressed archive, we dont need it anymore.

            // Step 2: Load SARC
            var sarc = Sarc.FromBinary(decompressed);
            endian = sarc.Endianness;
            decompressed = null; //we don't need the decompressed archive anymore

            // Step 3: Extract files
            foreach (var entry in sarc)
            {
                string name = entry.Key;      // filename inside the SARC
                byte[] data = sarc[entry.Key].ToArray();     // file contents

                files.Add(new LoadedFile(name, data));
            }

            //clean up remaining un-nessicery memory
            sarc.Clear();
            sarc = null;
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            Console.WriteLine($"Loaded/Reloaded: {FilePath}");
        }

    }

    internal class MSBTFile
    {
        public string FilePath = null;
        public SZSArchive OriginArchive = null;
        public string OriginFileName = null;
        public Msbt msbt;

        public MSBTFile(string path)
        {
            FilePath = path;
            FileStream MsbtBin = File.OpenRead(FilePath);
            msbt = new(MsbtBin);
        }

        public MSBTFile(byte[] MsbtData, SZSArchive originArchive, string originFileName)
        {
            OriginArchive = originArchive;
            OriginFileName = originFileName;
            msbt = new(MsbtData);
        }

        public string GetName()
        {
            if (OriginFileName != null) return OriginFileName;
            else if (FilePath != null) return Path.GetFileNameWithoutExtension(FilePath);
            return "";
        }

        public void Reload()
        {
            if (OriginArchive != null)
            {
                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        msbt = new(OriginArchive.files[i].Data);
                        break;
                    }
                }
            }
        }

        public void Save()
        {
            if (OriginArchive != null && OriginFileName != null)
            {
                byte[] msbtData = msbt.Write();
                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        OriginArchive.files[i].Data = msbtData;
                        break;
                    }
                }
                //Why was this saved here? Let the user pick when to save the archive, not force it on every file save.
                //OriginArchive.Save(); 
            }
            else if (FilePath != null)
            {
                byte[] msbtData = msbt.Write();
                File.WriteAllBytes(FilePath, msbtData);
            }
        }
    }

}
