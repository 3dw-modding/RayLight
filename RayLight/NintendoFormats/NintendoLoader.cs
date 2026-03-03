
using System.Diagnostics;
using System.IO;
using System.Runtime;
using Binary_Stream;
using Nintendo.Bfres;
using SarcLibrary;
using MsbtLib;
using Nintendo.Aamp;
using BymlLibrary;


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

    internal class ByamlContainer
    {
        public Byml byml;
        public SZSArchive OriginArchive = null;
        public string OriginFileName = null;

        Revrs.Endianness endianness;
        ushort version;

        public ByamlContainer(byte[] ByamlData, SZSArchive originArchive, string originFileName)
        {
            OriginArchive = originArchive;
            OriginFileName = originFileName;

            //Im going to try sanatise the bytes, cus for some reason you cant reopen them.
            ByamlData = ByamlData.ToArray();

            Load(ByamlData);
        }

        //HACK! Treats V1 as V2.
        private byte[] VersionHack(byte[] ByamlData, int version, bool bigEndian)
        {
            if (version == 1)
            {
                byte HackVersion = 0x3;
                //Change version, since V3 parser parses V1 files just fine.
                if (bigEndian)
                {
                    ByamlData[2] = 0;
                    ByamlData[3] = HackVersion;
                }
                else
                {
                    ByamlData[2] = HackVersion;
                    ByamlData[3] = 0;
                }
            }
            
            return ByamlData;
        }

        private ushort GetVersion(byte[] ByamlData, bool bigEndian)
        {
            return bigEndian ? (ushort)((ByamlData[2] << 8) | ByamlData[3]) : (ushort)(ByamlData[2] | (ByamlData[3] << 8));
        }

        private bool IsBigEndian(byte[] ByamlData)
        {
            if (ByamlData[0] == (byte)'Y' && ByamlData[1] == (byte)'B')
            {
                return false; // "YB" → little-endian
            }
            else if (ByamlData[0] == (byte)'B' && ByamlData[1] == (byte)'Y')
            {
                return true; // "BY" → big-endian
            }
            else throw new InvalidDataException("Not a valid BYML header.");
        }

        public void test()
        {
            Console.WriteLine(byml.ToYaml());
            //string TestPath = OriginArchive.FilePath + "_" + OriginFileName + ".yml";
            //File.WriteAllText(TestPath, byml.ToYaml());
        }

        public void Load(Byte[] ByamlData)
        {
            endianness = IsBigEndian(ByamlData) ? Revrs.Endianness.Big : Revrs.Endianness.Little;
            version = GetVersion(ByamlData, endianness == Revrs.Endianness.Big);
            ByamlData = VersionHack(ByamlData, version, endianness == Revrs.Endianness.Big);
            byml = Byml.FromBinary(ByamlData);
        }

        public void Reload()
        {
            if (OriginArchive != null)
            {
                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        byte[] ByamlData = OriginArchive.files[i].Data.ToArray();
                        Load(ByamlData);
                        break;
                    }
                }
            }
        }

        public void Save()
        {
            if (OriginArchive != null && OriginFileName != null)
            {
                using MemoryStream ms = new();
                if (version == 1) byml.WriteBinary(ms, endianness, 2); //HACK! Treats V1 as V2 since 3DW can read it. THIS BREAKS V1 ONLY GAMES!
                else byml.WriteBinary(ms, endianness, version);
                
                
                byte[] ByamlData = ms.ToArray();

                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        OriginArchive.files[i].Data = ByamlData;
                        break;
                    }
                }
            }
    }
    }
    internal class AampContainer
    {

        public string FilePath = null;
        public SZSArchive OriginArchive = null;
        public string OriginFileName = null;
        public Nintendo.Aamp.AampFile aamp;

        public AampContainer(byte[] AampData, SZSArchive originArchive, string originFileName)
        {
            OriginArchive = originArchive;
            OriginFileName = originFileName;

            //Im going to try sanatise the bytes, cus for some reason you cant reopen them.
            AampData = AampData.ToArray();
            aamp = new(AampData);
        }

        public AampContainer(string path)
        {
            FilePath = path;
            aamp = new(path);
        }

        public string GetName()
        {
            if (OriginFileName != null) return OriginFileName;
            else if (FilePath != null) return Path.GetFileNameWithoutExtension(FilePath);
            return "";
        }

        public void test()
        {
            //Console.WriteLine(aamp.ToJson());
            string TestPath =  OriginArchive.FilePath + "_" + OriginFileName + ".yml";
            File.WriteAllText(TestPath, aamp.ToYml());
        }

        public void Save()
        {
            if (OriginArchive != null && OriginFileName != null)
            {
                byte[] aampData = aamp.ToBinary();
                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        OriginArchive.files[i].Data = aampData;
                        break;
                    }
                }
            }
            else if (FilePath != null)
            {
                aamp.WriteBinary(FilePath);
            }
        }

        public void Reload()
        {
            if (OriginArchive != null)
            {
                for (int i = 0; i < OriginArchive.files.Count; i++)
                {
                    if (OriginArchive.files[i].Name == OriginFileName)
                    {
                        byte[] AampData = OriginArchive.files[i].Data.ToArray();
                        aamp = new(AampData);
                        break;
                    }
                }
            }
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
            }
            else if (FilePath != null)
            {
                byte[] msbtData = msbt.Write();
                File.WriteAllBytes(FilePath, msbtData);
            }
        }
    }

}
