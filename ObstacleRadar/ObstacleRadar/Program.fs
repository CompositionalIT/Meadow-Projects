open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation
open Meadow.Hardware
open Meadow.Foundation.Sensors.Distance
open Meadow.Foundation.Servos
open System.Threading
open Meadow.Foundation.Displays.TftSpi
open Meadow.Foundation.Graphics

let xCentre = 120
let yCentre = 170
let radarColor = Color.LawnGreen

let drawRadar (graphics : GraphicsLibrary) =

    [1..4]
    |> List.iter (fun i -> 
        graphics.DrawCircleQuadrant(xCentre, yCentre, 25 * i, 0, radarColor)
        graphics.DrawCircleQuadrant(xCentre, yCentre, 25 * i, 1, radarColor))

    [0..6]
    |> List.iter (fun i -> 
        graphics.DrawLine(
            xCentre,
            yCentre, 
            105, 
            float32 (float i * Math.PI / 6.), 
            radarColor))


type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let mutable angle = 160.
    let mutable increment = 4.
    let radarData : float32[] = Array.create 181 0f

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do led.SetColor RgbLed.Colors.Red

    let config = 
        new SpiClockConfiguration(
            speedKHz = 6000L,
            mode = SpiClockConfiguration.Mode.Mode3)
    
    let display = 
        new St7789(
            device = device,
            spiBus = device.CreateSpiBus(device.Pins.SCK, device.Pins.MOSI, device.Pins.MISO, config),
            chipSelectPin = device.Pins.D02,
            dcPin = device.Pins.D01,
            resetPin = device.Pins.D00,
            width = 240, height = 240)
    
    let graphics = GraphicsLibrary(display)

    let servo = Servo (device.CreatePwmPort(device.Pins.D05), NamedServoConfigs.SG90)
    
    let i2cBus = device.CreateI2cBus(I2cBusSpeed.FastPlus)
    let sensor = new Vl53l0x(device, i2cBus)
    let sensorObserver = 
        Vl53l0x
            .CreateObserver(
                (fun changeResult -> 
                    let distancemm = float32 changeResult.New.Millimeters

                    graphics.DrawText(
                        170,
                        0,
                        $"{distancemm}mm", 
                        Color.Yellow)

                    radarData.[int angle] <- distancemm / 2f))
    
    let _ = sensor.Subscribe sensorObserver

    do 
        graphics.CurrentFont <- Font12x20()
        graphics.Rotation <- GraphicsLibrary.RotationType._270Degrees
        sensor.StartUpdating(TimeSpan.FromMilliseconds 200.)
        servo.RotateTo(Units.Angle 0.)
        led.SetColor RgbLed.Colors.Green

        while true do

            graphics.Clear()
            drawRadar graphics

            match angle with
            | a when a >= 180. -> increment <- -4.
            | a when a <= 0. -> increment <- 4.
            | _ -> ()

            angle <- angle + increment

            servo.RotateTo (Units.Angle (angle, Meadow.Units.Angle.UnitType.Degrees))
            graphics.DrawText(0, 0, $"{180. - angle} degrees", Color.Yellow)

            [0..180]
            |> List.iter (fun i -> 
                let x = 120 + int (radarData.[i] * MathF.Cos(float32 i * MathF.PI / 180f))
                let y = 170 - int (radarData.[i] * MathF.Sin(float32 i * MathF.PI / 180f))
                graphics.DrawCircle(x, y, 2, Color.Yellow, true))

            graphics.Show()
            Thread.Sleep 500
            

[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0 