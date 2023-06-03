module WebScraping1_Helpers

open System
open TryWith.TryWith

let consoleAppProblemFixer() =
    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode

//***************************************************************************************************************

let xor a b = (a && not b) || (not a && b) //P

let errorStr str err = str |> (optionToSRTP err String.Empty) //AP                            

let private timeStr = errorStr "HH:mm:ss" "Error1" //AP                    
    
let processStart() =    //I 
    let processStartTime x =    //AP
        let processStartTime = errorStr (sprintf"Zacatek procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error2"                           
        printfn "%s" processStartTime
    tryWith processStartTime (fun x -> ()) () String.Empty () |> deconstructor
    
let processEnd() =    //I 
    let processEndTime x =    //AP
        let processEndTime = errorStr (sprintf"Konec procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error3"                       
        printfn "%s" processEndTime
    tryWith processEndTime (fun x -> ()) () String.Empty () |> deconstructor

let client() =  //I 
    let myClient x = new System.Net.Http.HttpClient() |> (optionToSRTP "Error4" (new System.Net.Http.HttpClient()))         
    tryWith myClient (fun x -> ()) () String.Empty (new System.Net.Http.HttpClient()) |> deconstructor

