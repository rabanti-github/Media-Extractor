
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MediaExtractor.preview
{
    public abstract class AbstractPreview<T>
    {

        private bool initialized;
        private T preview;

        /// <summary>
        /// File extension of the item
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Message of the last occurred error when processing the item
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// If true, the item was identified as valid preview file
        /// </summary>
        public bool ValidItem { get; internal set; }

        /// <summary>
        /// MemoryStream of the item
        /// </summary>
        public MemoryStream Stream { get; private set; }

        /// <summary>
        /// Preview Object
        /// </summary>
        public T Preview {

            get
            {
                return preview;
            }
            protected set
            {
                preview = value;
                initialized = true;
            }
        }

        public bool Initialized
        {
            get { return initialized; }
        }





        protected AbstractPreview(MemoryStream stream, string extension)
        {
            Stream = stream;
            FileExtension = extension;
        }
        

        /// <summary>
        /// Abstract Method to create Preview of Type T (stored in property Preview)
        /// </summary>
        /// <returns>Preview object of type T</returns>
        public abstract void CreatePreview();


    }


}
