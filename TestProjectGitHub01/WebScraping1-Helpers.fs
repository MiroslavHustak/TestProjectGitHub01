module WebScraping1_Helpers

open System
open System.Reflection

open Microsoft.FSharp.Reflection

open Settings
open TryWith.TryWith
open Messages.Messages
open ErrorFunctions.ErrorFunctions

let consoleAppProblemFixer() =
    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode

//***************************************************************************************************************

let xor a b = (a && not b) || (not a && b) //P

let errorStr str err = str |> (optionToSRTP <| lazy (msgParam7 err) <| String.Empty) //P                            

let private timeStr = errorStr "HH:mm:ss" "Error1" //P  
    
let processStart() =    //I 

    let processStartTime x =    //I
        let processStartTime = errorStr (sprintf"Začátek procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error2"                           
        msgParam7 processStartTime
    tryWith processStartTime (fun x -> ()) () String.Empty () |> deconstructor msgParam1
    
let processEnd() =    //I 

    let processEndTime x =    //I
        let processEndTime = errorStr (sprintf"Konec procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error3"                       
        msgParam7 processEndTime
    tryWith processEndTime (fun x -> ()) () String.Empty () |> deconstructor msgParam1

let client() =  //I 

    let myClient x = new System.Net.Http.HttpClient() |> (optionToSRTP <| lazy (msgParam7 "Error4") <| (new System.Net.Http.HttpClient()))         
    tryWith myClient (fun x -> ()) () String.Empty (new System.Net.Http.HttpClient()) |> deconstructor msgParam1

let getDefaultRecordValues (t: Type) (r: ODIS) itemNo = //P //record -> Array //open FSharp.Reflection
   
    //dostanu pole hodnot typu PropertyInfo
    FSharpType.GetRecordFields(t) 
    |> Array.map (fun (prop: PropertyInfo) -> prop.GetGetMethod().Invoke(r, [||]) :?> string)            
    |> Array.take itemNo //jen prvni 4 polozky jsou pro celo-KODIS variantu

