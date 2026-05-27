using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    [DataContract]
    public class IconData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public List<string> Tags { get; set; } = [];

        public string Character => char.ConvertFromUtf32(Convert.ToInt32(Code, 16));

        public string CodeGlyph => "\\x" + Code;

        public string TextGlyph => "&#x" + Code + ";";
    }
}
