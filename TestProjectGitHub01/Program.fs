//namespace TestProject

open System
open System.Data
open System.Threading

open Csv
open WebScraping1
open WebScraping2
open WebScraping3
open CodeChallenge
open TryWith.TryWith
open DiscriminatedUnions
open BrowserDialogWindow

[<EntryPoint; STAThread>]
let main argv =
    
    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode
    
    //*****************************WebScraping1******************************

    let myWebscraping1 x = 
        printfn "Hromadne stahovani jizdnich radu ODIS z webu https://www.kodis.cz"
        printfn "*****************************************************************"
        printfn "Vyber si adresar pro ulozeni jizdnich radu. Ve vybranem adresari bude vymazan jeho soucasny obsah!!!"
        printfn "Precti si pozorne vyse uvedene a bud stiskni ENTER pro vybrani adresare anebo krizkem ukonci aplikaci."
        Console.ReadKey() |> ignore
    
        let pathToFolder = 
            let (str, value) = openFolderBrowserDialog()
            match value with
            | false                           -> str       
            | true when (<>) str String.Empty -> 
                                                Console.Clear()
                                                printfn"%s%s" "\nNo jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" str
                                                do Console.ReadKey() |> ignore 
                                                do System.Environment.Exit(1)  
                                                String.Empty
            | _                               -> 
                                                Console.Clear()
                                                printfn "\nNebyl vybran adresar. Zmackni cokoliv pro ukonceni programu. \n"
                                                do Console.ReadKey() |> ignore 
                                                do System.Environment.Exit(1) 
                                                String.Empty  
   
        Console.Clear()
       
        printfn "Sqele! Adresar byl vybran. Nyni zadej cislici plus ENTER pro vyber varianty."
        printfn "2 = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu."
        printfn "3 = Pouze aktualni vylukove JR, JR NAD a JR X linek."
        printfn "4 = JR dlouhodobe platne bez jakykoliv vyluk. Mohou se hodit v pripade,"
        printfn "%4ckdy zakladni varianta obsahuje jedno ci dvoudenni vylukove JR." <| char(32)
        printfn "%c" <| char(32) 
        printfn "Jakakoliv jina klavesa = ZAKLADNI VARIANTA, tj. JR strikne platne dnesni den, tj. pokud je napr. pro dnesni"
        printfn "den platny pouze jednodenni vylukovy JR, stahne se tento JR, ne JR platny dalsi den.\r"
        printfn "%c" <| char(32) 
        printfn "%c" <| char(32) 
        printfn "Staci stisknout ENTER pro zakladni variantu."

        let variant = 
            Console.ReadLine()
            |> function 
                | "2" -> FutureValidity
                | "3" -> ReplacementService
                | "4" -> WithoutReplacementService
                | _   -> CurrentValidity
               
        Console.Clear()
        webscraping1 pathToFolder variant 
        
        printfn "%c" <| char(32)  
        printfn "Udaje KODISu nemaji konzistentni retezec, proto mohlo dojit ke stazeni i neceho, co do daneho vyberu nepatri."
        printfn "A naopak, JR bez spravneho retezce s udaji o platnosti (napr. NAD bez dalsich udaju) stazeny nebudou."
        printfn "%c" <| char(32)   
        printfn "Stiskni cokoliv pro ukonceni aplikace."
        Console.ReadKey() |> ignore

    tryWith myWebscraping1 (fun x -> ()) () String.Empty () |> deconstructor

    //*****************************WebScraping2******************************
    //normalScraping()

    //*****************************WebScraping3****************************** 
    //webscrapingFromPage()

    //*****************************CodeChallenge*****************************
    //codeChallenge()

    //*****************************Vyukovy kod (priklad s csv) ****************************** 
    
    let example() = 

        let readDataFromExcel = 
            readDataFromExcel() 
            |> function
                | Some value -> value
                | None       -> 
                                do System.Environment.Exit(1)  //simulace reseni situace (muze byt napr. nejaka default hodnota)                                 
                                new DataTable() //whatever  

        readDataFromExcel |> writeIntoCSV @"e:\E\Mirek po osme hodine a o vikendech\" @"LT-15381 az LT-17691 DGSada 03-04-2023" |> ignore

        do Console.ReadKey() |> ignore

    //example()

    //****************************************************************************************

    0 // return an integer exit code