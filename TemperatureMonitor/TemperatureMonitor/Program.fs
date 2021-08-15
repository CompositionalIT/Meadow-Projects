open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation
open Meadow.Foundation.Displays.TftSpi
open Meadow.Foundation.Sensors.Temperature
open Meadow.Hardware
open Meadow.Foundation.Graphics

let colors = [|
    Color.FromHex("#008500")
    Color.FromHex("#269926")
    Color.FromHex("#00CC00")
    Color.FromHex("#67E667") 
|]

let loadScreen (graphics : GraphicsLibrary) displayWidth displayHeight =
    Console.WriteLine("LoadScreen...");
    
    graphics.Clear()
    
    let mutable radius = 225;
    let mutable originX = displayWidth / 2
    let mutable originY = displayHeight / 2 + 130
    
    graphics.Stroke <- 3

    for i in 0..3 do
    
        graphics
            .DrawCircle(
                centerX = originX,
                centerY = originY,
                radius = radius,
                color = colors.[i],
                filled = true
            )
    
        graphics.Show();
        radius <- radius - 20;
    
    graphics.DrawLine(0, 220, 240, 220, Color.White)
    graphics.DrawLine(0, 230, 240, 230, Color.White)
    
    graphics.CurrentFont <- Font12x20();
    graphics.DrawText(54, 130, "TEMPERATURE", Color.White)
    
    graphics.Show()

let drawTemperature (graphics : GraphicsLibrary) (temperature : Meadow.Units.Temperature) =
    
    Console.WriteLine $"Temp: {temperature.Celsius} C"
    
    graphics.DrawRectangle(
        x = 48,
        y = 160,
        width = 144,
        height = 40,
        color = colors.[colors.Length - 1],
        filled = true)

    graphics.DrawText(
        x = 48, y = 160,
        text = "test",
        color = Color.White,
        scaleFactor = GraphicsLibrary.ScaleFactor.X2)

    graphics.Show()

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device
    
    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do led.SetColor RgbLed.Colors.Red
    
    let analogTemperature = 
        AnalogTemperature(
            device = device,
            analogPin = device.Pins.A00,
            sensorType  = AnalogTemperature.KnownSensorType.LM35)

    let config = 
        SpiClockConfiguration(
            6000L, 
            SpiClockConfiguration.Mode.Mode3)

    let st7789 = 
        new St7789 (
            device = device,
            spiBus = device.CreateSpiBus(device.Pins.SCK, device.Pins.MOSI, device.Pins.MISO, config),
            chipSelectPin = device.Pins.D02,
            dcPin = device.Pins.D01,
            resetPin = device.Pins.D00,
            width = 240, height = 240)

    let displayWidth = Convert.ToInt32(st7789.Width)
    let displayHeight = Convert.ToInt32(st7789.Height)
    let graphics = GraphicsLibrary st7789

    let _ = 
        analogTemperature
            .TemperatureUpdated
            .Subscribe (fun changeResult -> drawTemperature graphics changeResult.New)

    do 
        graphics.Rotation <- GraphicsLibrary.RotationType._270Degrees
        led.SetColor RgbLed.Colors.Green
        loadScreen graphics displayWidth displayHeight
        analogTemperature.StartUpdating(TimeSpan.FromSeconds(5.))

[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0