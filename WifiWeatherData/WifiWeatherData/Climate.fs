module Climate

open Meadow
open System.Text.Json
open System
open System.Net.Http
open System.Text
open System.Threading.Tasks

let climateDataUri = "http://localhost:2792/ClimateData";

type ClimateReading =
    { ID : Nullable<int64>
      TempC : Nullable<decimal>
      BarometricPressureMillibarHg : Nullable<decimal>
      RelativeHumdity : Nullable<decimal> }

let postTempReading (temp : Units.Temperature) = async {
    
    let reading =
        { ID = Nullable<int64>()
          TempC = Nullable<decimal>(decimal temp.Celsius)
          BarometricPressureMillibarHg = Nullable<decimal>()
          RelativeHumdity = Nullable<decimal>() }
    
    use client = new HttpClient() 
    client.Timeout <- TimeSpan(0, 5, 0);

    let json = JsonSerializer.Serialize reading

    let! response = 
        client.PostAsync(
            climateDataUri, new StringContent(
                json, Encoding.UTF8, "application/json")) |> Async.AwaitTask
    try 
        response.EnsureSuccessStatusCode() |> ignore
    with 
        | :? TaskCanceledException ->
            Console.WriteLine "Request time out."
        | e ->
            Console.WriteLine $"Request failed: {e.Message}"
}

let fetchTempReadings () = async {

    use client = new HttpClient() 
    client.Timeout <- TimeSpan(0, 5, 0);

    let! response = client.GetAsync(climateDataUri) |> Async.AwaitTask
    try 
        response.EnsureSuccessStatusCode() |> ignore

        let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        
        Console.WriteLine json
        
        let readings : ClimateReading[] = Array.empty //JsonSerializer.Deserialize<ClimateReading[]> json
        
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
    