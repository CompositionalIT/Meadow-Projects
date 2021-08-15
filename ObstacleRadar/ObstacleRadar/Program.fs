open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation
open Meadow.Hardware
//open Meadow.Foundation.Sensors.Distance.Test
open Meadow.Foundation.Servos
open System.Threading


type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    do Console.WriteLine "Meadow alive!"
    //let mutable angle = 160.
    //let mutable increment = 4.

    let led = 
        RgbLed(
            device,
            device.Pins.OnboardLedRed,
            device.Pins.OnboardLedGreen,
            device.Pins.OnboardLedBlue)
    
    //do led.SetColor RgbLed.Colors.Red

    //let i2cBus = device.CreateI2cBus(I2cBusSpeed.FastPlus)
    //let sensor = new Vl53l0x(device, i2cBus)

    do led.SetColor RgbLed.Colors.Green

    //let servo = Servo (device.CreatePwmPort(device.Pins.D05), NamedServoConfigs.SG90)
    
    //do 
    //    sensor.StartUpdating(TimeSpan.FromMilliseconds 200.);
    //    servo.RotateTo(Units.Angle 0.);
    //    led.SetColor RgbLed.Colors.Green

    //    while true do
    //        match angle with
    //        | a when a >= 180. -> increment <- -4.
    //        | a when a <= 0. -> increment <- 4.
    //        | _ -> ()

    //        angle <- angle + increment

    //        servo.RotateTo (Units.Angle (angle, Meadow.Units.Angle.UnitType.Degrees))
        //Console.WriteLine $"{sensor.Distance}"
        //Thread.Sleep(500)
            


[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0 