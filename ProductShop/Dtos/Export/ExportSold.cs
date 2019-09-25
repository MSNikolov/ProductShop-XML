using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    public class ExportSold
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public List<SoldProductExport> Products { get; set; }
    }
}