using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    [DataContract]
    public sealed class IconData
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Tags { get; set; }

        public string Character
        {
            get
            {
                try
                {
                    return char.ConvertFromUtf32(Convert.ToInt32(Code, 16));
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public string CodeGlyph
        {
            get { return "\\x" + Code; }
        }

        public string TextGlyph
        {
            get { return "&#x" + Code + ";"; }
        }
    }
}
