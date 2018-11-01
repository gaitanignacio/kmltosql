using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kml2Sql.Mapping
{
    internal static class ExtensionMethods
    {
        internal static string Sanitize(this string myString)
        {
            return myString.Replace("--", "").Replace(";", "").Replace("'", "\"");
        }
    }
}
