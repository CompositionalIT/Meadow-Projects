module Display

open Meadow.Devices
open Meadow.Foundation.Graphics
open Meadow.Hardware
open Meadow.Foundation.Displays.TftSpi
open System.Reflection
open System.IO
open SimpleJpegDecoder
open System
open Meadow.Foundation
open Meadow.Foundation.Sensors.Temperature
open Meadow

let loadResource filename =

    let assembly = Assembly.GetExecutingAssembly()
    let resourceName = $"WifiWeatherData.{filename}"

    use stream = assembly.GetManifestResourceStream resourceName
    use ms = new MemoryStream()
    
    stream.CopyTo ms
    ms.ToArray()

let displayJpeg (graphics : GraphicsLibrary) xStart yStart =

    let jpgData = loadResource "meadow.jpg"
    let decoder = new JpegDecoder()
    let jpg = decoder.DecodeJpeg(jpgData) |> Array.map Convert.ToInt32

    seq {
        for i in 0..jpg.Length-1 do
            let coords = 
                let x = i % decoder.Width
                let y = i / decoder.Width
                x + xStart, y + yStart
            yield! seq { coords; coords; coords }
    }
    |> Seq.iteri (fun i (x,y) -> 
        if i % 3 = 0 && i < jpg.Length - 2 then
            let r = jpg.[i]
            let g = jpg.[i + 1] 
            let b = jpg.[i + 2]
            let color = Color.FromRgb(r, g, b)
            graphics.DrawPixel(x, y, color))

    graphics.Show()

type Controller(device : F7Micro) =

    let mutable isRendering = false
    let renderLock = obj()

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
        graphics.CurrentFont <- Font12x20()
        graphics.Clear true

    member this.UpdateDisplay(temp : Units.Temperature) = 
        ()