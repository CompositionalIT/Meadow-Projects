module Climate

open Meadow

let climateDataUri = "http://localhost:2792/ClimateData";

type ClimateReading =
    { ID : int64
      TempC : decimal
      BarometricPressureMillibarHg : decimal
      RelativeHumdity : decimal }

let postTempReading (temp : Units.Temperature) = async {
    ()
}

let fetchTempReadings () = async {
    return List.empty
}
    