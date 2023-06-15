﻿module ProgressBarFSharp

open System
open System.Threading

open TryWith.TryWith
open Messages.Messages


//TODO Errors :-)
let private (++) a = (+) a 1

let private cancellationTokenSource = //I
    new CancellationTokenSource() |> optionToSRTP (lazy (msgParam7 "ErrorPB1")) (new CancellationTokenSource())  

let private animateProgress () = //I

    let rec loop counter =
        Console.CursorLeft <- 0
        Console.Write("Těžce na tom pracuji " + new string('.', counter % 4)) //neslo to se sprintf
        System.Threading.Thread.Sleep(100) 

        match cancellationTokenSource.IsCancellationRequested with
        | true  ->
                  Console.CursorLeft <- 0
                  printfn "Nadřel jsem se, ale úkol jsem úspěšně dokončil :-)"                
        | false ->
                  let c = (++) counter 
                  loop c
    loop 0
    (*
    The given F# function is tail-recursive because the recursive call to loop is the last operation in the function. 
    The function does not perform any additional operations after the recursive call is made, which means that the function
    does not need to store any information on the call stack before making the recursive call. 
    Therefore, the function is tail-recursive and can be optimized by the F# compiler to avoid stack overflow errors.  
    *)

let progressBarIndeterminate (longRunningProcess: unit -> unit) = //I

    let myFunction x = 
        let progressThread = 
            new Thread(animateProgress) |> optionToSRTP (lazy (msgParam7 "ErrorPB2")) (new Thread(animateProgress)) 
        
        progressThread.Start() 

        let processThread = 
            new Thread(longRunningProcess) |> optionToSRTP (lazy (msgParam7 "ErrorPB3")) (new Thread(longRunningProcess)) 

        processThread.Start()
        processThread.Join() 
        cancellationTokenSource.Cancel() 
        progressThread.Join()

    tryWith myFunction (fun x -> ()) () String.Empty () |> deconstructor msgParam1

let private updateProgressBar (currentProgress : int) (totalProgress : int) : unit =
    
    let myFunction x = 

        let bytes = //437 je tzv. Extended ASCII  
            System.Text.Encoding.GetEncoding(437).GetBytes("█") |> optionToSRTP (lazy (msgParam7 "ErrorPB4")) Array.empty 
                   
        let output =
            System.Text.Encoding.GetEncoding(852).GetChars(bytes) |> optionToSRTP (lazy (msgParam7 "ErrorPB5")) Array.empty   
        
        let progressBar = 
            let barWidth = 50 //nastavit delku dle potreby            
            let percentComplete = (/) ((*) currentProgress 101) ((++) totalProgress) // :-) //101 proto, ze pri deleni 100 to po zaokrouhleni dalo jen 99%                    
            let barFill = (/) ((*) currentProgress barWidth) totalProgress // :-)  
               
            let characterToFill = string (Array.item 0 output) //moze byt baj "#"
            let bar = String.replicate barFill characterToFill |> optionToSRTP (lazy (msgParam7 "ErrorPB5")) String.Empty 
            let remaining = String.replicate (barWidth - (++) barFill) "*" |> optionToSRTP (lazy (msgParam7 "ErrorPB6")) String.Empty // :-)
              
            sprintf "<%s%s> %d%%" bar remaining percentComplete 

        match (=) currentProgress totalProgress with
        | true  -> msgParam8 progressBar
        | false -> msgParam9 progressBar

    tryWith myFunction (fun x -> ()) () String.Empty () |> deconstructor msgParam1

let progressBarContinuous (currentProgress : int) (totalProgress : int) : unit =

    match currentProgress < (totalProgress - 1) with
    | true  -> updateProgressBar currentProgress totalProgress
    | false -> 
               Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r")
               Console.CursorLeft <- 0             