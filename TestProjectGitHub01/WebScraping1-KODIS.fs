module WebScraping1_KODIS

open System
open System.IO
open System.Net
open System.Reflection
open System.Threading.Tasks

open Fugit
open FSharp.Data
open FSharp.Reflection

open Helpers
open Settings
open TryWith.TryWith
open Messages.Messages
open ProgressBarFSharp
open DiscriminatedUnions
open WebScraping1_Helpers
open ErrorFunctions.ErrorFunctions
open PatternBuilders.PattternBuilders


//************************Console**********************************************************************************

consoleAppProblemFixer()

//************************Constants and types**********************************************************************

//tu a tam zkontrolovat json, zdali KODIS nezmenil jeho strukturu 
//pro type provider musi byt konstanta (nemozu pouzit sprintf partialPathJson) a musi byt forward slash"
let [<Literal>] pathJson = @"KODISJson/kodisMHDTotal.json" //v hl. adresari projektu

let [<Literal>] partialPathJson = @"KODISJson/" //v binu

let [<Literal>] pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"
let [<Literal>] pathKodisAmazonLink = @"https://kodis-files.s3.eu-central-1.amazonaws.com/"
let [<Literal>] lineNumberLength = 3 //3 je delka retezce pouze pro linky 001 az 999

let private currentTime = Fugit.now()//.AddDays(-1.0)   // new DateTime(2023, 04, 11)
let private regularValidityStart = new DateTime(2022, 12, 11) //zmenit pri pravidelne zmene JR 
let private regularValidityEnd = new DateTime(2023, 12, 09) //zmenit pri pravidelne zmene JR 
let private range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
let private rangeS = [ "S1_"; "S2_"; "S3_"; "S4_"; "S5_"; "S6_"; "S7_"; "S8_"; "S9_" ]
let private rangeR = [ "R1_"; "R2_"; "R3_"; "R4_"; "R5_"; "R6_"; "R7_"; "R8_"; "R9_" ]
let private rangeX = [ "X1_"; "X2_"; "X3_"; "X4_"; "X5_"; "X6_"; "X7_"; "X8_"; "X9_" ]
let private rangeA = [ "AE_" ] //ponechan prostor pro pripadne cislovani AE
let private rangeN1 = [ "NAD_1_"; "NAD_2_"; "NAD_3_"; "NAD_4_"; "NAD_5_"; "NAD_6_"; "NAD_7_"; "NAD_8_"; "NAD_9_" ]
let private rangeN2 = [ "NAD_10_"; "NAD_11_"; "NAD_12_"; "NAD_13_"; "NAD_14_"; "NAD_15_"; "NAD_16_"; "NAD_17_"; "NAD_18_"; "NAD_19_" ]
//TODO jestli bude cas, pridelat NAD vlakovych linek

type KodisTimetables = JsonProvider<pathJson> 

//************************Helpers**********************************************************************
//Pro ostatni helpers viz WebScraping1-Helpers.fs

let private client = client()

let private getDefaultRecordValues = //record -> Array //open FSharp.Reflection

    FSharpType.GetRecordFields(typeof<ODIS>) //dostanu pole hodnot typu PropertyInfo
    |> Array.map (fun (prop: PropertyInfo) -> prop.GetGetMethod().Invoke(ODIS.Default, [||]) :?> string)            
    |> Array.take 4 //jen prvni 4 polozky jsou pro celo-KODIS variantu

let private splitList list = //I 

    let mySplitting x = //P
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

let private splitListByPrefix (list: string list) : string list list = //I 

    let mySplitting x = //P
        let prefix = (fun (x: string) -> x.Substring(0, lineNumberLength))
        let groups = list |> List.groupBy prefix  
        let filteredGroups = groups |> List.filter (fun (k, _) -> k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength))
        let result = filteredGroups |> List.map snd
        result
    tryWith mySplitting (fun x -> ()) () String.Empty [ List.empty ] |> deconstructor

//ekvivalent splitListByPrefix za predpokladu existence teto podminky shodnosti k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength)   
let private splitList1 (list: string list) : string list list = //P

    list |> List.groupBy (fun (item: string) -> item.Substring(0, lineNumberLength)) |> List.map (fun (key, group) -> group) 
 
//************************Main code***********************************************************

let private jsonLinkList = //P

    [
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Český%20Těšín&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Frýdek-Místek&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Havířov&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Karviná&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Krnov&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Nový%20Jičín&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Opava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Orlová&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Studénka&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Třinec&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD%20MHD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&group_in%5B1%5D=232-293&group_in%5B2%5D=331-392&group_in%5B3%5D=440-465&group_in%5B4%5D=531-583&group_in%5B5%5D=613-699&group_in%5B6%5D=731-788&group_in%5B7%5D=811-885&group_in%5B8%5D=901-990&group_in%5B9%5D=NAD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=232-293&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=331-392&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=440-465&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=531-583&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=613-699&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=731-788&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=811-885&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=901-990&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=S1-S34&group_in%5B1%5D=R8-R61&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=S1-S34&group_in%5B1%5D=R8-R61&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=S1-S34&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=R8-R61&_sort=numeric_label"   
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD&_sort=numeric_label"     
    ]

let private pathToJsonList =  //P 
    
    [
        sprintf "%s%s" partialPathJson @"kodisMHDTotal.json"
        sprintf "%s%s" partialPathJson @"kodisMHDTotal1.json"
        sprintf "%s%s" partialPathJson @"kodisMHDBruntal.json"
        sprintf "%s%s" partialPathJson @"kodisMHDCT.json"
        sprintf "%s%s" partialPathJson @"kodisMHDFM.json"
        sprintf "%s%s" partialPathJson @"kodisMHDHavirov.json"
        sprintf "%s%s" partialPathJson @"kodisMHDKarvina.json"
        sprintf "%s%s" partialPathJson @"kodisMHDBKrnov.json"
        sprintf "%s%s" partialPathJson @"kodisMHDNJ.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOpava.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOrlova.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava.json"
        sprintf "%s%s" partialPathJson @"kodisMHDStudenka.json"
        sprintf "%s%s" partialPathJson @"kodisMHDTrinec.json"
        sprintf "%s%s" partialPathJson @"kodisMHDNAD.json"
        sprintf "%s%s" partialPathJson @"kodisRegionTotal.json"
        sprintf "%s%s" partialPathJson @"kodisRegion75.json"
        sprintf "%s%s" partialPathJson @"kodisRegion200.json"
        sprintf "%s%s" partialPathJson @"kodisRegion300.json"
        sprintf "%s%s" partialPathJson @"kodisRegion400.json"
        sprintf "%s%s" partialPathJson @"kodisRegion500.json"
        sprintf "%s%s" partialPathJson @"kodisRegion600.json"
        sprintf "%s%s" partialPathJson @"kodisRegion700.json"
        sprintf "%s%s" partialPathJson @"kodisRegion800.json"
        sprintf "%s%s" partialPathJson @"kodisRegion900.json"
        sprintf "%s%s" partialPathJson @"kodisRegionNAD.json"
        sprintf "%s%s" partialPathJson @"kodisTrainTotal.json"
        sprintf "%s%s" partialPathJson @"kodisTrainTotal1.json"
        sprintf "%s%s" partialPathJson @"kodisTrainPomaliky.json"
        sprintf "%s%s" partialPathJson @"kodisTrainSpesakyARychliky.json"   
        sprintf "%s%s" partialPathJson @"kodisNAD.json"
    ]

let private downloadAndSaveUpdatedJson() = 

    let updateJson x = //I
        let loadAndSaveJsonFiles = //I
            let l = jsonLinkList |> List.length
            jsonLinkList
            |> List.mapi (fun i item ->                                                
                                      progressBarContinuous i l 
                                      //updateJson x nezachyti exception v async
                                      async  
                                          { 
                                              try 
                                                  return! client.GetStringAsync(item) |> Async.AwaitTask 
                                              with
                                              | ex -> 
                                                      deconstructorError <| msgParam1 ex <| ()
                                                      return! client.GetStringAsync(String.Empty) |> Async.AwaitTask //whatever of that type
                                          } |> Async.RunSynchronously
                        
                         )  

        //save updated json files
        match (<>) (pathToJsonList |> List.length) (loadAndSaveJsonFiles |> List.length) with
        | true  -> 
                  msg1()
                  do Console.ReadKey() |> ignore 
                  do System.Environment.Exit(1)
        | false ->
                  (pathToJsonList, loadAndSaveJsonFiles)
                  ||> List.iteri2 (fun i path json ->                                                                          
                                                    use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                                    streamWriter.WriteLine(json)     
                                                    streamWriter.Flush()   
                                  ) 

    msg2() 

    tryWith updateJson (fun x -> ()) () String.Empty () |> deconstructor    

    msg3() 
    msg4() 

let private digThroughJsonStructure() = //prohrabeme se strukturou json souboru
    
    let kodisTimetables() = //I

        let myFunction x = //I
            pathToJsonList 
            |> Array.ofList 
            |> Array.collect (fun pathToJson ->   
                                              let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj //I
                                              //let kodisJsonSamples = kodisJsonSamples.GetSamples() |> Option.ofObj  //v pripade jen jednoho json               
                
                                              kodisJsonSamples 
                                              |> function 
                                                  | Some value -> value |> Array.map (fun item -> item.Timetable) //quli tomuto je nutno Array
                                                  | None       -> 
                                                                  msg5() 
                                                                  Array.empty    
                             ) 
        tryWith myFunction (fun x -> ()) () String.Empty Array.empty |> deconstructor

    let kodisAttachments() = //I

        (*
        //ponechavam pro pochopeni struktury u json type provider (pri pouziti option se to tahne az k susedovi)
        let kodisAttachments() = 
            kodisJsonSamples                              
            |> Array.collect (fun item ->                                            
                                        item.Vyluky 
                                        |> Array.collect (fun item ->                                                 
                                                                    item.Attachments
                                                                    |> Array.Parallel.map (fun item -> item.Url)
                                                         ) 
                             )   
        *)  

        let myFunction x = //I
            pathToJsonList
            |> Array.ofList 
            |> Array.collect (fun pathToJson ->
                                              let fn1 (value: JsonProvider<pathJson>.Attachment array) = //AP
                                                  value //Option je v errorStr 
                                                  |> Array.Parallel.map (fun item -> errorStr item.Url "Error7")

                                              let fn2 (item: JsonProvider<pathJson>.Vyluky) =  //quli tomuto je nutno Array //AP    
                                                  item.Attachments |> Option.ofObj        
                                                  |> function 
                                                      | Some value -> value |> fn1
                                                      | None       -> 
                                                                      msg6() 
                                                                      Array.empty                 

                                              let fn3 (item: JsonProvider<pathJson>.Root) =  //quli tomuto je nutno Array //AP
                                                  item.Vyluky |> Option.ofObj
                                                  |> function 
                                                      | Some value -> value |> Array.collect fn2 
                                                      | None       ->
                                                                      msg7() 
                                                                      Array.empty 
                                              
                                              let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj //I
                                              
                                              kodisJsonSamples 
                                              |> function 
                                                  | Some value -> value |> Array.collect fn3 
                                                  | None       -> 
                                                                  msg8() 
                                                                  Array.empty                                 
                             ) 
        tryWith myFunction (fun x -> ()) () String.Empty Array.empty |> deconstructor          

    (Array.append <| kodisAttachments() <| kodisTimetables()) |> Set.ofArray //jen z vyukovych duvodu -> konverzi na Set vyhodime stejne polozky, jinak staci jen |> Array.distinct 
    //kodisAttachments() |> Set.ofArray //over cas od casu
    //kodisTimetables() |> Set.ofArray //over cas od casu

let private filterTimetables param pathToDir diggingResult = //I

    //****************prvni filtrace odkazu na neplatne jizdni rady***********************   
    
    let myList = 
        let myFunction x =  //AP          
            diggingResult
            |> Set.toArray 
            |> Array.Parallel.map (fun (item: string) ->   
                                                        let item = string item

                                                        //misto pro opravu retezcu v PDF, ktere jsou v jsonu v nespravnem formatu - 
                                                        let item = 
                                                            match item.Contains(@"S2_2023_04_03_2023_04_3_v") with
                                                            | true  -> item.Replace(@"S2_2023_04_03_2023_04_3_v", @"S2_2023_04_03_2023_04_03_v")  
                                                            | false -> item        
                                                        //konec opravy retezcu 

                                                        //s chybnymi udaji v datech uz nic nenadelam, bez komplikovanych reseni..., tohle selekce vyradi jako neplatne (v JR je 2023_12_31)
                                                        //https://kodis-files.s3.eu-central-1.amazonaws.com/NAD_2022_12_11_2023_03_31_v_1a2f33dafa.pdf

                                                        let fileName =  
                                                            match item.Contains @"timetables/" with
                                                            | true  -> item.Replace(pathKodisAmazonLink, String.Empty).Replace("timetables/", String.Empty).Replace(".pdf", "_t.pdf")
                                                            | false -> item.Replace(pathKodisAmazonLink, String.Empty)  
                                                    
                                                        let charList = 
                                                            match fileName |> String.length >= lineNumberLength with  
                                                            | true  -> fileName.ToCharArray() |> Array.toList |> List.take lineNumberLength
                                                            | false -> 
                                                                       msg9() 
                                                                       List.empty
                                             
                                                        let a i range = range |> List.filter (fun item -> (charList |> List.item i = item)) 
                                                        let b range = range |> List.contains (fileName.Substring(0, 3))

                                                        let fileNameFullA = 
                                                            MyBuilder
                                                                {
                                                                    let!_ = not <| fileName.Contains("NAD"), fileName 
                                                                    let!_ = (<>) (a 0 range) List.empty, fileName 
                                                                    let!_ = (<>) (a 1 range) List.empty, sprintf "%s%s" "00" fileName //pocet "0" zavisi na delce retezce cisla linky 
                                                                    let!_ = (<>) (a 2 range) List.empty, sprintf "%s%s" "0" fileName 

                                                                    return fileName
                                                                }                                                            
                                                         
                                                        let fileNameFull =  
                                                            match b rangeS || b rangeR || b rangeX || b rangeA with
                                                            | true  -> sprintf "%s%s" "_" fileNameFullA                                                                       
                                                            | false -> fileNameFullA  

                                                        let numberOfChar =  //vyhovuje i pro NAD
                                                            match fileNameFull.Contains("_v") || fileNameFull.Contains("_t") with
                                                            | true  -> 27  //27 -> 113_2022_12_11_2023_12_09_t......   //overovat, jestli se v jsonu nezmenila struktura nazvu                                                                
                                                            | false -> 25  //25 -> 113_2022_12_11_2023_12_09......
                                                         
                                                        match not (fileNameFull |> String.length >= numberOfChar) with 
                                                        | true  -> String.Empty
                                                        | false ->     
                                                                   let yearValidityStart x = Parsing.parseMe(fileNameFull.Substring(4 + x, 4)) 
                                                                   let monthValidityStart x = Parsing.parseMe(fileNameFull.Substring(9 + x, 2))
                                                                   let dayValidityStart x = Parsing.parseMe(fileNameFull.Substring(12 + x, 2))

                                                                   let yearValidityEnd x = Parsing.parseMe(fileNameFull.Substring(15 + x, 4))
                                                                   let monthValidityEnd x = Parsing.parseMe(fileNameFull.Substring(20 + x, 2))
                                                                   let dayValidityEnd x = Parsing.parseMe(fileNameFull.Substring(23 + x, 2))
                                                                                                                                                                  
                                                                   let a x =
                                                                       [ 
                                                                           yearValidityStart x
                                                                           monthValidityStart x
                                                                           dayValidityStart x
                                                                           yearValidityEnd x
                                                                           monthValidityEnd x
                                                                           dayValidityEnd x
                                                                       ]
                                                                   
                                                                   let result x = 

                                                                       match (a x) |> List.contains -1 with
                                                                       | true  -> 
                                                                                let cond = 
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
                                                                                    let dateValidityStart x = new DateTime(yearValidityStart x, monthValidityStart x, dayValidityStart x) 
                                                                                    let dateValidityEnd x = new DateTime(yearValidityEnd x, monthValidityEnd x, dayValidityEnd x) 
                                                                                
                                                                                    let cond = 
                                                                                        match param with 
                                                                                        | CurrentValidity           -> 
                                                                                                                       (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime 
                                                                                                                       && 
                                                                                                                       dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                                       ||
                                                                                                                       ((dateValidityStart x).Equals(currentTime) 
                                                                                                                       && 
                                                                                                                       (dateValidityEnd x).Equals(currentTime))

                                                                                        | FutureValidity            -> dateValidityStart x |> Fugit.isAfter currentTime

                                                                                        | ReplacementService        -> 
                                                                                                                       (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime
                                                                                                                       && 
                                                                                                                       dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                                       &&
                                                                                                                       (fileNameFull.Contains("_v") 
                                                                                                                       || fileNameFull.Contains("X")
                                                                                                                       || fileNameFull.Contains("NAD"))

                                                                                        | WithoutReplacementService ->
                                                                                                                       (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime
                                                                                                                       && 
                                                                                                                       dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                                       &&
                                                                                                                       (not <| fileNameFull.Contains("_v") 
                                                                                                                       && not <| fileNameFull.Contains("X")
                                                                                                                       && not <| fileNameFull.Contains("NAD"))
                                                                                
                                                                                    match cond with
                                                                                    | true  -> fileNameFull
                                                                                    | false -> String.Empty                                                                                
                                                                               
                                                                                with 
                                                                                | _ -> String.Empty  

                                                                   let condNAD (rangeN: string list) =                                                                     
                                                                       rangeN |> List.tryFind (fun item -> fileNameFull.Contains(item))                                                                                    
                                                                       |> function 
                                                                           | Some _ -> true
                                                                           | None   -> false  
                                                                               
                                                                   let condNAD = xor (condNAD rangeN1) (condNAD rangeN2) 
                                                                                
                                                                   let x = //korekce pozice znaku v retezci
                                                                       match fileNameFull.Contains("NAD") && condNAD = true with
                                                                       | true  -> 2 
                                                                       | false -> 0 

                                                                   result x
                                                   
                                  ) |> Array.toList |> List.distinct 
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor
    
    let myList1 = //P
        myList |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))     
    
    //****************druha filtrace odkazu na neplatne jizdni rady***********************
   
    let myList2 = //I
        let myFunction x = //P
            //list listu se stejnymi linkami s ruznou dobou platnosti JR  
            myList1 
            |> splitListByPrefix //splitList1 //splitList 
            |> List.collect (fun list ->  
                                        match (>) (list |> List.length) 1 with 
                                        | false -> list 
                                        | true  -> 
                                                   let latestValidityStart =  
                                                       list
                                                       |> List.map (fun item -> 
                                                                              let item = string item                                                                              
                                                                              try
                                                                                  let condNAD (rangeN: string list) =                                                                     
                                                                                      rangeN |> List.tryFind (fun item1 -> item.Contains(item1))                                                                                    
                                                                                      |> function 
                                                                                          | Some _ -> true
                                                                                          | None   -> false  
                                                                                              
                                                                                  let condNAD = xor (condNAD rangeN1) (condNAD rangeN2) 
                                                                                               
                                                                                  let x = //korekce pozice znaku v retezci
                                                                                      match item.Contains("NAD") && condNAD = true with
                                                                                      | true  -> 2 
                                                                                      | false -> 0 
                                                                                  
                                                                                  let yearValidityStart x = Parsing.parseMe(item.Substring(4 + x, 4)) //overovat, jestli se v jsonu neco nezmenilo //113_2022_12_11_2023_12_09.....
                                                                                  let monthValidityStart x = Parsing.parseMe(item.Substring(9 + x, 2))
                                                                                  let dayValidityStart x = Parsing.parseMe(item.Substring(12 + x, 2))

                                                                                  let yearValidityEnd x = Parsing.parseMe(item.Substring(15 + x, 4))
                                                                                  let monthValidityEnd x = Parsing.parseMe(item.Substring(20 + x, 2))
                                                                                  let dayValidityEnd x = Parsing.parseMe(item.Substring(23 + x, 2))

                                                                                  item, new DateTime(yearValidityStart x, monthValidityStart x, dayValidityStart x) 
                                                                                  //item, new DateTime(yearValidityEnd x, monthValidityEnd x, dayValidityEnd x) //pro pripadnou zmenu logiky
                                                                              with 
                                                                              | _ -> item, currentTime
                                                                   ) |> List.maxBy snd                                                        
                                                   [ fst latestValidityStart ]                                                   
                            ) |> List.distinct                              
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor 
        
    let myList3 = //P
        myList2 |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))
  
    let myList4 = //I
        let myFunction x = //P
            myList3 
            |> List.map (fun (item: string) ->     
                                             let item = string item   
                                             let str = item
                                             let str =
                                                 match str.Substring(0, 2).Equals("00") with
                                                 | true   -> str.Remove(0, 2)
                                                 | false  ->
                                                             match str.Substring(0, 1).Equals("0") || str.Substring(0, 1).Equals("_") with
                                                             | false -> item
                                                             | true  -> str.Remove(0, 1)                                                                                  
                                             
                                             let link = 
                                                 match item.Contains("_t") with 
                                                 | true  -> (sprintf "%s%s%s" pathKodisAmazonLink @"timetables/" str).Replace("_t", String.Empty)
                                                 | false -> sprintf "%s%s" pathKodisAmazonLink str                                                

                                             let path =     
                                                 match item.Contains("_t") with 
                                                 | true  -> 
                                                           let fileName = item.Substring(0, item.Length) //zatim bez generovaneho kodu, sem tam to zkontrolovat
                                                           sprintf "%s/%s" pathToDir fileName   
                                                 | false -> 
                                                           let fileName = item.Substring(0, item.Length - 15) //bez 15 znaku s generovanym kodem a priponou pdf dostaneme toto: 113_2022_12_11_2023_12_09 
                                                           sprintf "%s/%s%s" pathToDir fileName ".pdf"  //pdf opet musime pridat
                                                           
                                             link, path 
                        )
        tryWith myFunction (fun x -> ()) () String.Empty List.empty |> deconstructor   
    
    myList4 
    |> List.filter (fun item -> 
                              (not <| String.IsNullOrWhiteSpace(fst item) 
                              && 
                              not <| String.IsNullOrEmpty(fst item)) 
                              ||
                              (not <| String.IsNullOrWhiteSpace(snd item)
                              && 
                              not <| String.IsNullOrEmpty(snd item))                                         
                   ) |> List.sort                                             
        
let private deleteAllODISDirectories pathToDir = //I 

    let myDeleteFunction x = //I  

        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
        let dirInfo = new DirectoryInfo(pathToDir) |> optionToSRTP "Error8" (new DirectoryInfo(pathToDir))             
       
        //smazeme pouze adresare obsahujici stare JR, ostatni ponechame   
        let deleteIt = 
            dirInfo.EnumerateDirectories()
            |> optionToSRTP "Error11g" Seq.empty  
            |> Array.ofSeq
            |> Array.filter (fun item -> (getDefaultRecordValues |> Array.contains item.Name)) //prunik dvou kolekci (plus jeste Array.distinct pro unique items)
            |> Array.distinct 
            |> Array.Parallel.iter (fun item -> item.Delete(true))     
        deleteIt 
        
    tryWith myDeleteFunction (fun x -> ()) () String.Empty () |> deconstructor

    msg10() 
    msg11() 
 
let private listOfNewDirectories pathToDir = 
     
    getDefaultRecordValues 
    |> List.ofArray
    |> List.map (fun item -> sprintf"%s\%s"pathToDir item) 

let private deleteOneODISDirectory param pathToDir = //I 

    //smazeme pouze jeden adresar obsahujici stare JR, ostatni ponechame 
    let dirName =
        match param with 
        | CurrentValidity           -> getDefaultRecordValues |> Array.item 0
        | FutureValidity            -> getDefaultRecordValues |> Array.item 1
        | ReplacementService        -> getDefaultRecordValues |> Array.item 2                                
        | WithoutReplacementService -> getDefaultRecordValues |> Array.item 3

    let myDeleteFunction x = //I  

        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
        let dirInfo = new DirectoryInfo(pathToDir) |> optionToSRTP "Error8" (new DirectoryInfo(pathToDir))        
       
        dirInfo.EnumerateDirectories()
        |> optionToSRTP "Error11h" Seq.empty  
        |> Seq.filter (fun item -> item.Name = dirName) 
        |> Seq.iter (fun item -> item.Delete(true)) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce
                  
    tryWith myDeleteFunction (fun x -> ()) () String.Empty () |> deconstructor

    msg10() 
    msg11() 
    
    dirName   

let private newDirectory pathToDir dirName = [ sprintf"%s\%s"pathToDir dirName ] //list -> aby bylo mozno pouzit funkci createFolders bez uprav  

let private createFolders dirList = //I 

   let myFolderCreation x = //I  
       dirList |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)  
              
   tryWith myFolderCreation (fun x -> ()) () String.Empty () |> deconstructor   

let private downloadAndSaveTimetables pathToDir (filterTimetables: (string*string) list) = //I     

    let downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =   //I 

            async
                {   //muj custom-made tryWith nezachyti exception u async
                    //info about the complexity of concurrent downloading https://stackoverflow.com/questions/6219726/throttled-async-download-in-f
                    try 
                        let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                        use fileStream = new FileStream(path, FileMode.CreateNew) //|> (optionToGenerics "Error9" (new FileStream(path, FileMode.CreateNew))) //nelze, vytvari to dalsi stream a uklada to znovu                                
                        return! stream.CopyToAsync(fileStream) |> Async.AwaitTask                        
                    with 
                    | :? AggregateException as ex -> 
                                                     msgParam2 uri //printfn "\n%s%s" "Jizdni rad s timto odkazem se nepodarilo stahnout: \n" uri  //msgParam2
                                                     return()                                              
                    | ex                          -> 
                                                     deconstructorError <| msgParam1 ex <| client.Dispose()
                                                     return()                                
                }     

    //tryWith je ve funkci downloadFileTaskAsync
    msgParam3 pathToDir 

    let downloadTimetables1() = //I  //sequential?

        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  //Array.Parallel.iter tady nelze  
                                                 progressBarContinuous i l
                                                 async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously  
                      )    

    let downloadTimetables2() = //I  //concurrent?

        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  //Array.Parallel.iter tady nelze  
                                                 let dispatch = 
                                                     async                                                 
                                                         {
                                                             progressBarContinuous i l
                                                             async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously
                                                             //async { printfn"%s" pathToFile; return! Async.Sleep 0 } |> Async.RunSynchronously
                                                         }
                                                 Async.StartImmediate dispatch 
                      )                           
   
    //progressBarIndeterminate <| downloadTimetables2()  

    downloadTimetables2() //progressBarContinuous
    
    msgParam4 pathToDir 

let webscraping1_KODIS pathToDir (variantList: Validity list) = //I  
    
    let x variant dir = 
        match dir |> Directory.Exists with 
        | false -> 
                   msgParam5 dir 
                   msg13()                                                
        | true  ->                  
                   digThroughJsonStructure >> filterTimetables variant dir >> downloadAndSaveTimetables dir <| ()  

    processStart >> downloadAndSaveUpdatedJson <| ()     

    match variantList |> List.length with
    | 1 -> 
           let variant = variantList |> List.head
           let dirName = deleteOneODISDirectory variant pathToDir 
           let dirList = newDirectory pathToDir dirName //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
           createFolders dirList
           x variant (dirList |> List.head)              
    | _ ->  
           deleteAllODISDirectories pathToDir
           let dirList = listOfNewDirectories pathToDir
           createFolders dirList
           (variantList, dirList)
           ||> List.iter2 (fun variant dir -> x variant dir)            
    
    |> (client.Dispose >> processEnd)
    

    //CurrentValidity = JR striktne platne k danemu dni, tj. pokud je napr. na dany den vylukovy JR, stahne se tento JR, ne JR platny dalsi den
    //FutureValidity = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu
    //ReplacementService = pouze vylukove JR, JR NAD a JR X linek
    //WithoutReplacementService = JR dlouhodobe platne bez jakykoliv vyluk. Tento vyber neobsahuje ani dlouhodobe nekolikamesicni vyluky, muze se ale hodit v pripade, ze zakladni slozka s JR obsahuje jedno ci dvoudenni vylukove JR. 
   
    //Vzhledem k nekonzistnosti retezce s udaji o lince a platnosti muze dojit ke stazeni i neceho, co do daneho vyberu nepatri