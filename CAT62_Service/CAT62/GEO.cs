using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public static class GEO {

    public static Double Angle(LatLng P1, LatLng P2) {
      //stackoverflow.com/questions/3932502/calculate-angle-between-two-latitude-longitude-points
      /*
      double long1 = lng, long2 = Coordinate.lng;
      double lat1 = lat, lat2 = Coordinate.lat;
      */
      double long1 = P1.Lng, long2 = P2.Lng;
      double lat1 = P1.Lat, lat2 = P2.Lat;

      double dLon = (long2 - long1);

      double y = Math.Sin(dLon) * Math.Cos(lat2);
      double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
              * Math.Cos(lat2) * Math.Cos(dLon);

      double brng = Math.Atan2(y, x);

      brng = brng * (180.0 / Math.PI); //convert to degree
      brng = (brng + 360) % 360;
      brng = 360 - brng; // count degrees counter-clockwise - remove to make clockwise

      return (int)brng;
    }



    public static Double Distance(LatLng P1, LatLng P2) {
      double long1 = P1.Lng, long2 = P2.Lng;
      double lat1 = P1.Lat, lat2 = P2.Lat;


      Double MileToKilometer = 1.609344;
      var baseRad = Math.PI * lat1 / 180;
      var targetRad = Math.PI * lat2 / 180;
      var theta = long1 - long2;
      var thetaRad = Math.PI * theta / 180;

      double dist =
          Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
          Math.Cos(targetRad) * Math.Cos(thetaRad);
      dist = Math.Acos(dist);

      dist = dist * 180 / Math.PI;
      dist = dist * 60 * 1.1515;
      dist = dist * MileToKilometer * 1000;
      return dist;
    }
  }
}
