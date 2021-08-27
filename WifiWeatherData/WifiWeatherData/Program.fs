open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation.Sensors.Temperature
open Meadow.Gateway.WiFi
open Climate

let printFirstReading (readings : ClimateReading []) =
    Seq.tryHead readings
    |> Option.map (fun r -> 
        match r.TempC with
        | Some reading -> $"Temp: {reading}"
        | None -> "No temperature provided in climate reading")
    |> Option.defaultValue "No readings returned from server"
    |> Console.WriteLine

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do 
        Console.WriteLine("Initializing...")
        led.SetColor RgbLed.Colors.Red
    
    let displayController = Display.Controller device

    let analogTemperature = 
        AnalogTemperature(
            device,
            device.Pins.A00,
            AnalogTemperature.KnownSensorType.LM35)

    do 
        led.SetColor RgbLed.Colors.Blue
        device.InitWiFiAdapter().Wait()

        let result = 
            device.WiFiAdapter.Connect(
                Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).Result

        if result.ConnectionStatus <> ConnectionStatus.Success then
            led.SetColor(RgbLed.Colors.Magenta);
            failwith $"Cannot connect to network: {result.ConnectionStatus}"
        else
            led.SetColor RgbLed.Colors.Green

            let temp = analogTemperature.Read().Result
            displayController.UpdateDisplay temp

            Climate.fetchTempReadings().Result |> printFirstReading
            Climate.postTempReading(temp).Result
            Climate.fetchTempReadings().Result |> printFirstReading

            Console.WriteLine("App finished running.")

            
[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0