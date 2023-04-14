module WebScraping1

open System
open System.Threading

open Fugit
open System
open System.IO
open System.Net
open FSharp.Data

open Helpers
open TryWith.TryWith
open ProgressBarFSharp
open DiscriminatedUnions

do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
Console.BackgroundColor <- ConsoleColor.Blue 
Console.ForegroundColor <- ConsoleColor.White 
Console.InputEncoding   <- System.Text.Encoding.Unicode
Console.OutputEncoding  <- System.Text.Encoding.Unicode

//************************Constants and types**********************************************************************

//tu a tam zkontrolovat json, zdali KODIS nezmenil jeho strukturu 
let [<Literal>] partialPathJson =  @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/"
//pro type provider musi byt konstanta (nemozu pouzit sprintf partialPathJson) a musi byt forward slash"
let [<Literal>] pathJson = @"e:/E/Mirek po osme hodine a o vikendech/KODISJson/kodisMHDTotal.json" 
let [<Literal>] pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"
let [<Literal>] pathKodisAmazonLink = @"https://kodis-files.s3.eu-central-1.amazonaws.com/"
let [<Literal>] lineNumberLength = 3 //3 je delka retezce pouze pro linky 001 az 999

let private currentTime = Fugit.now().AddDays(-1.0)    // new DateTime(2023, 04, 11)
let private pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\" 
let private regularValidityStart = new DateTime(2022, 12, 11) //zmenit pri pravidelne zmene JR 
let private regularValidityEnd = new DateTime(2023, 12, 09) //zmenit pri pravidelne zmene JR 
let private range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
let private rangeS = [ "S1_"; "S2_"; "S3_"; "S4_"; "S5_"; "S6_"; "S7_"; "S8_"; "S9_" ]
let private rangeR = [ "R1_"; "R2_"; "R3_"; "R4_"; "R5_"; "R6_"; "R7_"; "R8_"; "R9_" ]
let private rangeX = [ "X1_"; "X2_"; "X3_"; "X4_"; "X5_"; "X6_"; "X7_"; "X8_"; "X9_" ]

type KodisTimetables = JsonProvider<pathJson> 

//************************Helpers**********************************************************************

let private xor a b = (a && not b) || (not a && b)

let private errorStr str err = str |> (optionToGenerics err String.Empty)                            

let private timeStr = errorStr "HH:mm:ss" "Error1"                     
    
let private processStart() =     
    let processStartTime x = 
        let processStartTime = errorStr (sprintf"Zacatek procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error2"                           
        printfn "%s" processStartTime
    tryWith processStartTime (fun x -> ()) () String.Empty () |> deconstructor
    
let private processEnd() =     
    let processEndTime x = 
        let processEndTime = errorStr (sprintf"Konec procesu: %s" <| DateTime.Now.ToString(timeStr)) "Error3"                       
        printfn "%s" processEndTime
    tryWith processEndTime (fun x -> ()) () String.Empty () |> deconstructor

let private client = 
    let myClient x = new System.Net.Http.HttpClient() |> (optionToGenerics "Error4" (new System.Net.Http.HttpClient()))         
    tryWith myClient (fun x -> ()) () String.Empty (new System.Net.Http.HttpClient()) |> deconstructor

let private splitList list = 
    let mySplitting x =
        let folder (a: string, b: string) (cur, acc) =
            let cond = a.Substring(0, lineNumberLength) = b.Substring(0, lineNumberLength) 
            match a with
            | _ when cond -> a::cur, acc
            | _           -> [a], cur::acc
        let result = List.foldBack folder (List.pairwise list) ([ List.last list ], []) 
        (fst result)::(snd result)
    tryWith mySplitting (fun x -> ()) () String.Empty [ List.empty ] |> deconstructor

    (*
    splitList will split the input list into groups of adjacent elements that have the same prefix.
    splitListByPrefix will group together all elements that have the same prefix, regardless of whether they are adjacent in the input list or not.
    *)

let private splitListByPrefix (list: string list) : string list list =
    let mySplitting x = 
        let prefix = (fun (x: string) -> x.Substring(0, lineNumberLength))
        let groups = list |> List.groupBy prefix  
        let filteredGroups = groups |> List.filter (fun (k, _) -> k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength))
        let result = filteredGroups |> List.map snd
        result
    tryWith mySplitting (fun x -> ()) () String.Empty [ List.empty ] |> deconstructor

//ekvivalent splitListByPrefix za predpokladu existence teto podminky shodnosti k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength)   
let private splitList1 (list: string list) : string list list =
    list |> List.groupBy (fun (item: string) -> item.Substring(0, lineNumberLength)) |> List.map (fun (key, group) -> group) 
    
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =    
    //muj custom made tryWith nezachyti exception u async
        async
            {   
                //TODO priste zrob Async.Catch
                try 
                    let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                    use fileStream = new FileStream(path, FileMode.CreateNew) //|> (optionToGenerics "Error9" (new FileStream(path, FileMode.CreateNew))) //nelze, vytvari to dalsi stream a uklada to znovu                                
                    return! stream.CopyToAsync(fileStream) |> Async.AwaitTask 
                with
                | ex -> 
                        printfn"\n%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" (string ex)
                        do Console.ReadKey() |> ignore 
                        do client.Dispose()
                        do System.Environment.Exit(1)
                        return ()
            }     
 

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
        sprintf"%s%s" partialPathJson @"kodisMHDTotal.json"
        sprintf"%s%s" partialPathJson @"kodisMHDBruntal.json"
        sprintf"%s%s" partialPathJson @"kodisMHDCT.json"
        sprintf"%s%s" partialPathJson @"kodisMHDFM.json"
        sprintf"%s%s" partialPathJson @"kodisMHDHavirov.json"
        sprintf"%s%s" partialPathJson @"kodisMHDKarvina.json"
        sprintf"%s%s" partialPathJson @"kodisMHDBKrnov.json"
        sprintf"%s%s" partialPathJson @"kodisMHDNJ.json"
        sprintf"%s%s" partialPathJson @"kodisMHDOpava.json"
        sprintf"%s%s" partialPathJson @"kodisMHDOrlova.json"
        sprintf"%s%s" partialPathJson @"kodisMHDOstrava.json"
        sprintf"%s%s" partialPathJson @"kodisMHDStudenka.json"
        sprintf"%s%s" partialPathJson @"kodisMHDTrinec.json"
        sprintf"%s%s" partialPathJson @"kodisMHDNAD.json"
        sprintf"%s%s" partialPathJson @"kodisRegionTotal.json"
        sprintf"%s%s" partialPathJson @"kodisRegion75.json"
        sprintf"%s%s" partialPathJson @"kodisRegion200.json"
        sprintf"%s%s" partialPathJson @"kodisRegion300.json"
        sprintf"%s%s" partialPathJson @"kodisRegion400.json"
        sprintf"%s%s" partialPathJson @"kodisRegion500.json"
        sprintf"%s%s" partialPathJson @"kodisRegion600.json"
        sprintf"%s%s" partialPathJson @"kodisRegion700.json"
        sprintf"%s%s" partialPathJson @"kodisRegion800.json"
        sprintf"%s%s" partialPathJson @"kodisRegion900.json"
        sprintf"%s%s" partialPathJson @"kodisRegionNAD.json"
        sprintf"%s%s" partialPathJson @"kodisTrainTotal.json"
        sprintf"%s%s" partialPathJson @"kodisTrainPomaliky.json"
        sprintf"%s%s" partialPathJson @"kodisTrainSpesakyARychliky.json"                
    ]

let private downloadAndSaveUpdatedJson() = 

    let updateJson x = 
        let loadAndSaveJsonFiles = 
            let l = jsonLinkList.Length
            jsonLinkList
            |> List.mapi (fun i item ->                                                
                                      progressBarContinuous i l 
                                      //updateJson x nezachyti exception v async
                                      async  
                                          { 
                                              //TODO priste zrob Async.Catch
                                              try 
                                                  return! client.GetStringAsync(item) |> Async.AwaitTask 
                                              with
                                              | ex -> 
                                                      printfn"\n%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" (string ex)
                                                      do Console.ReadKey() |> ignore 
                                                      do System.Environment.Exit(1)
                                                      return! client.GetStringAsync(String.Empty) |> Async.AwaitTask //whatever of that type
                                          } |> Async.RunSynchronously
                        
                         )  
        //save updated json files
        (pathToJsonList, loadAndSaveJsonFiles)
        ||> List.iteri2 (fun i path json ->                                                                          
                                          use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                          streamWriter.WriteLine(json)     
                                          streamWriter.Flush()   
                        ) 
    printfn "Probiha stahovani a ukladani json souboru do prislusneho adresare"
    tryWith updateJson (fun x -> ()) () String.Empty () |> deconstructor                

let private digThroughJsonStructure() = //prohrabeme se strukturou json souboru
    
    let kodisTimetables() = 

        let myFunction x = 
            pathToJsonList 
            |> Array.ofList 
            |> Array.collect (fun pathToJson ->   
                                              let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj
                                              //let kodisJsonSamples = kodisJsonSamples.GetSamples() |> Option.ofObj  //v pripade jen jednoho json               
                
                                              kodisJsonSamples 
                                              |> function 
                                                  | Some value -> value |> Array.map (fun item -> item.Timetable) //quli tomuto je nutno Array
                                                  | None       -> 
                                                                  printfn "%s" "Error5"
                                                                  Array.empty    
                             ) 
        tryWith myFunction (fun x -> ()) () String.Empty Array.empty |> deconstructor

    let kodisAttachments() = 

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

        let myFunction x = 
            pathToJsonList
            |> Array.ofList 
            |> Array.collect (fun pathToJson ->
                                              let fn1 (value: JsonProvider<pathJson>.Attachment array) =
                                                  value //Option je v errorStr 
                                                  |> Array.Parallel.map (fun item -> errorStr item.Url "Error7")

                                              let fn2 (item: JsonProvider<pathJson>.Vyluky) =  //quli tomuto je nutno Array      
                                                  item.Attachments |> Option.ofObj        
                                                  |> function 
                                                      | Some value -> value |> fn1
                                                      | None       -> 
                                                                      printfn "%s" "Error6c"
                                                                      Array.empty                 

                                              let fn3 (item: JsonProvider<pathJson>.Root) =  //quli tomuto je nutno Array
                                                  item.Vyluky |> Option.ofObj
                                                  |> function 
                                                      | Some value -> value |> Array.collect fn2 
                                                      | None       ->
                                                                      printfn "%s" "Error6b"
                                                                      Array.empty 
                                              
                                              let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj 
                                              
                                              kodisJsonSamples 
                                              |> function 
                                                  | Some value -> value |> Array.collect fn3 
                                                  | None       -> 
                                                                  printfn "%s" "Error6a"
                                                                  Array.empty                                 
                             ) 
        tryWith myFunction (fun x -> ()) () String.Empty Array.empty |> deconstructor

    (Array.append <| kodisAttachments() <| kodisTimetables()) |> Set.ofArray //jen z vyukovych duvodu -> konverzi na Set vyhodime stejne polozky, jinak staci jen |> Array.distinct 
    //kodisAttachments() |> Set.ofArray //over cas od casu
    //kodisTimetables() |> Set.ofArray //over cas od casu

let private filterTimetables param diggingResult = 

    //****************prvni filtrace odkazu na neplatne jizdni rady***********************
    
    let myList = 
        let myFunction x =            
            diggingResult
            |> Set.toArray 
            |> Array.Parallel.map (fun (item: string) ->                                                       
                                                        //misto pro opravu retezcu, ktere jsou v jsonu v nespravnem formatu
                                                        let item = match item.Contains(@"S2_2023_04_03_2023_04_3_v") with
                                                                   | true  -> item.Replace(@"S2_2023_04_03_2023_04_3_v", @"S2_2023_04_03_2023_04_03_v")  
                                                                   | false -> item     

                                                        let fileName =  
                                                            match item.Contains @"timetables/" with
                                                            | true  -> item.Replace(pathKodisAmazonLink, String.Empty).Replace("timetables/", String.Empty).Replace(".pdf", "_t.pdf")
                                                            | false -> item.Replace(pathKodisAmazonLink, String.Empty)  
                                                    
                                                        let charList = 
                                                            match fileName |> String.length >= lineNumberLength with  
                                                            | true  -> fileName.ToCharArray() |> Array.toList |> List.take lineNumberLength
                                                            | false -> 
                                                                       printfn "Error11"
                                                                       List.empty
                                             
                                                        let a i range = range |> List.filter (fun item -> (charList |> List.item i = item)) 
                                                        let b range = range |> List.contains (fileName.Substring(0, 3))
                                                   
                                                        let fileNameFullA = 
                                                            match a 0 range <> List.empty  with
                                                            | true  -> match a 1 range <> List.empty with
                                                                        | true  -> match a 2 range <> List.empty with
                                                                                   | true  -> fileName                                                                     
                                                                                   | false -> sprintf "%s%s" "0" fileName                    
                                                                        | false -> sprintf "%s%s" "00" fileName //pocet "0" zavisi na delce retezce cisla linky
                                                            | false -> fileName  
                                                         
                                                        let fileNameFull =  
                                                            match b rangeS || b rangeR || b rangeX with
                                                            | true  -> sprintf "%s%s" "_" fileNameFullA                                                                       
                                                            | false -> fileNameFullA  

                                                        let numberOfChar =  
                                                            match fileNameFull.Contains("_v") || fileNameFull.Contains("_t") with
                                                            | true  -> 27  //27 -> 113_2022_12_11_2023_12_09_t......   //overovat, jestli se v jsonu nezmenila struktura nazvu                                                                
                                                            | false -> 25  //25 -> 113_2022_12_11_2023_12_09......
                                                         
                                                        match not (fileNameFull |> String.length >= numberOfChar) with 
                                                        | true  -> String.Empty
                                                        | false ->     
                                                                   let yearValidityStart = Parsing.parseMe(fileNameFull.Substring(4, 4)) 
                                                                   let monthValidityStart = Parsing.parseMe(fileNameFull.Substring(9, 2))
                                                                   let dayValidityStart = Parsing.parseMe(fileNameFull.Substring(12, 2))

                                                                   let yearValidityEnd = Parsing.parseMe(fileNameFull.Substring(15, 4))
                                                                   let monthValidityEnd = Parsing.parseMe(fileNameFull.Substring(20, 2))
                                                                   let dayValidityEnd = Parsing.parseMe(fileNameFull.Substring(23, 2))
                                                                                                                                                                  
                                                                   let a = [ yearValidityStart; monthValidityStart; dayValidityStart; yearValidityEnd; monthValidityEnd; dayValidityEnd ]
                                                                
                                                                   match a |> List.contains -1 with
                                                                   | true  -> let cond = 
                                                                                  match param with 
                                                                                  | CurrentValidity           -> true //s tim nic nezrobim, nekonzistentni informace v retezci
                                                                                  | FutureValidity            -> true //s tim nic nezrobim, nekonzistentni informace v retezci
                                                                                  | ReplacementService        -> 
                                                                                                                 fileNameFull.Contains("_v") 
                                                                                                                 || fileNameFull.Contains("X")
                                                                                                                 || fileNameFull.Contains("NAD")
                                                                                  | WithoutReplacementService -> 
                                                                                                                 not <| fileNameFull.Contains("_v") 
                                                                                                                 && not <| fileNameFull.Contains("X")
                                                                                                                 && not <| fileNameFull.Contains("NAD")

                                                                              match cond with
                                                                              | true  -> fileNameFull
                                                                              | false -> String.Empty 
                                                                              
                                                                   | false -> 
                                                                            try
                                                                                let dateValidityStart = new DateTime(yearValidityStart, monthValidityStart, dayValidityStart) 
                                                                                let dateValidityEnd = new DateTime(yearValidityEnd, monthValidityEnd, dayValidityEnd) 
                                                                                
                                                                                let cond = 
                                                                                    match param with 
                                                                                    | CurrentValidity           -> dateValidityStart |> Fugit.isBeforeOrEqual currentTime && dateValidityEnd |> Fugit.isAfter currentTime
                                                                                    | FutureValidity            -> dateValidityStart |> Fugit.isAfter currentTime
                                                                                    | ReplacementService        -> 
                                                                                                                   (dateValidityStart |> Fugit.isBeforeOrEqual currentTime
                                                                                                                   && dateValidityEnd |> Fugit.isAfter currentTime)
                                                                                                                   && (fileNameFull.Contains("_v") 
                                                                                                                   || fileNameFull.Contains("X")
                                                                                                                   || fileNameFull.Contains("NAD"))
                                                                                    | WithoutReplacementService ->
                                                                                                                   (dateValidityStart |> Fugit.isBeforeOrEqual currentTime
                                                                                                                   && dateValidityEnd |> Fugit.isAfter currentTime)
                                                                                                                   && (not <| fileNameFull.Contains("_v") 
                                                                                                                   && not <| fileNameFull.Contains("X")
                                                                                                                   && not <| fileNameFull.Contains("NAD"))

                                                                                match cond with
                                                                                | true  -> fileNameFull
                                                                                | false -> String.Empty  

                                                                            with 
                                                                            | _ -> String.Empty  
                                                   
                                  ) |> Array.toList |> List.distinct 
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor
    
    let myList1 = 
        myList |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))    
    
    printfn "Dokoncena prvni filtrace odkazu na neplatne jizdni rady"
    
    //****************druha filtrace odkazu na neplatne jizdni rady***********************
   
    let myList2 = 
        let myFunction x = 
            //list listu se stejnymi linkami s ruznou dobou platnosti JR      
            myList1 
            |> splitListByPrefix //splitList1 //splitList 
            |> List.collect (fun list ->  
                                        match list.Length > 1 with 
                                        | false -> list 
                                        | true  -> 
                                                   let latestValidityStart =  
                                                       list
                                                       |> List.map (fun item -> 
                                                                              try
                                                                                  let yearValidityStart = Parsing.parseMe(item.Substring(4, 4)) //overovat, jestli se v jsonu neco nezmenilo //113_2022_12_11_2023_12_09.....
                                                                                  let monthValidityStart = Parsing.parseMe(item.Substring(9, 2))
                                                                                  let dayValidityStart = Parsing.parseMe(item.Substring(12, 2))

                                                                                  let yearValidityEnd = Parsing.parseMe(item.Substring(15, 4))
                                                                                  let monthValidityEnd = Parsing.parseMe(item.Substring(20, 2))
                                                                                  let dayValidityEnd = Parsing.parseMe(item.Substring(23, 2))

                                                                                  item, new DateTime(yearValidityStart, monthValidityStart, dayValidityStart) 
                                                                                  //item, new DateTime(yearValidityEnd, monthValidityEnd, dayValidityEnd) //pro pripadnou zmenu logiky
                                                                              with 
                                                                              | _ -> item, currentTime
                                                                   ) |> List.maxBy snd                                                        
                                                   [ fst latestValidityStart ]                                                   
                            ) |> List.distinct                              
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor 
        
    let myList3 = 
        myList2 |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))

    let myList4 = 
        let myFunction x = 
            myList3 
            |> List.map (fun (item: string) ->        
                                             let str = item
                                             let str =
                                                 match str.Substring(0, 2).Equals("00") with
                                                 | true   -> str.Remove(0, 2)
                                                 | false  -> match str.Substring(0, 1).Equals("0") || str.Substring(0, 1).Equals("_") with
                                                             | false -> item
                                                             | true  -> str.Remove(0, 1)                                                                                  
                                             
                                             let link = 
                                                match item.Contains("_t") with 
                                                | true  -> (sprintf"%s%s%s" pathKodisAmazonLink @"timetables/" str).Replace("_t", String.Empty)
                                                | false -> sprintf"%s%s" pathKodisAmazonLink str                                                

                                             let path =     
                                                match item.Contains("_t") with 
                                                | true  -> 
                                                           let fileName = item.Substring(0, item.Length) //zatim bez generovaneho kodu, sem tam to zkontrolovat
                                                           sprintf"%s/%s" pathToDir fileName   
                                                | false -> 
                                                           let fileName = item.Substring(0, item.Length - 15) //bez 15 znaku s generovanym kodem a priponou pdf dostaneme toto: 113_2022_12_11_2023_12_09 
                                                           sprintf"%s/%s%s" pathToDir fileName ".pdf"  //pdf opet musime pridat
                                                           
                                             link, path 
                        )
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor
    
    printfn "Dokoncena druha filtrace odkazu na neplatne jizdni rady"
    
    myList4 |> List.filter (fun item -> 
                                         (not <| String.IsNullOrWhiteSpace(fst item) 
                                         && 
                                         not <| String.IsNullOrEmpty(fst item)) 
                                         ||
                                         (not <| String.IsNullOrWhiteSpace(snd item)
                                         && 
                                         not <| String.IsNullOrEmpty(snd item))                                         
                           ) |> List.sort                                             
        
let private downloadAndSaveTimetables pathToDir (filterTimetables: (string*string) list) =    
    
        
    let myFileDelete x =   
        let dirInfo = new DirectoryInfo(pathToDir) |> optionToGenerics "Error8" (new DirectoryInfo(pathToDir))                                                               
        
        //failwith "Testovani funkce tryWith"

        //smazeme stare soubory v adresari  
        dirInfo.EnumerateFiles()
        |> optionToGenerics "Error11" Seq.empty       
        |> Array.ofSeq
        |> Array.Parallel.iter (fun item -> item.Delete())    
   
    tryWith myFileDelete (fun x -> ()) () String.Empty () |> deconstructor
    printfn "Dokonceno mazani starych jizdnich radu v prislusnem adresari"
    
    //************************download pdf souboru, ktere jsou aktualni*******************************************
    
    //tryWith je ve funkci downloadFileTaskAsync
    printfn "Probiha stahovani jizdnich radu a jejich ukladani do prislusneho adresare" 
    let downloadTimetables() = 
        let l = filterTimetables.Length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  //Array.Parallel.iter tady nelze  
                                                 progressBarContinuous i l
                                                 async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously  
                                                 //async { printfn"%s" pathToFile; return! Async.Sleep 0 } |> Async.RunSynchronously
                                                 //async {return! Async.Sleep 10 } |> Async.RunSynchronously   
                      )    
   
    //progressBarIndeterminate <| downloadTimetables  

    downloadTimetables() //progressBarContinuous

    printfn"%c" <| char(32)   
    printfn"%c" <| char(32)  
    printfn"Pocet stazenych jizdnich radu: %i" filterTimetables.Length   

let webscraping1() =
    processStart 
    >> downloadAndSaveUpdatedJson
    >> digThroughJsonStructure 
    >> filterTimetables CurrentValidity //CurrentValidity //FutureValidity //ReplacementService //WithoutReplacementService
    >> downloadAndSaveTimetables pathToDir       
    >> client.Dispose  
    >> processEnd

    //CurrentValidity = JR striktne platne k danemu dni, tj. pokud je napr. na dany den vylukovy JR, stahne se tento JR, ne JR platny dalsi den
    //FutureValidity = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu
    //ReplacementService = pouze vylukove JR, JR NAD a JR X linek
    //WithoutReplacementService = JR dlouhodobe platne bez jakykoliv vyluk. Tento vyber neobsahuje ani dlouhodobe nekolikamesicni vyluky, muze se ale hodit v pripade, ze zakladni slozka s JR obsahuje jedno ci dvoudenni vylukove JR. 
   
   //vzhledem k nekonzistnosti retezce s udaji o lince a platnosti muze dojit ke stazeni i neceho, co do daneho vyberu nepatri
