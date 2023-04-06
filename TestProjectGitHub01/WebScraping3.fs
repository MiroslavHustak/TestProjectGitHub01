module WebScraping3

open System
open System.IO
open System.Net
open FSharp.Data

//FOR LEARNING PURPOSES ONLY

//TODO try-with blocks 
//TODO validations  
//TODO Option.ofObj

let webscrapingWithSplittingJson() = 

    let client = new System.Net.Http.HttpClient()

    let myJson =         
        async 
            {     
                let! response = client.GetStringAsync("https://kodisweb-backend.herokuapp.com/linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label") 
                                |> Async.AwaitTask
                return response
            } 
        |> Async.RunSynchronously
      
    use streamWriter = new StreamWriter(Path.GetFullPath($@"e:\E\Mirek po osme hodine a o vikendech\kodis.json"))                   
    streamWriter.WriteLine(myJson)     
    streamWriter.Flush()      

    let charsToTrim = [| '\"'; ' '|]
   
    let myStringArray = myJson.Split(".pdf")
    let myStringArray1 = myStringArray
                        |> Array.collect (fun item -> item.Split("url")) 
                        |> Array.collect (fun item -> item.Split("timetable")) 
                        |> Array.collect (fun item -> item.Split("href"))        
                        |> Array.filter (fun item ->
                                                    let item1 = item + ".pdf"
                                                    
                                                    let cond1 = not <| item1.Contains(@"https://kodis-files.s3.eu-central-1.amazonaws.com/.pdf")
                                                    let cond2 = not <| item1.Contains("jpg")
                                                    let cond3 = item1.Contains @"https://kodis-files.s3.eu-central-1.amazonaws.com"  
                                                    
                                                    cond1 && cond2 && cond3
                                        )
    let myStringArray2 = myStringArray1 
                        |> Array.map (fun item -> item.Replace("\":\"", String.Empty).Replace("=\\", String.Empty).Replace("\"", String.Empty))                                                    
                                             
    let mySet =  myStringArray2 |> Set.ofArray //vyhodime stejne polozky  pomoci konverze na Set
   
    printfn "Pocet odkazu na JR  %i" mySet.Count
    mySet |> Set.iter (fun item -> ())//printfn "%s" item)

    let pathToDir = @"e:\E\Mirek po osme hodine a o vikendech\KODIS\" 
    
    let dirInfo = new DirectoryInfo(pathToDir)
    
    dirInfo.EnumerateFiles() |> Array.ofSeq |> Array.Parallel.iter(fun item -> item.Delete()) //smazeme stare soubory v adresari 

    let webClient = new WebClient() //TODO to be replaced by async HttpClient()

    let myList = mySet |> Set.toList

    myList
    |> List.iteri (fun i item -> 
                                let s = (myList |> List.item i)
                                let sPdf = s + ".pdf"
                                
                                let fileName = 
                                    let myString = s.Replace("https://kodis-files.s3.eu-central-1.amazonaws.com", String.Empty)
                                    myString.Substring(0, myString.Length - 11)
                                
                                //TODO to be replaced by async HttpClient()
                                webClient.DownloadFile(sPdf, @$"{pathToDir}{fileName}.pdf")
                        )        




