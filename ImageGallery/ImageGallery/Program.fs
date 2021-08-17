open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation
open Meadow.Foundation.Displays.TftSpi
open SimpleJpegDecoder
open Meadow.Foundation.Sensors.Buttons
open Meadow.Hardware
open Meadow.Foundation.Graphics
open System.Reflection
open System.IO

let images = [| "image1.jpg"; "image2.jpg"; "image3.jpg" |]

let loadResource filename =

    let assembly = Assembly.GetExecutingAssembly()
    let resourceName = $"ImageGallery.{filename}"

    use stream = assembly.GetManifestResourceStream resourceName
    use ms = new MemoryStream()
    
    stream.CopyTo ms
    ms.ToArray()

let showJpeg (graphics : GraphicsLibrary) index =

    let jpgData = loadResource images.[index]    
    let decoder = new JpegDecoder()
    let jpg = decoder.DecodeJpeg(jpgData)

    Console.WriteLine $"Jpeg decoded is {jpg.Length} bytes"
    Console.WriteLine $"Width {decoder.Width}"
    Console.WriteLine $"Height {decoder.Height}"

    graphics.Clear()

    seq {
        for i in 0..jpg.Length-1 do
            let x = i % decoder.Width
            let y = i / decoder.Width
            yield! seq { x,y; x,y; x,y }
    }
    |> Seq.iteri (fun i (x,y) -> 
        if i % 3 = 0 && i < jpg.Length - 2 then
            let r = jpg.[i] |> Convert.ToInt32
            let g = jpg.[i + 1] |> Convert.ToInt32
            let b = jpg.[i + 2] |> Convert.ToInt32
            let color = Color.FromRgb(r, g, b)
            graphics.DrawPixel(x, y, color))

    graphics.Show()


type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let mutable selectedIndex = 0
    
    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do 
        Console.WriteLine("Initializing...")
        led.SetColor RgbLed.Colors.Red
    
    let buttonNext = new PushButton(device, device.Pins.D03)
    let buttonPrevious = new PushButton(device, device.Pins.D04)
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

    let _ =
        buttonNext.Clicked.Subscribe(
            fun _ ->
                led.SetColor RgbLed.Colors.Red

                if selectedIndex - 1 < 0 then do
                    selectedIndex <- 2
                else
                    selectedIndex <- selectedIndex - 1

                showJpeg graphics selectedIndex
                
                led.SetColor RgbLed.Colors.Green)
    
    let _ =
        buttonPrevious.Clicked.Subscribe(
            fun _ ->
                led.SetColor RgbLed.Colors.Red

                if selectedIndex + 1 > 2 then do
                    selectedIndex <- 0
                else
                    selectedIndex <- selectedIndex + 1
                
                showJpeg graphics selectedIndex
                
                led.SetColor RgbLed.Colors.Green)

    do 
        graphics.Rotation <- GraphicsLibrary.RotationType._270Degrees
        showJpeg graphics selectedIndex
        led.SetColor RgbLed.Colors.Green


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0