open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Hardware
open System.Threading
open Meadow.Foundation.Sensors.Distance
open Meadow.Foundation.Audio

let mapLinearRange oldMin oldMax newMin newMax oldVal =
    ((oldVal - oldMin) / (oldMax - oldMin) ) * (newMax - newMin) + newMin

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let mutable beepIntervalMs = 0

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    do led.SetColor RgbLed.Colors.Red

    let speaker = 
        device.Pins.D09
        |> device.CreatePwmPort
        |> PiezoSpeaker
    do speaker.StopTone()

    let i2cBus = device.CreateI2cBus I2cBusSpeed.FastPlus
    let sensor = new Vl53l0x(device, i2cBus)

    let sensorObserver = 
        Vl53l0x
            .CreateObserver(
                (fun changeResult -> 
                    
                    let interval =
                        match changeResult.New.Centimeters with
                        | d when d <= 0. || d >= 70. -> 0
                        | d when  d > 0. && d <= 7. -> 62
                        | d -> mapLinearRange 7. 70. 62.5 1000. d |> int

                    beepIntervalMs <- interval
                    Console.WriteLine $"{changeResult.New.Centimeters}cm, {beepIntervalMs} interval"))
    
    let _ = sensor.Subscribe sensorObserver

    do 
        sensor.StartUpdating(TimeSpan.FromMilliseconds 200.)
        led.SetColor RgbLed.Colors.Green

        while true do
            if beepIntervalMs > 0 then
                speaker.PlayTone(440f, beepIntervalMs).Wait()
                Thread.Sleep beepIntervalMs
            else
                Thread.Sleep 500


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep System.Threading.Timeout.Infinite
    0