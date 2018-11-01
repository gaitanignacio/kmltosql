using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.IO;

namespace Kml2Sql.Mapping
{
    internal static class KMLParser
    {
        public static Kml Parse(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                return Parse(sr.BaseStream);
            }
                
        }

        internal static Kml Parse(Stream fileStream)
        {
            KmlFile file = KmlFile.Load(fileStream);
            Kml kml = file.Root as Kml;
            if (kml == null)
            {
                throw new Exception("Could not parse file into KML. If this is a KMZ, unzip it first!");
            }
            return kml;
        }
    }
}
