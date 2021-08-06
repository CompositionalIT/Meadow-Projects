open System
open Meadow.Devices
open Meadow
open Meadow.Foundation.Leds
open Meadow.Foundation

type MeadowApp() =
    inherit App<F7Micro, MeadowApp>()
        
    do Console.WriteLine "Meadow running!"










[<EntryPoint>]
let main argv =
    let app = new MeadowApp()
    Threading.Thread.Sleep (System.Threading.Timeout.Infinite)
    0