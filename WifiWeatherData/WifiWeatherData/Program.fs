open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation.Sensors.Temperature
open Meadow.Gateway.WiFi

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

            let readings = Climate.fetchTempReadings().Result
            Console.WriteLine($"Temp: {readings.[0].TempC}")

            Climate.postTempReading(temp).Result
            
            let readings = Climate.fetchTempReadings().Result
            Console.WriteLine($"Temp: {readings.[0].TempC}")

            Console.WriteLine("App finished running.")

            
[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0