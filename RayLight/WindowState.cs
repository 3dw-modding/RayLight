using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayLight.NintendoFormats;
using RayLight.Windows;

namespace RayLight
{
    internal class WindowState
    {
        //Open states
        public bool AboutWindowOpen = false;
        public bool FileWindowOpen = true;
        public bool SZSViewerOpen = false;

        //SZS Viewer/Editor
        public SZSEditorState SZSEditorState = new SZSEditorState();
    }
}
