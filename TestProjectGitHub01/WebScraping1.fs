module WebScraping1

open System
open System.IO
open System.Net
open FSharp.Data

open Fugit

open Helpers

do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
Console.BackgroundColor <- ConsoleColor.Blue 
Console.ForegroundColor <- ConsoleColor.White 
Console.InputEncoding   <- System.Text.Encoding.Unicode
Console.OutputEncoding  <- System.Text.Encoding.Unicode

//************************Constants and types**********************************************************************
//tu a tam zkontrolovat json, zdali KODIS nezmenil jeho strukturu 
let [<Literal>] pathJson = @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDTotal.json" //musi byt forward slash"
let [<Literal>] pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"

let private pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\" 
let private range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]

type KodisTimetables = JsonProvider<pathJson> 


//************************Helpers**********************************************************************

let private errorFn str err = 
    str
    |> Option.ofObj
    |> function 
        | Some value -> value
        | None       -> printfn "%s" err
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
        | None       -> printfn "%s" "Error4"
                        new System.Net.Http.HttpClient() //whatever 

let private split list =
    let num = sprintf"%s%s" pathToDir @"/" 
    let folder (a: string*string, b: string*string) (cur, acc) = 
        //tryWith
        let cond = (snd a).Substring(num.Length, 3) = (snd b).Substring(num.Length, 3)
        match a with
        | _ when cond -> a::cur, acc
        | _           -> [a], cur::acc

    let result = List.foldBack folder (List.pairwise list) ([List.last list], []) 
    (fst result)::(snd result)
    
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =    
    try
        async
            {
                let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                let fileStream = new FileStream(path, FileMode.CreateNew)
                                 |> Option.ofObj     
                                 |> function 
                                     | Some value -> value
                                     | None       -> printfn "%s" "Error9"
                                                     new FileStream(path, FileMode.CreateNew) //whatever 
                return! stream.CopyToAsync(fileStream) |> Async.AwaitTask 
            } 
    with
    | _ -> printfn "Error10"
           Async.Sleep 0  


//************************Main code***********************************************************

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
         @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisTrainSpesakyARychliky.json"                
    ]

let private saveUpdatedJson() = 
   
    //TryWith
    let loadedJsonFiles = 
        jsonLinkList |> List.map (fun item -> async { return! client.GetStringAsync(item) |> Async.AwaitTask } |> Async.RunSynchronously)        
        
    (pathToJsonList, loadedJsonFiles)
    ||> List.iter2 (fun path json -> 
                                    //TryWith
                                    use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                    streamWriter.WriteLine(json)     
                                    streamWriter.Flush()   
                   )      

let private myLinksSet() = 
    //TryWith
    let kodisTimetables() = 

        pathToJsonList 
        |> Array.ofList 
        |> Array.collect (fun pathToJson ->         
                                          //TryWith 
                                          let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj
                                          //let kodisJsonSamples = kodisJsonSamples.GetSamples() |> Option.ofObj                 
                
                                          kodisJsonSamples
                                          |> function 
                                              | Some value -> value |> Array.map (fun item -> item.Timetable) //quli tomuto je nutno Array
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
         //tryWith
        pathToJsonList
        |> Array.ofList 
        |> Array.collect (fun pathToJson -> 
                                          let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj 

                                          let fn1 (value: JsonProvider<pathJson>.Attachment array) =
                                              value
                                              |> Array.Parallel.map (fun item -> errorFn item.Url "Error7")

                                          let fn2 (item: JsonProvider<pathJson>.Vyluky) =  //quli tomuto je nutno Array      
                                              item.Attachments |> Option.ofObj        
                                              |> function 
                                                  | Some value -> value |> fn1
                                                  | None       -> printfn "%s" "Error6"
                                                                  Array.empty                 

                                          let fn3 (item: JsonProvider<pathJson>.Root) =  //quli tomuto je nutno Array
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

    //****************prvni filtrace odkazu na neplatne jizdni rady***********************

    let currentTime = Fugit.now()
    
    //tryWith substring, take
    let myList = 
        myLinksSet
        |> Set.toArray 
        |> Array.Parallel.map (fun (item: string) ->  
                                                    let fileName =                                     
                                                        match item.Contains("timetables") with
                                                        | true  -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty).Replace("timetables", String.Empty)
                                                                   s.Substring(0, s.Length - 4).Remove(0, 2)                                                 
                                                        | false -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty)
                                                                   s.Substring(0, s.Length - 15).Remove(0, 1)
                                                    
                                                    let charList = fileName.ToCharArray() |> Array.toList |> List.take 3

                                                    let a i range = range |> List.filter (fun item -> charList |> List.item i = item)

                                                    let link, fileName = 
                                                        let fileNameFull = 
                                                            match a 0 range <> List.empty with
                                                            | true  -> match a 1 range <> List.empty with
                                                                        | true  -> match a 2 range <> List.empty with
                                                                                    | true  -> fileName                                                                     
                                                                                    | false -> sprintf "%s%s" "0" fileName                    
                                                                        | false -> sprintf "%s%s" "00" fileName
                                                            | false -> fileName  

                                                        match not (fileNameFull |> String.length >= 25) with //113_2022_12_11_2023_12_09.pdf
                                                        | true  -> String.Empty, String.Empty
                                                        | false ->                                     
                                                                 let lineNumberStr = fileNameFull.Substring(0, 3) //Substring(startIndex, length)
                                                                 let lineNumberInt = Parsing.parseMe(lineNumberStr)

                                                                 let yearOld = Parsing.parseMe(fileNameFull.Substring(4, 4)) //overovat, jestli se v jsonu neco nezmenilo //113_2022_12_11_2023_12_09.pdf
                                                                 let monthOld = Parsing.parseMe(fileNameFull.Substring(9, 2))
                                                                 let dayOld = Parsing.parseMe(fileNameFull.Substring(12, 2))
                                                                 let yearNew = Parsing.parseMe(fileNameFull.Substring(15, 4))
                                                                 let monthNew = Parsing.parseMe(fileNameFull.Substring(20, 2))
                                                                 let dayNew = Parsing.parseMe(fileNameFull.Substring(23, 2))
                                   
                                                                 let a = [ yearOld; monthOld; dayOld; yearNew; monthNew; dayNew ]

                                                                 match a |> List.contains -1 with
                                                                 | true  -> item, fileNameFull
                                                                 | false -> 
                                                                           try
                                                                               let dateOld = new DateTime(yearOld, monthOld, dayOld) 
                                                                               let dateNew = new DateTime(yearNew, monthNew, dayNew)                                   
                                                                                                                                                           
                                                                               let cond = ((currentTime |> Fugit.isAfter dateNew)
                                                                                          &&
                                                                                          (currentTime |> Fugit.isAfter dateOld))
                                                                                          || 
                                                                                          (currentTime |> Fugit.isAfter dateNew)                                                          
                                                                                
                                                                               match cond with
                                                                               | false -> item, fileNameFull                                               
                                                                               | true  -> String.Empty, String.Empty    
                                                                           with 
                                                                           | _ -> String.Empty, String.Empty              

                                                    let pathToFile = 
                                                        match link = String.Empty with
                                                        | true  -> String.Empty
                                                        | false -> sprintf"%s/%s%s" pathToDir fileName ".pdf"
                                                    link, pathToFile                                 
                            ) |> Set.ofArray |> Set.toList |> List.sort 

    let myList = 
        match myList |> List.contains (String.Empty, String.Empty) with
        | true  -> myList |> List.tail
        | false -> myList
    
    //****************druha filtrace odkazu na neplatne jizdni rady***********************
    let myList = 
        //tryWith substring 
        myList |> split         
        |> List.collect (fun item ->  
                                    match item.Length > 1 with
                                    | false -> item 
                                    | true  -> item                                           
                                               |> List.map (fun (link, pathToFile) ->  
                                                                                    //e:\E\Mirek po osme hodine a o vikendech\KODISTP\/929_2022_12_11_2023_12_09.pdf
                                                                                    //= pathToDir + / + zbytek ...
                                                                                    let length = pathToDir.Length + 1 //= 48 + 1 = 49
                                                                                    let yearOld = Parsing.parseMe(pathToFile.Substring(length + 4, 4))
                                                                                    let monthOld = Parsing.parseMe(pathToFile.Substring(length + 9, 2))
                                                                                    let dayOld = Parsing.parseMe(pathToFile.Substring(length + 12, 2))
                                                                                       
                                                                                    match [ yearOld; monthOld; dayOld ] |> List.contains -1 with
                                                                                    | true  -> String.Empty, String.Empty
                                                                                    | false -> 
                                                                                             try                                                                                                   
                                                                                                 let dateOld = new DateTime(yearOld, monthOld, dayOld)
                                                                                                 let dateValidityStart = new DateTime(2022, 12, 11) //zmenit s dalsim jizdnim radem                                                                                              
                                                                                                    
                                                                                                 match not <| dateOld.Equals(dateValidityStart) with
                                                                                                 | true  -> match pathToFile.Contains("_v") with 
                                                                                                            | true  -> link, pathToFile  
                                                                                                            | false -> String.Empty, String.Empty                                                                                                                                                        
                                                                                                 | false -> link, pathToFile                                                                                                                
                                                                                             with 
                                                                                             | _ -> String.Empty, String.Empty      
                                                           )        
                        ) |> List.distinctBy snd |> List.sort        
    
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

    //smazeme stare soubory v adresari   
    dirInfo.EnumerateFiles()
    |> Option.ofObj 
    |> function 
        | Some value -> value  
        | None       -> printfn "%s" "Error11"
                        Seq.empty    
    |> Array.ofSeq |> Array.Parallel.iter (fun item -> item.Delete()) 

    
    //************************download pdf souboru, ktere jsou aktualni*******************************************
      
    sortTimetables //TryWith //Parallel vyhazuje chyby  
    |> List.iter (fun (link, pathToFile) -> async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously 
                                            (*
                                            async 
                                                {  
                                                    let! responseBody = Async.Sleep 0
                                                    printfn"%s" pathToFile
                                                    return responseBody
                                                }  
                                            |> Async.RunSynchronously    *)                                      
                 )

    printfn"Pocet stazenych jizdnich radu: %i" sortTimetables.Length
    
let webscraping1() =
    processStart >> saveUpdatedJson >> myLinksSet 
                 >> sortTimetables >> downloadTimetables pathToDir 
                 >> processEnd >> client.Dispose  
