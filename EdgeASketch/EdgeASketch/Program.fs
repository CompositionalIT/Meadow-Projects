open System
open System.Threading
open Meadow.Devices
open Meadow
open Meadow.Hardware
open Meadow.Foundation
open Meadow.Foundation.Displays.TftSpi
open Meadow.Foundation.Graphics
open Meadow.Foundation.Sensors.Rotary
open Meadow.Peripherals.Sensors.Rotary

let random = System.Random()

let randomColor () =
    let r, g, b =
        random.Next(0, 256),
        random.Next(0, 256),
        random.Next(0, 256)
    Color.FromRgb (r, g, b)

let initialiseGraphics (graphics : GraphicsLibrary) x y =
    graphics.Clear true
    graphics.DrawRectangle (0, 0, 240, 240, Color.White, true)
    graphics.DrawPixel (x, y, Color.Red)
    graphics.Show ()

let updateParameter (changeResult : IChangeResult<RotationDirection>) oldParam =
    
    let newParamValue =
        match changeResult.New with
        | RotationDirection.Clockwise -> oldParam + 1
        | _ -> oldParam - 1

    match newParamValue with
    | p when p > 239 -> 239
    | p when p < 0 -> 0
    | _ -> newParamValue

let createRotaryObserver handleChange =
    RotaryEncoderWithButton
        .CreateObserver(
            (fun changeResult -> handleChange changeResult))    

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    // Device state
    let mutable x = 120
    let mutable y = 120
    let mutable colour = Color.Red

    // Setup hardware connections
    let config = 
        SpiClockConfiguration(
            speedKHz = 6000L,
            mode = SpiClockConfiguration.Mode.Mode3)

    let spiBus =
        device.CreateSpiBus(
            clock = device.Pins.SCK,
            copi = device.Pins.MOSI,
            cipo = device.Pins.MISO,
            config = config)

    let st7789 = 
        new St7789(
            device = device,
            spiBus = spiBus,
            chipSelectPin = null,
            dcPin = device.Pins.D01,
            resetPin = device.Pins.D00,
            width = 240, 
            height = 240)
    
    let graphics = GraphicsLibrary st7789

    let rotaryX = 
        RotaryEncoderWithButton(
            device,
            device.Pins.A00,
            device.Pins.A01,
            device.Pins.A02)

    let rotaryY = 
        RotaryEncoderWithButton(
            device,
            device.Pins.D02,
            device.Pins.D03,
            device.Pins.D04)
    
    // Subscribe to rotary encoder change events
    let xRotated changeResult = 
        x <- updateParameter changeResult x
        graphics.DrawPixel (x, y + 1, colour)
        graphics.DrawPixel (x, y, colour)
        graphics.DrawPixel (x, y - 1, colour)
        graphics.Show ()

    let yChanged changeResult = 
        y <- updateParameter changeResult y
        graphics.DrawPixel (x + 1, y, colour)
        graphics.DrawPixel (x, y, colour)
        graphics.DrawPixel (x - 1, y, colour)
        graphics.Show ()

    let xClicked _ = 
        colour <- randomColor ()
    
    let yClicked _ =
        x <- 120
        y <- 120
        initialiseGraphics graphics x y

    let _ = xClicked |> rotaryX.Clicked.Subscribe 
    let _ = yClicked |> rotaryY.Clicked.Subscribe 
    let _ = createRotaryObserver xRotated |> rotaryX.Subscribe
    let _ = createRotaryObserver yChanged |> rotaryY.Subscribe

    // Prepare screen
    do initialiseGraphics graphics x y



[<EntryPoint>]
let main argv =
    let app = MeadowApp()
    Thread.Sleep (System.Threading.Timeout.Infinite)
    0