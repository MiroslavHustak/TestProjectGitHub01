module WebScraping1_MDPO

open System
open System.IO
open System.Net

open FSharp.Data

open Settings
open TryWith.TryWith
open ProgressBarFSharp
open Messages.Messages
open WebScraping1_Helpers
open ErrorFunctions.ErrorFunctions

//************************Console**********************************************************************************

consoleAppProblemFixer()

//************************Constants and types**********************************************************************

let [<Literal>] pathMdpoWeb = @"https://www.mdpo.cz"

//************************Helpers**********************************************************************************

//Pro ostatni helpers viz WebScraping1-Helpers.fs

let private client = client()  
 
//************************Main code********************************************************************************

let private filterTimetables pathToDir = //I  
    
    let urlList = 
        [
            @"https://www.mdpo.cz/jizdni-rady"             
        ]
    
    urlList
    |> List.collect (fun url -> 
                              let document = FSharp.Data.HtmlDocument.Load(url)        
                           
                              document.Descendants "a"
                              |> Seq.choose (fun htmlNode ->
                                                           htmlNode.TryGetAttribute("href") //inner text zatim nepotrebuji, cisla linek mam resena jinak 
                                                           |> Option.map (fun a -> string <| htmlNode.InnerText(), string <| a.Value())                                           
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

    msg12() 
    
    dirName  
    
let private newDirectory pathToDir dirName = [ sprintf"%s\%s"pathToDir dirName ] //list -> aby bylo mozno pouzit funkci createFolders bez uprav    

let private createFolders dirList = //I 

   let myFolderCreation x = //I  
       dirList |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)  
              
   tryWith myFolderCreation (fun x -> ()) () String.Empty () |> deconstructor   

let private downloadAndSaveTimetables pathToDir (filterTimetables: (string*string) list) = //I 

    let downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =   //I         
               
            async
                {   
                    try //muj custom made tryWith nezachyti exception u async
                        let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                        use fileStream = new FileStream(path, FileMode.CreateNew) //|> (optionToGenerics "Error9" (new FileStream(path, FileMode.CreateNew))) //nelze, vytvari to dalsi stream a uklada to znovu                                
                        return! stream.CopyToAsync(fileStream) |> Async.AwaitTask 
                    with 
                    | :? AggregateException as ex -> 
                                                     msgParam2 uri 
                                                     return()                                              
                    | ex                          -> 
                                                     deconstructorError <| msgParam1 ex <| client.Dispose()
                                                     return()                                
                }  
    
    //tryWith je ve funkci downloadFileTaskAsync
    msgParam3 pathToDir 

    let downloadTimetables() = //I
        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  
                                                 let dispatch = 
                                                     async                                                 
                                                         {
                                                             progressBarContinuous i l
                                                             async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously
                                                         }
                                                 Async.StartImmediate dispatch 
                      )    
   
    //progressBarIndeterminate <| downloadTimetables  

    downloadTimetables() //progressBarContinuous
    
    msgParam4 pathToDir 

let webscraping1_MDPO pathToDir = //I  
    
    let x dir = 
        match dir |> Directory.Exists with 
        | false -> 
                  msgParam5 dir 
                  msg13()                                              
        | true  -> 
                  filterTimetables >> downloadAndSaveTimetables dir <| dir  
    
    processStart()
    let dirName = deleteOneODISDirectory pathToDir 
    let dirList = newDirectory pathToDir dirName //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
    createFolders dirList
    x (dirList |> List.head) 
    |> (client.Dispose >> processEnd)

