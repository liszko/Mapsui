using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Mapsui.Desktop.Tests.Utilities;
using Mapsui.Providers.GeoTiff;
using Mapsui.Providers.Shapefile;
using NUnit.Framework;

namespace Mapsui.Desktop.Tests
{
    [TestFixture]
    class ShapeFileTests
    {
        [Test]
        public void ShapeFileStreamConstructor()
        {
            /*var shpLocation = $@"{AssemblyInfo.AssemblyDirectory}\Resources\50mc_cities.shp";
            var dbfLocation = $@"{AssemblyInfo.AssemblyDirectory}\Resources\50mc_cities.dbf";
            var shxLocation = $@"{AssemblyInfo.AssemblyDirectory}\Resources\50mc_cities.shx";
            StreamReader shpSr = new StreamReader(shpLocation);
            StreamReader dbfSr = new StreamReader(dbfLocation);
            StreamReader shxSr = new StreamReader(shxLocation);/**/
            //"Mapsui.Desktop.Tests.Resources.50mc_cities.dbf"
            var assembly = Assembly.GetExecutingAssembly();
            var shpResourceName = "Mapsui.Desktop.Tests.Resources.50mc_cities.shp";
            var dbfResourceName = "Mapsui.Desktop.Tests.Resources.50mc_cities.dbf";
            var idxResourceName = "Mapsui.Desktop.Tests.Resources.50mc_cities.shx";

            Stream streamShp = assembly.GetManifestResourceStream(shpResourceName);
            StreamReader readerShp = new StreamReader(streamShp);
            //using ()
            using (Stream streamIdx = assembly.GetManifestResourceStream(idxResourceName))
            using (Stream streamDbf = assembly.GetManifestResourceStream(dbfResourceName))
            //using ()
            using (StreamReader readerIdx = new StreamReader(streamIdx))
            using (StreamReader readerDbf = new StreamReader(streamDbf))
            {

                var shape = new ShapeFile(readerShp, readerIdx, readerDbf) {CRS = "EPSG:4326"};
                var objectsInView = shape.GetFeaturesInView(shape.GetExtents(), 10.0);
                Assert.AreEqual(1249, objectsInView.Count());
            }
        }
    }
}
