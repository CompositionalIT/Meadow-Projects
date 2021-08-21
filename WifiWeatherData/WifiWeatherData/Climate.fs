module Climate

open Meadow
open Newtonsoft.Json
open System.Net.Http
open System.Text
open System.Threading.Tasks
open FSharp.Control.Tasks
open System
open Serialization

let climateDataUri = "http://192.168.50.97:2792/ClimateData"

type ClimateReading =
    { ID : Nullable<int64>
      TempC : Nullable<decimal>
      BarometricPressureMillibarHg : Nullable<decimal>
      RelativeHumdity : Nullable<decimal> }

let postTempReading (temp : Units.Temperature) = task {
    
    let reading =
        { ID = Nullable<int64>()
          TempC = Nullable<decimal>(decimal temp.Celsius)
          BarometricPressureMillibarHg = Nullable<decimal>()
          RelativeHumdity = Nullable<decimal>() }
    
    use client = new HttpClient() 
    client.Timeout <- TimeSpan(0, 5, 0);

    let json = JsonConvert.SerializeObject(reading, converters = [| OptionConverter() |])

    let! response = 
        client.PostAsync(
            climateDataUri, new StringContent(
                json, Encoding.UTF8, "application/json"))
    try 
        response.EnsureSuccessStatusCode() |> ignore
    with 
        | :? TaskCanceledException ->
            Console.WriteLine "Request time out."
        | e ->
            Console.WriteLine $"Request failed: {e.Message}"
}

let fetchTempReadings () = task {

    use client = new HttpClient() 
    client.Timeout <- TimeSpan(0, 5, 0);

    let! response = client.GetAsync(climateDataUri)
    try 
        response.EnsureSuccessStatusCode() |> ignore

        let! json = response.Content.ReadAsStringAsync()
        
        Console.WriteLine json
        
        let readings : ClimateReading[] = JsonConvert.DeserializeObject<ClimateReading[]>(json, converters = [| OptionConverter() |])
        
        Console.WriteLine "Deserialized to object"
        Console.WriteLine($"Temp: {readings.[0].TempC}")

        return readings
    with 
        | :? TaskCanceledException ->
            Console.WriteLine "Request time out."
            return Array.empty
        | e ->
            Console.WriteLine $"Request went sideways: {e.Message}"
            return Array.empty
}
    