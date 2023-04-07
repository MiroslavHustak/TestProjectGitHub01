module WebScraping1

open System
open System.IO
open System.Net
open FSharp.Data

open Fugit
open Helpers

open FSharp.Quotations
open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions

do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
Console.BackgroundColor <- ConsoleColor.Blue 
Console.ForegroundColor <- ConsoleColor.White 
Console.InputEncoding   <- System.Text.Encoding.Unicode
Console.OutputEncoding  <- System.Text.Encoding.Unicode

//************************Helpers**********************************************************************
//tu a tam zkontrolovat json, zdali KODIS nezmenil jeji strukturu 
let [<Literal>] pathJson = @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDTotal.json" //musi byt forward slash"

type KodisTimetables = JsonProvider<pathJson> 

let private pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\" 

let private errorFn str err = 
    str
    |> Option.ofObj
    |> function 
        | Some value -> value
        | None       -> //printfn "%s" err
                        String.Empty   

let private timeStr = errorFn "HH:mm:ss" "Error1"                     
    
let private processStart() =     
    //TryWith
    let processStartTime = errorFn $"Začátek procesu: {DateTime.Now.ToString(timeStr)}" "Error2"                           
    printfn "%s" processStartTime
    
let private processEnd() =     
    //TryWith
    let processEndTime = errorFn $"Konec procesu: {DateTime.Now.ToString(timeStr)}" "Error3"                       
    printfn "%s" processEndTime

let private client = 
    //TryWith
    new System.Net.Http.HttpClient() 
    |> Option.ofObj
    |> function 
        | Some value -> value
        | None       -> //printfn "%s" "Error4"
                        new System.Net.Http.HttpClient() //whatever 

let private split lst =
    let num = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\/" //50 znaku
    let folder (a: string*string, b: string*string) (cur, acc) = 
        match a with
        | _ when (snd a).Substring(num.Length, 3) = (snd b).Substring(num.Length, 3) -> a::cur, acc
        | _                                                                          -> [a], cur::acc

    let result = List.foldBack folder (List.pairwise lst) ([List.last lst], []) 
    (fst result)::(snd result)
    
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =
    //TryWith   
    try
        async
            {
                let! stream = client.GetStreamAsync(uri) 
                              |> Async.AwaitTask 
                let fileStream = new FileStream(path, FileMode.CreateNew)
                let! responseBody = stream.CopyToAsync(fileStream)
                                    |> Async.AwaitTask 
                return responseBody
            } 
    with
    | _ -> printfn "laksjdhflkajshdflk"
           Async.Sleep 0  


//************************main code***********************************************************

let [<Literal>] pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"

let private jsonLinkList = 
    [
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Český%20Těšín&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Frýdek-Místek&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Havířov&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Karviná&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Krnov&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Nový%20Jičín&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Opava&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Orlová&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Studénka&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Třinec&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD%20MHD&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&group_in%5B1%5D=232-293&group_in%5B2%5D=331-392&group_in%5B3%5D=440-465&group_in%5B4%5D=531-583&group_in%5B5%5D=613-699&group_in%5B6%5D=731-788&group_in%5B7%5D=811-885&group_in%5B8%5D=901-990&group_in%5B9%5D=NAD&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=232-293&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=331-392&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=440-465&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=531-583&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=613-699&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=731-788&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=811-885&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=901-990&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=S1-S34&group_in%5B1%5D=R8-R61&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=S1-S34&_sort=numeric_label"
        sprintf"%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=R8-R61&_sort=numeric_label"         
    ]

let private pathToJsonList = 
   
    [
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDTotal.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDBruntal.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDCT.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDFM.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDHavirov.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDKarvina.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDBKrnov.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDNJ.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDOpava.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDOrlova.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDOstrava.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDStudenka.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDTrinec.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDNAD.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegionTotal.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion75.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion200.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion300.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion400.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion500.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion600.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion700.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion800.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegion900.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisRegionNAD.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisTrainTotal.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisTrainPomaliky.json"
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisTrainTSpesakyARychliky.json"                
    ]

let private saveUpdatedJson() = 
   
    //TryWithjsonLinkList
    let loadedJsonFiles = 
        jsonLinkList |> List.map (fun item -> async { return! client.GetStringAsync(item) |> Async.AwaitTask } |> Async.RunSynchronously)        
        
    (pathToJsonList, loadedJsonFiles)
    ||> List.iter2 (fun path json -> 
                                    use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                    streamWriter.WriteLine(json)     
                                    streamWriter.Flush()   
                    )      

let private myLinksSet() = 
    //TryWith
    let kodisTimetables() = 

        pathToJsonList |> Array.ofList 
        |> Array.collect (fun pathToJson -> 
        
                                        //TryWith 
                                    let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj
                                    //let kodisJsonSamples = kodisJsonSamples.GetSamples() |> Option.ofObj                 
                
                                    kodisJsonSamples
                                    |> function 
                                        | Some value -> value |> Array.map (fun item -> item.Timetable) 
                                        | None       -> printfn "%s" "Error5"
                                                        Array.empty    
                         ) 
   
        (*
        //ponechavam pro pochopeni struktury u json type provider (pri pouziti option se to tahne az k susedovi)
        let kodisAttachments() = kodisJsonSamples                              
                                 |> Array.collect (fun item ->                                            
                                                             item.Vyluky 
                                                             |> Array.collect (fun item ->                                                 
                                                                                         item.Attachments
                                                                                         |> Array.Parallel.map (fun item -> item.Url )
                                                                               ) 
                                                  )   
        *)  

    let kodisAttachments() = 

        pathToJsonList |> Array.ofList 
        |> Array.collect (fun pathToJson ->  
                                            let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj 

                                            let fn1 (value: JsonProvider<pathJson>.Attachment array) =
                                                value
                                                |> Array.Parallel.map (fun item -> errorFn item.Url "Error7")

                                            let fn2 (item: JsonProvider<pathJson>.Vyluky) =        
                                                item.Attachments |> Option.ofObj        
                                                |> function 
                                                    | Some value -> value |> fn1
                                                    | None       -> printfn "%s" "Error6"
                                                                    Array.empty                 

                                            let fn3 (item: JsonProvider<pathJson>.Root) =  
                                                item.Vyluky |> Option.ofObj
                                                |> function 
                                                    | Some value -> value |> Array.collect fn2 
                                                    | None       -> printfn "%s" "Error6"
                                                                    Array.empty 
           
                                            kodisJsonSamples 
                                            |> function 
                                                | Some value -> value |> Array.collect fn3 
                                                | None       -> printfn "%s" "Error6"
                                                                Array.empty                                 
                         ) 

    (Array.append <| kodisAttachments() <| kodisTimetables()) |> Set.ofArray //konverzi na Set vyhodime stejne polozky  
        

let private sortTimetables myLinksSet = 

    //****************filtrace odkazu na neplatne jizdni rady***********************

    let currentTime = Fugit.now()
    
    let myList = 
        myLinksSet |> Set.toArray 
        |> Array.Parallel.map (fun (item: string) ->                                    
                                            let fileName =                                     
                                                match item.Contains("timetables") with
                                                | true  -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty).Replace("timetables", String.Empty)
                                                           s.Substring(0, s.Length - 4).Remove(0, 2)                                                 
                                                | false -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty)
                                                           s.Substring(0, s.Length - 15).Remove(0, 1)

                                            let range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
                                            let charList = fileName.ToCharArray() |> Array.toList |> List.take 3

                                            let aux i range  = range |> List.filter (fun item -> charList |> List.item i = item)

                                            let link, fileName = 
                                                let fileNameFull = 
                                                    match aux 0 range <> List.empty with
                                                    | true  -> match aux 1 range <> List.empty with
                                                                | true  -> match aux 2 range <> List.empty with
                                                                            | true  -> fileName                                                                     
                                                                            | false -> sprintf "%s%s" "0" fileName                    
                                                                | false -> sprintf "%s%s" "00" fileName
                                                    | false -> fileName  
                                   
                                                //printfn"%s" fileNameFull

                                                //let substring = fileName.Substring(startIndex, length)

                                                match not (fileNameFull |> String.length >= 25) with
                                                | true  -> String.Empty, String.Empty
                                                | false ->                                     
                                                        let lineNumberStr = fileNameFull.Substring(0, 3)
                                                        let lineNumberInt = Parsing.parseMe(lineNumberStr)

                                                        let yearOld = Parsing.parseMe(fileNameFull.Substring(4, 4))
                                                        let monthOld = Parsing.parseMe(fileNameFull.Substring(9, 2))
                                                        let dayOld = Parsing.parseMe(fileNameFull.Substring(12, 2))
                                                        let yearNew = Parsing.parseMe(fileNameFull.Substring(15, 4))
                                                        let monthNew = Parsing.parseMe(fileNameFull.Substring(20, 2))
                                                        let dayNew = Parsing.parseMe(fileNameFull.Substring(23, 2))
                                   
                                                        //printfn"%i-%i-%i-----%i-%i-%i" yearOld monthOld dayOld yearNew monthNew dayNew 

                                                        let aux = [ yearOld; monthOld; dayOld; yearNew; monthNew; dayNew ]

                                                        match aux |> List.contains -1 with
                                                        | true  -> item, fileNameFull
                                                        | false -> 
                                                                    try
                                                                        let dateOld = new DateTime(yearOld, monthOld, dayOld)
                                                                        let dateNew = new DateTime(yearNew, monthNew, dayNew)                                   

                                                                    //printfn"%s" lineNumberStr

                                                                 
                                                                        //let cond1 = lineNumberInt < 999 && lineNumberInt > 0 //vytridime castecne pouze linky 1 az 999
                                                                        let cond2 = ((currentTime |> Fugit.isAfter dateNew)
                                                                                    &&
                                                                                    (currentTime |> Fugit.isAfter dateOld))
                                                                                    || 
                                                                                    (currentTime |> Fugit.isAfter dateNew)
                                                          
                                                                        //match cond1 with
                                                                        // | false -> item, fileNameFull                                               
                                                                        //| true  ->
                                                                        match cond2 with
                                                                        | false -> item, fileNameFull                                               
                                                                        | true  -> String.Empty, String.Empty    
                                                                    with 
                                                                    | _ -> String.Empty, String.Empty              

                                            let pathToFile = 
                                                match link = String.Empty with
                                                | true  -> String.Empty
                                                | false -> sprintf"%s/%s%s" pathToDir fileName ".pdf"
                                            link, pathToFile                                 
                    ) 
    
        |> Set.ofArray |> Set.toList |> List.sort 

    let myList = 
        match myList |> List.contains (String.Empty, String.Empty) with
        | true  -> myList |> List.tail
        | false -> myList
    
    let myList = 
        myList
        |>
        split 
        |> Array.ofList
        |> Array.collect (fun item ->  
                                match item.Length > 1 with
                                | false -> item |> Array.ofList
                                | true  -> item
                                            |> Array.ofList
                                            |> Array.map (fun (link, pathToFile) ->
    
                                                                                let yearOld = Parsing.parseMe(pathToFile.Substring(53, 4))
                                                                                let monthOld = Parsing.parseMe(pathToFile.Substring(58, 2))
                                                                                let dayOld = Parsing.parseMe(pathToFile.Substring(61, 2))
    
                                                                                let aux = [ yearOld; monthOld; dayOld ]
    
                                                                                match aux |> List.contains -1 with
                                                                                | true  -> String.Empty, String.Empty
                                                                                | false -> 
                                                                                            try
                                                                                                   
                                                                                                let dateOld = new DateTime(yearOld, monthOld, dayOld)
                                                                                                let dateProsinec = new DateTime(2022, 12, 11)                                                                                               
                                                                                                    
                                                                                                match not <| dateOld.Equals(dateProsinec) with
                                                                                                | true  -> match pathToFile.Contains("_v") with 
                                                                                                            | true  -> link, pathToFile  
                                                                                                            | false -> String.Empty, String.Empty                                                                                                                                                        
                                                                                                | false -> link, pathToFile
                                                                                                                
                                                                                            with 
                                                                                            | _ -> String.Empty, String.Empty  
    
                                )        
                            ) |> Set.ofArray |> Set.toList |> List.distinctBy snd |> List.sort        
    
    match myList |> List.contains (String.Empty, String.Empty) with
    | true  -> myList |> List.tail
    | false -> myList

let private downloadTimetables pathToDir (sortTimetables: (string*string) list) =
    
    //TryWith
   
    let dirInfo = new DirectoryInfo(pathToDir)
                     |> Option.ofObj
                     |> function 
                         | Some value -> value  
                         | None       -> printfn "%s" "Error8"
                                         new DirectoryInfo(pathToDir)

    //dirInfo.EnumerateFiles() |> Array.ofSeq |> Array.Parallel.iter (fun item -> item.Delete()) //smazeme stare soubory v adresari 

    //vlastni download pdf souboru
      
    sortTimetables 
    |> List.iter (fun (link, pathToFile) -> 
                                            //TryWith
                                            async 
                                                {  
                                                    //let! responseBody =  //Async.Sleep 0
                                                    //printfn"%s" link
                                                    //printfn"%s" pathToFile
                                                    
                                                           //Parallel vyhazuje chyby                                           
                                                    //return! downloadFileTaskAsync client link pathToFile 
                                                    return! Async.Sleep 0
                                                }  
                                            |> Async.RunSynchronously
                                          
                 )
    printfn"sortTimetables %i" sortTimetables.Length

    
         
let webscraping1() =
    processStart >> saveUpdatedJson >> myLinksSet >> sortTimetables >> (downloadTimetables pathToDir) >> processEnd >> client.Dispose  
    //processStart() |> myLinksSet |> sortTimetables |> downloadTimetables pathToDir |> processEnd |> client.Dispose  