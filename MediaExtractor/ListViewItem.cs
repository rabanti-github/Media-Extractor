using System;

namespace MediaExtractor
{
    public class ListViewItem
    {
        public enum FileType
        {
            image,
            other,
            none,
        }

        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public FileType Type { get; set; }
        public Extractor.ExtractorItem FileReference { get; set; }
    }
}
