using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsGenerator
{
    class FrameFileInfo
    {
        public FileInfo FileInfo;

        public string AnimName;

        public string AnimNameUpper;

        public int FrameIndex;

        public SizeInt PixelSize;

        public Point SheetPos;

        // Debug
        public override string ToString()
        {
            return $"{{ AnimName: \"{AnimName}\" FrameIndex: {FrameIndex} PixelSize: {PixelSize.Width},{PixelSize.Height} SheetPos: {SheetPos.X},{SheetPos.Y} }}";
        }
    }
}
