open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation
open Meadow.Foundation.Graphics
open Meadow.Hardware
open Meadow.Foundation.Displays.TftSpi

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
    
    let config = 
        new SpiClockConfiguration(
            speedKHz = 6000L,
            mode = SpiClockConfiguration.Mode.Mode3)
    
    let display = 
        new St7789 (
            device = device,
            spiBus = device.CreateSpiBus(device.Pins.SCK, device.Pins.MOSI, device.Pins.MISO, config),
            chipSelectPin = device.Pins.D02,
            dcPin = device.Pins.D01,
            resetPin = device.Pins.D00,
            width = 240, height = 240)
    
    let graphics = GraphicsLibrary(display)

    do 
        led.SetColor RgbLed.Colors.Green


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0