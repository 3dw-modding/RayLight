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

        //Editors
        public SZSEditorState SZSEditorState = new SZSEditorState();
        public MsbtEditorState MSBTEditorState = new MsbtEditorState();

        public AampEditorState AampEditorState = new AampEditorState();


    }
}
