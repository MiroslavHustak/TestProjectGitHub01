module WebScraping1

open System
open System.IO
open System.Net
open FSharp.Data

do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
Console.BackgroundColor <- ConsoleColor.Blue 
Console.ForegroundColor <- ConsoleColor.White 
Console.InputEncoding   <- System.Text.Encoding.Unicode
Console.OutputEncoding  <- System.Text.Encoding.Unicode

//******************************************************************************************************************

let [<Literal>] pathJson = @"e:/E/Mirek po osme hodine a o vikendech/kodis.json" //musi byt forward slash

type KodisTimetables = JsonProvider<pathJson> 

let private errorFn str err = 
    str
    |> Option.ofObj
    |> function 
        | Some value -> value
        | None       -> printfn "%s" err
                        String.Empty   

let private timeStr = errorFn "HH:mm:ss" "Error1"                     
    
let private processStart() =     
    let processStartTime = errorFn $"Začátek procesu: {DateTime.Now.ToString(timeStr)}" "Error2"                           
    printfn "%s" processStartTime
    
let private processEnd() =     
    let processEndTime = errorFn $"Konec procesu: {DateTime.Now.ToString(timeStr)}" "Error3"                       
    printfn "%s" processEndTime

let private client = 
    new System.Net.Http.HttpClient() 
    |> Option.ofObj
    |> function 
        | Some value -> value
        | None       -> printfn "%s" "Error4"
                        new System.Net.Http.HttpClient() //whatever of the type
    
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =
    async
        {
            let! stream = client.GetStreamAsync(uri) 
                            |> Async.AwaitTask 
            let fileStream = new FileStream(path, FileMode.CreateNew)
            let! responseBody = stream.CopyToAsync(fileStream)
                                |> Async.AwaitTask 
            return responseBody
        } 

//***********************************************************************************

let private saveCurrentJson () = 

    let kodisJson =         
        async 
            {     
                let! response = 
                    client.GetStringAsync("https://kodisweb-backend.herokuapp.com/linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label")
                    |> Async.AwaitTask
                return response
            }  
        |> Async.RunSynchronously
       
    use streamWriter = new StreamWriter(Path.GetFullPath($@"e:\E\Mirek po osme hodine a o vikendech\kodis.json"))                   
    streamWriter.WriteLine(kodisJson)     
    streamWriter.Flush()        

let private myKodisTP () = 

    let kodisJsonSamples = KodisTimetables.GetSamples() |> Option.ofObj      

    let kodisTimetables() = 
        kodisJsonSamples
        |> function 
            | Some value -> value  |> Array.map (fun item -> item.Timetable) 
            | None       -> printfn "%s" "Error5"
                            Array.empty

    (*
    //pro pochopeni struktury (pri pouziti option se to tahne az k susedovi)
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

    let myLinksSet = (Array.append <| kodisAttachments() <| kodisTimetables() ) |> Set.ofArray //konverzi na Set vyhodime stejne polozky  
    
    printfn "Pocet odkazu na JR  %i" myLinksSet.Count

    let pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODISTP\" 

    let dirInfo = new DirectoryInfo(pathToDir)

    dirInfo.EnumerateFiles() |> Array.ofSeq |> Array.Parallel.iter(fun item -> item.Delete()) //smazeme stare soubory v adresari 
       
    let mySecondSet = 
        myLinksSet //Parallel vyhazuje chyby, asi problem v tom, ze dana www neumoznuje tolik pozadavku na download zaroven
        |> Set.map (fun item ->                                    
                                let fileName =                                     
                                        match item.Contains("timetables") with
                                        | true  -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty).Replace("timetables", String.Empty)
                                                   s.Substring(0, s.Length - 4).Remove(0, 2) + "_extra"                                                 
                                        | false -> let s = item.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty)
                                                   s.Substring(0, s.Length - 15).Remove(0, 1)

                                let range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
                                let charList = fileName.ToCharArray() |> Array.toList |> List.take 3

                                let aux i range  = range |> List.filter (fun item -> charList |> List.item i = item)

                                let fileName = 
                                    match aux 0 range <> List.empty with
                                    | true  -> match aux 1 range <> List.empty with
                                                | true  -> match aux 2 range <> List.empty with
                                                            | true  -> fileName                                                                     
                                                            | false -> sprintf "%s%s" "0" fileName                    
                                                | false ->  sprintf "%s%s" "00" fileName
                                    | false -> fileName                            

                                item, sprintf"%s/%s%s" pathToDir fileName ".pdf"                                 
                    ) 

    mySecondSet 
    |> Set.iter(fun (link, pathToFile) -> 
                                        let downloadIt = 
                                            async 
                                                {     
                                                    let! responseBody = 
                                                        downloadFileTaskAsync client link pathToFile                                               
                                                    return responseBody
                                                }  
                                            |> Async.RunSynchronously
                                        downloadIt    
                )

let webscraping1() = saveCurrentJson() |> processStart |> myKodisTP |> processEnd |> client.Dispose       
    
   

