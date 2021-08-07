open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation.ICs.IOExpanders
open Meadow.Hardware
open Meadow.Foundation.Sensors.Rotary
open Meadow.Peripherals.Sensors

let createRotaryObserver handleChange =
    RotaryEncoderWithButton
        .CreateObserver(
            (fun changeResult -> handleChange changeResult))   

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let mutable percentage = 0f

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do led.SetColor RgbLed.Colors.Red

    let shiftRegister = x74595 (device, device.CreateSpiBus(), device.Pins.D00, 8)
    
    let ports = [|
        device.CreateDigitalOutputPort device.Pins.D14
        device.CreateDigitalOutputPort device.Pins.D15
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP0, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP1, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP2, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP3, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP4, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP5, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP6, false, OutputType.PushPull)
        shiftRegister.CreateDigitalOutputPort(
            shiftRegister.Pins.GP7, false, OutputType.PushPull)
    |]

    let ledBarGraph = LedBarGraph ports
    let rotaryEncoder =
        RotaryEncoder (device, device.Pins.D02, device.Pins.D03)

    let rotated (changeResult : IChangeResult<_>) = 
        
        match changeResult.New with
        | Rotary.RotationDirection.Clockwise -> 
            percentage <- percentage + 0.05f
        | _ -> 
            percentage <- percentage - 0.05f
        
        match percentage with
        | p when p > 1f -> percentage <- 1f
        | p when p < 0f -> percentage <- 0f
        | _ -> ()

        ledBarGraph.Percentage <- percentage
    
    let _ = createRotaryObserver rotated |> rotaryEncoder.Subscribe
    
    do 
        shiftRegister.Clear()
        led.SetColor RgbLed.Colors.Green


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0