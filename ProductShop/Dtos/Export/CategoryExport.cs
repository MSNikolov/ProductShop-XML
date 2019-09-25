using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("Category")]
    public class CategoryExport
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("count")]
        public int Products { get; set; }

        [XmlElement("averagePrice")]
        public decimal AvgPrice { get; set; }

        [XmlElement("totalRevenue")]
        public decimal Tot { get; set; }
    }
}
