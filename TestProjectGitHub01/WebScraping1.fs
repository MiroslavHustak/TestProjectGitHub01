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

//************************Helpers**********************************************************************

let [<Literal>] pathJson = @"e:/E/Mirek po osme hodine a o vikendech/kodis.json" //musi byt forward slash

type KodisTimetables = JsonProvider<pathJson> 

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
    
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =
    //TryWith   
    async
        {
            let! stream = client.GetStreamAsync(uri) 
                          |> Async.AwaitTask 
            let fileStream = new FileStream(path, FileMode.CreateNew)
            let! responseBody = stream.CopyToAsync(fileStream)
                                |> Async.AwaitTask 
            return responseBody
        } 

//************************main code***********************************************************

let private saveCurrentJson() = 

    let kodisJson =   
        //TryWith
        async 
            {     
                let! response = 
                    client.GetStringAsync("https://kodisweb-backend.herokuapp.com/linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label")
                    |> Async.AwaitTask
                return response
            }  
        |> Async.RunSynchronously

    //TryWith   
    use streamWriter = new StreamWriter(Path.GetFullPath($@"e:\E\Mirek po osme hodine a o vikendech\kodis.json"))                   
    streamWriter.WriteLine(kodisJson)     
    streamWriter.Flush()        

let private myKodisTP() = 
    //TryWith
    let kodisJsonSamples = KodisTimetables.GetSamples() |> Option.ofObj      

    let kodisTimetables() = 
        kodisJsonSamples
        |> function 
            | Some value -> value |> Array.map (fun item -> item.Timetable) 
            | None       -> printfn "%s" "Error5"
                            Array.empty

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

    let kodisAttachments() =  
        kodisJsonSamples 
        |> function 
            | Some value -> value |> Array.collect fn3 
            | None       -> printfn "%s" "Error6"
                            Array.empty       

    let myLinksSet = (Array.append <| kodisAttachments() <| kodisTimetables()) |> Set.ofArray //konverzi na Set vyhodime stejne polozky  
    
    //printfn "Pocet odkazu na JR  %i" myLinksSet.Count

    let pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\" 

    //TryWith
    let dirInfo = new DirectoryInfo(pathToDir)
                  |> Option.ofObj
                  |> function 
                      | Some value -> value  
                      | None       -> printfn "%s" "Error8"
                                      new DirectoryInfo(pathToDir)       
    //TryWith 
    dirInfo.EnumerateFiles() |> Array.ofSeq |> Array.Parallel.iter(fun item -> item.Delete()) //smazeme stare soubory v adresari 


    //****************castecne vytrizeni odkazu na neplatne jizdni rady (jen to, co jde jednoduse)***********************

    let currentTime = Fugit.now()

    let mySortedLinksSet = 
        myLinksSet |> Set.toArray 
        |> Array.Parallel.map (fun item ->                                    
                                         let fileName =                                     
                                                match item.Contains("timetables") with
                                                | true  -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty).Replace("timetables", String.Empty)
                                                           s.Substring(0, s.Length - 4).Remove(0, 2) + "_t"                                                 
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
                                                                  let dateOld = new DateTime(yearOld, monthOld, dayOld)
                                                                  let dateNew = new DateTime(yearNew, monthNew, dayNew)                                   

                                                                  //printfn"%s" lineNumberStr

                                                                  let cond1 = lineNumberInt < 999 && lineNumberInt > 0 //vytridime castecne pouze linky 1 az 999
                                                                  let cond2 = ((currentTime |> Fugit.isAfter dateNew)
                                                                              &&
                                                                              (currentTime |> Fugit.isAfter dateOld))
                                                                              || 
                                                                              (currentTime |> Fugit.isAfter dateNew)
                                                          
                                                                  match cond1 with
                                                                  | false -> item, fileNameFull                                               
                                                                  | true  ->
                                                                           match cond2 with
                                                                           | false -> item, fileNameFull                                               
                                                                           | true  -> String.Empty, String.Empty                                

                                         let pathToFile = 
                                             match link = String.Empty with
                                             | true  -> String.Empty
                                             | false -> sprintf"%s/%s%s" pathToDir fileName ".pdf"
                                         link, pathToFile                                 
                    ) |> Set.ofArray    

    mySortedLinksSet |> Set.toList |> List.sort |> List.tail 
    |> List.iter (fun (link, pathToFile) -> 
                                            //TryWith
                                            async 
                                                {  
                                                    let! responseBody = Async.Sleep 0
                                                    //printfn"%s" link
                                                        //downloadFileTaskAsync client link pathToFile    //Parallel vyhazuje chyby                                           
                                                    return responseBody
                                                }  
                                            |> Async.RunSynchronously
                                          
                )
           
let webscraping1() =    
    saveCurrentJson() |> processStart |> myKodisTP |> processEnd |> client.Dispose       
    
   

