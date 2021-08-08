open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Sensors.Buttons
open Meadow.Foundation.Displays.Lcd
open System.Threading

let writeLine (display : CharacterDisplay) message lineNumber =
    display.WriteLine(message, byte lineNumber)

let characterDisplayClock (display : CharacterDisplay) =
    display.ClearLines()
    let writeToDisplay = writeLine display
    while true do
        let clock = DateTime.Now;
        writeToDisplay "Welcome to Meadow!" 1
        writeToDisplay $"{clock:MM}/{clock:dd}/{clock:yyyy}" 2
        writeToDisplay $"{clock:hh}:{clock:mm}:{clock:ss} {clock:tt}" 3
        Thread.Sleep 1000
    
type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
    let device = MeadowApp.Device

    let hourButton = new PushButton(device, device.Pins.D15)
    let minuteButton = new PushButton(device, device.Pins.D12)
    let display = 
        CharacterDisplay
            ( device,
              device.Pins.D10,
              device.Pins.D09,
              device.Pins.D08,
              device.Pins.D07,
              device.Pins.D06,
              device.Pins.D05)

    let _ =
        hourButton.Clicked.Subscribe(
            fun _ -> device.SetClock(DateTime.Now.AddHours 1.))

    let _ =
        minuteButton.Clicked.Subscribe(
            fun _ -> device.SetClock(DateTime.Now.AddMinutes 1.))

    do 
        device.SetClock(DateTime(2020, 04, 01, 11, 00, 00))
        characterDisplayClock display
















[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0