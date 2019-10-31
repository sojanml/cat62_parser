using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {

  public class ADSB {
    public Double Latitude { get; set; }
    public Double Longtitude { get; set; }
    public Double Heading { get; set; }
    public String Registration { get; set; }
    public String HexCode { get; set; }
    public DateTime ADSBDate { get; set; }
    public Double Altitude { get; set; }
    public Double Speed { get; set; } = 0;

    internal void SetHeading(Double lat, Double lon, Double LastHeading) {
      //this.Heading = GEO.DegreeBearing(this.Latitude, this.Longtitude, lat, lon);
      LatLng P1 = new LatLng { Lat = lat, Lng = lon };
      LatLng P2 = new LatLng { Lat = this.Latitude, Lng = this.Longtitude };
      Double Distance = GEO.Distance(P1, P2);
      if (Distance > 50) {
        this.Heading = GEO.Angle(P1, P2);
      } else {
        this.Heading = LastHeading + 0;
      }
    }

    internal void Update(SqlConnection CN) {

      StringBuilder SB = new StringBuilder();
      String IPAddress = "0.0.0.0";


      String SQL = $@"[dbo].[usp_ADSB_UpdateInsert]
		    @FlightID = N'{this.Registration}',
		    @HexID = N'{this.HexCode}',
		    @FlightTime = N'{ ADSBDate.ToString("yyyy-MM-dd HH:mm:ss")}',
		    @Lat = {Latitude},
		    @Lon = {Longtitude},
		    @Alt = {Altitude},
		    @speed = {Speed},
		    @track = N'{this.Heading}',
		    @flightsource = N'Exponent',
		    @newtrack = N'{Registration}',
		    @category = N'ADSI',
		    @IPAddress = N'{IPAddress}'";
      using (SqlCommand cmd = new SqlCommand(SQL, CN)) {
        cmd.ExecuteNonQuery();
      }//using (SqlCommand cmd)

    }

  }

}