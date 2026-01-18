
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using NativeFileDialogSharp;
using System.Reflection;
using RayLight.NintendoFormats;
using System.Xml.Linq;
using System.Runtime;

namespace RayLight.Windows
{
    internal class SZSEditorState
    {
        public bool Open = false;

        public int SZSViwerTab = 0;
        public List<SZSArchive> loadedSZS = new List<SZSArchive>();

        //Export
        public LoadedFile ExportFile = null;
        public SZSArchive ExportFileOrigin = null;
        public string ExportDirectory = "";

        //Import
        public SZSArchive ImportTargetSZS;
        public string ImportFilePath;
    }

    internal class SZSEditor
    {
        public static void RenderEditor(WindowState windowState)
        {
            SZSEditorState EditorState = windowState.SZSEditorState;

            //Prevent the tab being selected being invalid.
            EditorState.SZSViwerTab = EditorState.SZSViwerTab % EditorState.loadedSZS.Count();


            SZSArchive SelectedSZS = EditorState.loadedSZS[EditorState.SZSViwerTab];

            if (ImGui.Begin("SZS Viewer", ref windowState.SZSEditorState.Open, ImGuiWindowFlags.MenuBar))
            {

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Save"))
                        {
                            SelectedSZS.Save();
                        }
                        if (ImGui.MenuItem("Reload"))
                        {
                            SelectedSZS.Reload();
                        }
                        if (ImGui.MenuItem("Close"))
                        {
                            EditorState.loadedSZS.Remove(SelectedSZS);
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                if (ImGui.BeginTabBar("Loaded_SZS_TAB_BAR"))
                {
                    for (int i = 0; i < EditorState.loadedSZS.Count(); i++)
                    {
                        if (ImGui.BeginTabItem(Path.GetFileNameWithoutExtension(EditorState.loadedSZS[i].FilePath)))
                        {
                            EditorState.SZSViwerTab = i;
                            //EditorState.ExportFile = null;  //Close export window.
                            ImGui.EndTabItem();
                        }
                    }

                    ImGui.EndTabBar();
                }


                ImGui.TextUnformatted("Path: ");
                ImGui.TextWrapped(SelectedSZS.FilePath.Replace("\\","/").Replace("/"," / "));
                ImGui.Spacing();
                ImGui.TextUnformatted("Files:");

                ImGui.BeginChild("FileList-SZS", new Vector2(0, 0));





                // Files
                int fileIndex = 0;
                while (fileIndex < SelectedSZS.files.Count())
                {
                    bool FileRemoved = false;
                    LoadedFile file = SelectedSZS.files[fileIndex];
                    string name = file.Name;
                    string extention = "." + name.Split(".")[^1];

                    bool isSupported = SupportedFormats.isSupported(extention);

                    if (!isSupported) StyleData.ColourButtonRed();
                    else StyleData.ColourButtonGreen();

                    if (ImGui.Button(name))
                    {
                        if (isSupported)
                        {
                            //SupportedFormats.HandleFile(file, windowState);
                            SupportedFormats.HandleFile(extention, file.Data, name, SelectedSZS, windowState);
                        }
                    }
                    


                    ImGui.PopStyleColor(3);

                    StyleData.ColourButtonRed();
                    ImGui.SameLine();
                    if (ImGui.Button(IconFonts.FontAwesome6.TrashCan + $"##{name}"))
                    {
                        SelectedSZS.files.Remove(file);
                        FileRemoved = true;
                    }
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text("Remove File");
                        ImGui.EndTooltip();
                    }
                    ImGui.PopStyleColor(3);

                    if (!FileRemoved) fileIndex++;

                    ImGui.SameLine();
                    if (ImGui.Button(IconFonts.FontAwesome6.FloppyDisk + $"##{name}"))
                    {
                        EditorState.ExportFile = file;
                        EditorState.ExportFileOrigin = SelectedSZS;
                    }
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text("Export File");
                        ImGui.EndTooltip();
                    }

                }

                if (ImGui.Button($"Add File"))
                {
                    EditorState.ImportTargetSZS = SelectedSZS;
                    EditorState.ImportFilePath = "";
                }

                ImGui.EndChild();

                


                ImGui.End();
            }

            if (EditorState.ExportFile != null)
            {
                RenderExportMenu(EditorState);
            }
            if (EditorState.ImportTargetSZS != null)
            {
                RenderImportMenu(EditorState);
            }
        }

        public static void RenderExportMenu(SZSEditorState editorState)
        {
            SZSArchive SelectedSZS = editorState.loadedSZS[editorState.SZSViwerTab];
            if (!SelectedSZS.files.Contains(editorState.ExportFile)  || SelectedSZS != editorState.ExportFileOrigin)
            {
                //Don't let the user export a file they have deleted.
                //Or if they have changed tab to a different SZS
                editorState.ExportFile = null;
                editorState.ExportFileOrigin = null;
                return;
            }

            string ExportFilename = editorState.ExportFile.Name;
            LoadedFile file = editorState.ExportFile; //For shorthand.
            if (ImGui.Begin($"Export ##{ExportFilename}",ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextUnformatted($"File: {ExportFilename} from {Path.GetFileNameWithoutExtension(SelectedSZS.FilePath)}");

                ImGui.TextUnformatted("Export Path:");
                ImGui.SameLine();
                ImGui.InputText($"##Export_{ExportFilename} path entry", ref editorState.ExportDirectory, 260);

                string ExportInvalidReason = null;

                if (!Directory.Exists(editorState.ExportDirectory)) ExportInvalidReason = "Export Directory not valid.";
                else
                {
                    if (File.Exists(Path.Combine(editorState.ExportDirectory,file.Name)))
                    {
                        ExportInvalidReason = "Export Path allready has a file with that name.";
                    }
                }

                if (ExportInvalidReason != null) StyleData.ColourButtonRed();
                else StyleData.ColourButtonGreen();

                if (ImGui.Button($"Export ##Button {ExportFilename}"))
                {
                    if (ExportInvalidReason == null)
                    {
                        File.WriteAllBytes(Path.Combine(editorState.ExportDirectory, file.Name), file.Data);
                        editorState.ExportFile = null;
                        editorState.ExportFileOrigin = null;
                    }
                }
                if (ExportInvalidReason != null)
                {
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text(ExportInvalidReason);
                        ImGui.EndTooltip();
                    }
                }
                ImGui.PopStyleColor(3);

                ImGui.SameLine();
                if (ImGui.Button($"Cancel ##Button {ExportFilename}"))
                {
                    editorState.ExportFile = null;
                    editorState.ExportFileOrigin = null;
                }

                ImGui.End();
            }
        }

        public static void RenderImportMenu(SZSEditorState editorState)
        {
            
            if (!editorState.loadedSZS.Contains(editorState.ImportTargetSZS))
            {
                //Don't let the user att a file to an unloaded SZS
                editorState.ImportTargetSZS = null;
                return;
            }

            string szsName = Path.GetFileName( editorState.ImportTargetSZS.FilePath);
            if (ImGui.Begin($"Import ##{szsName}", ImGuiWindowFlags.AlwaysAutoResize))
            {

                ImGui.TextUnformatted($"Taret: {szsName}");

                ImGui.TextUnformatted("Export Path:");
                ImGui.SameLine();
                ImGui.InputText($"##Import_{szsName} path entry", ref editorState.ImportFilePath, 260);

                string ImportInvalidReason = null;

                if (!File.Exists(editorState.ImportFilePath))
                {
                    ImportInvalidReason = "File doesn't excist";
                }
                else { 
                    foreach (LoadedFile file in editorState.ImportTargetSZS.files)
                    {
                        if (file.Name == Path.GetFileName(editorState.ImportFilePath)) ImportInvalidReason = "File with that name allready is part of this SZS";
                    }
                }

                if (ImportInvalidReason != null) StyleData.ColourButtonRed();
                else StyleData.ColourButtonGreen();

                if (ImGui.Button($"Import ##Button {szsName}"))
                {
                    if (ImportInvalidReason == null)
                    {
                        //Load file
                        string filename = Path.GetFileName(editorState.ImportFilePath);
                        byte[] data = File.ReadAllBytes(filename);
                        LoadedFile loadedFile = new LoadedFile(filename, data);
                        editorState.ImportTargetSZS.files.Add(loadedFile);

                        //Cleanup
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                        data = null;


                        editorState.ImportTargetSZS = null;
                    }
                }
                if (ImportInvalidReason != null)
                {
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text(ImportInvalidReason);
                        ImGui.EndTooltip();
                    }
                }
                ImGui.PopStyleColor(3);

                ImGui.SameLine();
                if (ImGui.Button($"Cancel ##Button {szsName}"))
                {
                    editorState.ImportTargetSZS = null;
                }

                ImGui.End();
            }
        }
    }
}
