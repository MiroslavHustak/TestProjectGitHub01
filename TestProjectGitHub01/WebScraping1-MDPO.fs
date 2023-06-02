module WebScraping1_MDPO

open System
open System.IO
open System.Net

open FSharp.Data

open Settings
open TryWith.TryWith
open ProgressBarFSharp
open WebScraping1_Helpers

do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    
Console.BackgroundColor <- ConsoleColor.Blue 
Console.ForegroundColor <- ConsoleColor.White 
Console.InputEncoding   <- System.Text.Encoding.Unicode
Console.OutputEncoding  <- System.Text.Encoding.Unicode

//************************Constants and types**********************************************************************

let [<Literal>] pathMdpoWeb = @"https://www.mdpo.cz"

//************************Helpers**********************************************************************************

//Pro ostatni helpers viz WebScraping1-Helpers.fs

let client = client()    
        
let private downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =   //I 
    
        let errMsg ex = 
            printfn "\n%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" (string ex)
            do Console.ReadKey() |> ignore 
            do client.Dispose()
            do System.Environment.Exit(1)

        async
            {   
                //TODO vyzkusaj Async.Catch
                try //muj custom made tryWith nezachyti exception u async
                    let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                    use fileStream = new FileStream(path, FileMode.CreateNew) //|> (optionToGenerics "Error9" (new FileStream(path, FileMode.CreateNew))) //nelze, vytvari to dalsi stream a uklada to znovu                                
                    return! stream.CopyToAsync(fileStream) |> Async.AwaitTask 
                with 
                | :? AggregateException as ex -> 
                                                 printfn "\n%s%s" "Jizdni rad s timto odkazem se nepodarilo stahnout: \n" uri
                                                 return()                                              
                | ex                          -> 
                                                 errMsg ex
                                                 return()                                
            }     
 
//************************Main code***********************************************************
let private filterTimetables pathToDir = //I  
    
    let urlList = 
        [
            @"https://www.mdpo.cz/jizdni-rady"             
        ]
    
    urlList
    |> List.collect (fun url -> 
                              let document = FSharp.Data.HtmlDocument.Load(url)        
                           
                              document.Descendants "a"
                              |> Seq.choose (fun x ->
                                                    x.TryGetAttribute("href")
                                                    |> Option.map (fun a -> string <| x.InnerText(), string <| a.Value()) //inner text zatim nepotrebuji, cisla linek mam resena jinak                                           
                                            )      
                              |> Seq.filter (fun (_ , item2) -> item2.Contains @"/qr/" && item2.Contains ".pdf")
                              |> Seq.map (fun (_ , item2)    ->                                                                 
                                                                let linkToPdf = 
                                                                    sprintf"%s%s" pathMdpoWeb item2  //https://www.mdpo.cz // /qr/201.pdf
                                                                let lineName = item2.Replace(@"/qr/", String.Empty)  
                                                                let pathToFile = 
                                                                    sprintf "%s/%s" pathToDir lineName
                                                                linkToPdf, pathToFile
                                         )
                              |> Seq.toList
                              |> List.distinct
                    )    

let private deleteOneODISDirectory pathToDir = //I 

    //smazeme pouze jeden adresar obsahujici stare JR, ostatni ponechame 
    let dirName = ODIS.Default.odisDir6
      
    let myDeleteFunction x = //I  

        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
        let dirInfo = new DirectoryInfo(pathToDir) |> optionToSRTP "Error8" (new DirectoryInfo(pathToDir))        
       
        dirInfo.EnumerateDirectories()
        |> optionToSRTP "Error11h" Seq.empty  
        |> Seq.filter (fun item -> item.Name = dirName) 
        |> Seq.iter (fun item -> item.Delete(true)) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce
                  
    tryWith myDeleteFunction (fun x -> ()) () String.Empty () |> deconstructor

    printfn "Provedeno mazani starych JR v dane variante."
    
    //po vymazani stareho vytvorime novy podadresar
    [ sprintf"%s\%s"pathToDir dirName ] //list -> aby bylo mozno pouzit funkci createFolders bez uprav  

let private createFolders dirList = //I 

   let myFolderCreation x = //I  
       dirList |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)  
              
   tryWith myFolderCreation (fun x -> ()) () String.Empty () |> deconstructor   

let private downloadAndSaveTimetables pathToDir (filterTimetables: (string*string) list) = //I 
            
    //************************download pdf souboru, ktere jsou aktualni*******************************************
    
    //tryWith je ve funkci downloadFileTaskAsync
    printfn "Probiha stahovani prislusnych JR a jejich ukladani do [%s]." pathToDir 

    let downloadTimetables() = //I
        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  //Array.Parallel.iter tady nelze  
                                                 progressBarContinuous i l
                                                 async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously  
                                                 //async { printfn"%s" pathToFile; return! Async.Sleep 0 } |> Async.RunSynchronously
                      )    
   
    //progressBarIndeterminate <| downloadTimetables  

    downloadTimetables() //progressBarContinuous
    
    printfn "Dokonceno stahovani prislusnych JR a jejich ukladani do [%s]." pathToDir

let webscraping1_MDPO pathToDir = //I  
    
    let x dir = 
        match dir |> Directory.Exists with 
        | false -> 
                  printfn "Adresar [%s] neexistuje, prislusne JR do nej urceny nemohly byt stazeny." dir
                  printfn "Pravdepodobne nekdo dany adresar v prubehu prace tohoto programu smazal."                                                    
        | true  ->         
                  filterTimetables dir
                  |> downloadAndSaveTimetables dir 
    
    processStart()

    let dirList = deleteOneODISDirectory pathToDir //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
    createFolders dirList
    x (dirList |> List.head)                   
    
    |> (client.Dispose >> processEnd)

