
using System.Diagnostics;
using System.IO;
using System.Runtime;
using Binary_Stream;
using Nintendo.Bfres;
using SarcLibrary;
using MsbtLib;
using Nintendo.Aamp;
using BymlLibrary;
using OatmealDome.BinaryData.Extensions;


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

        private byte[] ConvertToV2(byte[] ByamlData)
        {
            Stream Stream = new MemoryStream(ByamlData);
            object V1Byaml = OatmealDome.NinLib.Byaml.Dynamic.ByamlFile.Load(Stream);

            OatmealDome.NinLib.Byaml.ByamlSerializerSettings byamlSerializerSettings = new OatmealDome.NinLib.Byaml.ByamlSerializerSettings();
            byamlSerializerSettings.ByteOrder = endianness == Revrs.Endianness.Big ? OatmealDome.BinaryData.ByteOrder.BigEndian : OatmealDome.BinaryData.ByteOrder.LittleEndian;
            byamlSerializerSettings.Version = OatmealDome.NinLib.Byaml.ByamlVersion.Two;

            Stream = new MemoryStream();
            OatmealDome.NinLib.Byaml.Dynamic.ByamlFile.Save(Stream, V1Byaml, byamlSerializerSettings);

            return Stream.ReadBytes((int)Stream.Length);
        }

        private byte[] ConvertToV1(byte[] ByamlData)
        {
            Stream Stream = new MemoryStream(ByamlData);
            object V2Byaml = OatmealDome.NinLib.Byaml.Dynamic.ByamlFile.Load(Stream);

            OatmealDome.NinLib.Byaml.ByamlSerializerSettings byamlSerializerSettings = new OatmealDome.NinLib.Byaml.ByamlSerializerSettings();
            byamlSerializerSettings.ByteOrder = endianness == Revrs.Endianness.Big ? OatmealDome.BinaryData.ByteOrder.BigEndian : OatmealDome.BinaryData.ByteOrder.LittleEndian;
            byamlSerializerSettings.Version = OatmealDome.NinLib.Byaml.ByamlVersion.One;

            Stream = new MemoryStream();
            OatmealDome.NinLib.Byaml.Dynamic.ByamlFile.Save(Stream, V2Byaml, byamlSerializerSettings);

            return Stream.ReadBytes((int)Stream.Length);
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
            if (version != 1)
            {
                byml = Byml.FromBinary(ByamlData);
                return;
            }
            //Convert to V2 to load it, since the fast loader can't load V1.
            //This is segnificantly slower, but BymlLibrary doesn't support V1. 
            //So we need to convert it to V2 to load it, then we can convert it back to V1 when saving.
            else
            {
                byml = Byml.FromBinary(ConvertToV2(ByamlData));    
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
                if (version == 1) byml.WriteBinary(ms, endianness, 2); //Write as V2 for now.
                else byml.WriteBinary(ms, endianness, version);
                
                
                byte[] ByamlData = ms.ToArray();
                if (version == 1) ByamlData = ConvertToV1(ByamlData); //Convert back to V1 if it was originally V1.

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
