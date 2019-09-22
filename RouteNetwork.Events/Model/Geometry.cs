using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RouteNetwork.Events.Model
{
    public class Geometry
    {
        public Geometry (string geoJsonType, string geoJsonCoordinates)
        {
            GeoJsonType = geoJsonType;
            GeoJsonCoordinates = geoJsonCoordinates;
        }
        public string GeoJsonType { get; set; }

        public string GeoJsonCoordinates { get; set; }
    }
}
