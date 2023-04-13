module ProgressBarFSharp

open System
open System.Threading

let private cancellationTokenSource = new CancellationTokenSource()

let rec private animateProgress () =
    let rec loop counter =
        Console.CursorLeft <- 0
        Console.Write("Tezce na tom pracuji " + new string('.', counter % 4)) //neslo to se sprintf
        System.Threading.Thread.Sleep(100)

        match cancellationTokenSource.IsCancellationRequested with
        | true ->
                Console.CursorLeft <- 0
                printfn "Nadrel jsem se, ale ukol jsem uspesne dokoncil :-)"
                ()
        | false -> loop (counter + 1)
    loop 0

let progressBarImmediate (longRunningProcess: unit -> unit) =  
    let progressThread = new Thread(animateProgress)
    progressThread.Start()

    let processThread = new Thread(longRunningProcess)
    processThread.Start()

    processThread.Join()
    cancellationTokenSource.Cancel()
    progressThread.Join()

let private updateProgressBar (currentProgress : int) (totalProgress : int) : unit =

    let bytes = System.Text.Encoding.GetEncoding(437).GetBytes("█");//437 je tzv. Extended ASCII
    let output = System.Text.Encoding.GetEncoding(852).GetChars(bytes);

    let barWidth = 50
    let percentComplete = currentProgress * 100 / totalProgress + 1
    let barFill = currentProgress * barWidth / totalProgress
    //let characterToFill = "#"
    let characterToFill = string (Array.item 0 output)
    let bar = String.replicate barFill characterToFill
    let remaining = String.replicate (barWidth - barFill - 1) "*"
    let progressBar = sprintf "<%s%s> %d%%" bar remaining percentComplete

    match currentProgress = totalProgress with
    | true  -> printfn "%s\n" progressBar
    | false -> printf "%s\r" progressBar

let progressBarContinuous (currentProgress : int) (totalProgress : int) : unit =
    match currentProgress <= totalProgress with
    | true  -> updateProgressBar currentProgress totalProgress
    | false -> Console.CursorLeft <- 0
               ()