open System
open Meadow.Devices
open Meadow
open Meadow.Foundation
open Meadow.Foundation.Leds
open Meadow.Foundation.Servos
open Meadow.Foundation.Sensors.Rotary
open Meadow.Peripherals.Sensors
open Meadow.Units

let createRotaryObserver handleChange =
    RotaryEncoderWithButton
        .CreateObserver(
            (fun changeResult -> handleChange changeResult))    

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let mutable angle = 0;

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)

    do led.SetColor RgbLed.Colors.Red

    let servo = 
        Servo(device.CreatePwmPort device.Pins.D08, NamedServoConfigs.SG90)

    let rotaryEncoder =
        RotaryEncoder (device, device.Pins.D02, device.Pins.D03)

    let rotated (changeResult : IChangeResult<_>) = 
        
        match changeResult.New with
        | Rotary.RotationDirection.Clockwise -> 
            angle <- angle + 1
        | _ -> 
            angle <- angle - 1

        match angle with
        | a when a > 180 -> angle <- 180
        | a when a < 0 -> angle <- 0
        | _ -> ()
        
        servo.RotateTo (angle |> float |> Angle)

    let _ = createRotaryObserver rotated |> rotaryEncoder.Subscribe

    do led.SetColor RgbLed.Colors.Green


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0