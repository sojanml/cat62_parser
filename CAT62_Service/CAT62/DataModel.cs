using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {

  public class DataField {
    public String Code { get; set; } = String.Empty;
    public String Name { get; set; }
    public int FieldLength { get; set; } = 0;
    public Boolean DynamicLength { get; set; } = false;
  }

  public class LatLng {
    public Double Lat { get; set; } = 0;
    public Double Lng { get; set; } = 0;
    public LatLng() {

    }
    public LatLng(byte[] latLngBytes) {

    }
  }

  public static class FieldMaping {
    public static Double WGS84 = 180D / (Double)Math.Pow(2, 25);

    public static Dictionary<int, DataField> DataModel = new Dictionary<int, DataField> {
      //            0 1 2 3 4 5 6 7
      //Bit 1, 9B - 1 0 0 1 1 0 1 1
      /*01*/{0, new DataField{ Code ="I062/010", Name= "Data Source Identifier",FieldLength=2 } },
      /*02*/{1, new DataField{ Code ="0000/001", Name="Unknown" } },
      /*03*/{2, new DataField{ Code="I062/015", Name="Service Identification ",FieldLength=1 } },
      /*04*/{3, new DataField{ Code="I062/070", Name="Time of Track Information",FieldLength=3 } },
      /*05*/{4, new DataField{ Code="I062/105", Name="Calculated Position in WGS84 Coordinates",FieldLength=8 } },
      /*06*/{5, new DataField{ Code="I062/100", Name="Calculated Track Position (Cartesian) ",FieldLength=6 } },
      /*07*/{6, new DataField{ Code="I062/185", Name="Calculated Track Velocity (Cartesian)",FieldLength=4 } },
      /*FX*/{7, new DataField{ Name="Extention" } },
      //        8 + 0 1 2 3 4 5 6 7
      //Bit 2, 0D - ‭0 0 0 0 1 1 0 1‬
      /*08*/{8 + 0, new DataField{ Code="I062/210", Name="Calculated Acceleration (Cartesian)",FieldLength=2 }},
      /*09*/{8 + 1, new DataField{ Code="I062/060", Name="Track Mode 3/A Code",FieldLength=2 } },
      /*10*/{8 + 2, new DataField{ Code="I062/245", Name="Target Identification",FieldLength=7 } },
      /*11*/{8 + 3, new DataField{ Code="I062/380", Name="Aircraft Derived Data",FieldLength=1  } },
      /*12*/{8 + 4, new DataField{ Code="I062/040", Name="Track Number",FieldLength=2 } },
      /*13*/{8 + 5, new DataField{ Code="I062/080", Name="Track Status",FieldLength=0, DynamicLength=true } },
      /*14*/{8 + 6, new DataField{ Code="I062/290", Name="System Track Update Ages",FieldLength=0, DynamicLength=true  } },
      /*FX*/{8 + 7, new DataField{ Name="Extention" } },
      //        16 + 0 1 2 3 4 5 6 7
      //xxxxxxxxxxBit 3, 8C -  ‭‭1 0 0 0 1 1 0 0‬
      /*15*/{16 + 0, new DataField{ Code="I062/200", Name="Mode of Movement",FieldLength=1 }},
      /*16*/{16 + 1, new DataField{ Code="I062/295", Name="Track Data Ages",FieldLength=0, DynamicLength=true } },
      /*17*/{16 + 2, new DataField{ Code="I062/136", Name="Measured Flight Level",FieldLength=2 } },
      /*18*/{16 + 3, new DataField{ Code="I062/130", Name="Calculated Track Geometric Altitude",FieldLength=2  } },
      /*19*/{16 + 4, new DataField{ Code="I062/135",  Name="Calculated Track Barometric Altitude",FieldLength=2 } },
      /*20*/{16 + 5, new DataField{ Code="I062/220",  Name="Calculated Rate of Climb/Descent",FieldLength=2 } },
      /*21*/{16 + 6, new DataField{ Code="I062/390", Name="Flight Plan Related Data", DynamicLength=true } },
      /*FX*/{16 + 7, new DataField{ Name="Extention" } },

      /*22*/{24 + 0, new DataField{ Code="I062/270", Name="Target Size & Orientation",DynamicLength=true }},
      /*23*/{24 + 1, new DataField{ Code="I062/300", Name="Vehicle Fleet Identification",FieldLength=1 } },
      /*24*/{24 + 2, new DataField{ Code="I062/110", Name="Mode 5 Data reports & Extended Mode 1 Code",FieldLength=0,DynamicLength=true } },
      /*25*/{24 + 3, new DataField{ Code="I062/120", Name="Track Mode 2 Code",FieldLength=2 } },
      /*26*/{24 + 4, new DataField{ Code="I062/510", Name="Composed Track Number",FieldLength=0,DynamicLength=true } },
      /*27*/{24 + 5, new DataField{ Code="I062/500", Name="Estimated Accuracies",FieldLength=0,DynamicLength=true } },
      /*28*/{24 + 6, new DataField{ Code="I062/340", Name="Measured Information",FieldLength=0,DynamicLength=true  } },
      /*FX*/{24 + 7, new DataField{ Name="Extention" } },

      /*29*/{32 + 0, new DataField{ Code="0000/001", Name="Spare",FieldLength=1 }},
      /*30*/{32 + 1, new DataField{ Code="0000/002", Name="Spare",FieldLength=1 } },
      /*31*/{32 + 2, new DataField{ Code="0000/003", Name="Spare",FieldLength=1 } },
      /*32*/{32 + 3, new DataField{ Code="0000/004", Name="Spare",FieldLength=1 } },
      /*33*/{32 + 4, new DataField{ Code="0000/005",  Name="Spare",FieldLength=1 } },
      /*34*/{32 + 5, new DataField{ Code="0000/0RE",  Name="Reserved Expansion Field",FieldLength=0,DynamicLength=true } },
      /*35*/{32 + 6, new DataField{ Code="0000/0SP", Name="Reserved For Special Purpose Indicator",FieldLength=0,DynamicLength=true } },
      /*FX*/{32 + 7, new DataField{ Name="Extention" } },

    };



    public static Dictionary<int, DataField> AircraftDerivedData = new Dictionary<int, DataField> {
      //            0 1 2 3 4 5 6 7
      //Bit 1, 9B - 1 0 0 1 1 0 1 1
      /*01*/{0, new DataField{ Code="ADR", Name= "Subfield #1: Target Address",FieldLength=3 } },
      /*02*/{1, new DataField{ Code="ID", Name="Subfield #2: Target Identification",FieldLength=6  } },
      /*03*/{2, new DataField{ Code="MHG", Name="Subfield #3: Magnetic Heading",FieldLength=2 } },
      /*04*/{3, new DataField{ Code="IAS", Name="Subfield #4: Indicated Airspeed/Mach Number",FieldLength=2 } },
      /*05*/{4, new DataField{ Code="TAS", Name="Subfield #5: True Airspeed",FieldLength=2 } },
      /*06*/{5, new DataField{ Code="SAL", Name="Subfield #6: Selected Altitude",FieldLength=2 } },
      /*07*/{6, new DataField{ Code="FSS", Name="Subfield #7: Final State Selected Altitude",FieldLength=2 } },
      /*FX*/{7, new DataField{ Code="FX1", Name="Extension indicator" } },
      //        8 + 0 1 2 3 4 5 6 7
      //Bit 2, 0D - ‭0 0 0 0 1 1 0 1‬
      /*08*/{8 + 0, new DataField{ Code="TIS", Name="Subfield #8: Trajectory Intent Status",FieldLength=1 }},
      /*09*/{8 + 1, new DataField{ Code="TID", Name="Subfield #9: Trajectory Intent Data",FieldLength=16 } },
      /*10*/{8 + 2, new DataField{ Code="COM", Name="Subfield #10: Communications / ACAS Capability and Flight Status",FieldLength=2 } },
      /*11*/{8 + 3, new DataField{ Code="SAB", Name="Subfield #11: Status reported by ADS-B",FieldLength=2  } },
      /*12*/{8 + 4, new DataField{ Code="ACS", Name="Subfield #12: ACAS Resolution Advisory Report ",FieldLength=7 } },
      /*13*/{8 + 5, new DataField{ Code="BVR", Name="Subfield #13: Barometric Vertical Rate",FieldLength=2 } },
      /*14*/{8 + 6, new DataField{ Code="GVR", Name="Subfield #14: Geometric Vertical Rate",FieldLength=2  } },
      /*FX*/{8 + 7, new DataField{ Code="FX2", Name="Extension indicator" } },
      //        16 + 0 1 2 3 4 5 6 7
      //xxxxxxxxxxBit 3, 8C -  ‭‭1 0 0 0 1 1 0 0‬
      /*15*/{16 + 0, new DataField{ Code="RAN", Name="Subfield #15: Roll Angle",FieldLength=2 }},
      /*16*/{16 + 1, new DataField{ Code="TAR", Name="Subfield #16: Track Angle Rate",FieldLength=2 } },
      /*17*/{16 + 2, new DataField{ Code="TAN", Name="Subfield #17: Track Angle",FieldLength=2 } },
      /*18*/{16 + 3, new DataField{ Code="GSP", Name="Subfield #18: Ground Speed",FieldLength=2  } },
      /*19*/{16 + 4, new DataField{ Code="VUN", Name="Subfield #19: Velocity Uncertainty",FieldLength=1 } },
      /*20*/{16 + 5, new DataField{ Code="MET", Name="Subfield #20: Meteorological Data",FieldLength=8 } },
      /*21*/{16 + 6, new DataField{ Code="EMC", Name="Subfield #21: Emitter Category",FieldLength=1 } },
      /*FX*/{16 + 7, new DataField{ Code="FX3", Name="Extension indicator"  } },

      /*22*/{24 + 0, new DataField{ Code="POS", Name="Subfield #22: Position Data",FieldLength=6 }},
      /*23*/{24 + 1, new DataField{ Code="GAL", Name="Subfield #23: Geometric Altitude Data",FieldLength=2 } },
      /*24*/{24 + 2, new DataField{ Code="PUN", Name="Subfield #24: Position Uncertainty Data",FieldLength=1 } },
      /*25*/{24 + 3, new DataField{ Code="MB", Name="Subfield #25: Mode S MB Data",FieldLength=9 } },
      /*26*/{24 + 4, new DataField{ Code="IAR", Name="Subfield #26: Indicated Airspeed",FieldLength=2} },
      /*27*/{24 + 5, new DataField{ Code="MAC", Name="Subfield #27: Mach Number ",FieldLength=2 } },
      /*28*/{24 + 6, new DataField{ Code="BPS", Name="Subfield #28: Barometric Pressure Setting",FieldLength=2 } },
      /*FX*/{24 + 7, new DataField{ Code="FX4", Name="Extension indicator" } }

    };



    public static Dictionary<int, DataField> FlightPlanRelatedData = new Dictionary<int, DataField> {
      //            0 1 2 3 4 5 6 7
      //Bit 1, 9B - 1 0 0 1 1 0 1 1
      /*01*/{0, new DataField{ Code="TAG", Name= "Subfield #1: FPPS Identification Tag",FieldLength=2 } },
      /*02*/{1, new DataField{ Code="CSN", Name="Subfield #2: Callsign",FieldLength=7  } },
      /*03*/{2, new DataField{ Code="IFI", Name="Subfield #3: IFPS_FLIGHT_ID",FieldLength=4 } },
      /*04*/{3, new DataField{ Code="FCT", Name="Subfield #4: Flight Category",FieldLength=1 } },
      /*05*/{4, new DataField{ Code="TAC", Name="Subfield #5: Type of Aircraft",FieldLength=4 } },
      /*06*/{5, new DataField{ Code="WTC", Name="Subfield #6: Selected Altitude",FieldLength=1 } },
      /*07*/{6, new DataField{ Code="DEP", Name="Subfield #7: Departure Airport",FieldLength=4 } },
      /*FX*/{7, new DataField{ Code="FX1", Name="Extension indicator" } },
      //        8 + 0 1 2 3 4 5 6 7
      //Bit 2, 0D - ‭0 0 0 0 1 1 0 1‬
      /*08*/{8 + 0, new DataField{ Code="DST", Name="Subfield #8: Destination Airpor",FieldLength=4 }},
      /*09*/{8 + 1, new DataField{ Code="RDS", Name="Subfield #9: Runway Designation",FieldLength=3 } },
      /*10*/{8 + 2, new DataField{ Code="CFL", Name="Subfield #10: Current Cleared Flight Level",FieldLength=2 } },
      /*11*/{8 + 3, new DataField{ Code="CTL", Name="Subfield #11: Current Control Position",FieldLength=2  } },
      /*12*/{8 + 4, new DataField{ Code="TOD", Name="Subfield #12: Time of Departure / Arrival",FieldLength=5 } },
      /*13*/{8 + 5, new DataField{ Code="AST", Name="Subfield #13: Aircraft Stand",FieldLength=6 } },
      /*14*/{8 + 6, new DataField{ Code="STS", Name="Subfield #14: Stand Status",FieldLength=1  } },
      /*FX*/{8 + 7, new DataField{ Code="FX2", Name="Extension indicator" } },
      //        16 + 0 1 2 3 4 5 6 7
      //xxxxxxxxxxBit 3, 8C -  ‭‭1 0 0 0 1 1 0 0‬
      /*15*/{16 + 0, new DataField{ Code="STD", Name="Subfield #15: Standard Instrument Departure",FieldLength=7 }},
      /*16*/{16 + 1, new DataField{ Code="STA", Name="Subfield #16: STandard Instrument ARrival",FieldLength=7 } },
      /*17*/{16 + 2, new DataField{ Code="PEM", Name="Subfield #17: Pre-emergency Mode 3/A code",FieldLength=2 } },
      /*18*/{16 + 3, new DataField{ Code="PEC", Name="Subfield #18: Pre-emergency Callsign",FieldLength=7  } },
      /*19*/{16 + 4, new DataField{ Code="BT4", Name="Spare bits set to zero",FieldLength=0 } },
      /*20*/{16 + 5, new DataField{ Code="BT5", Name="Spare bits set to zero",FieldLength=0 } },
      /*21*/{16 + 6, new DataField{ Code="BT6", Name="Spare bits set to zero",FieldLength=0 } },
      /*FX*/{16 + 7, new DataField{ Code="FX3", Name="Extension indicator"  } }

    };

  }
}
