open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation.Graphics
open Meadow.Hardware
open Meadow.Foundation.Displays.TftSpi
open Meadow.Foundation.Sensors.Temperature

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
        led.SetColor RgbLed.Colors.Green


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0