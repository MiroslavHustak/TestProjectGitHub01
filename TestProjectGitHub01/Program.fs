//namespace TestProject

open System
open System.Data

open Csv
open Settings

open WebScraping1_DPO
open WebScraping1_MDPO
open WebScraping1_KODIS

open WebScraping2
open WebScraping3
open CodeChallenge
open TryWith.TryWith
open DiscriminatedUnions
open BrowserDialogWindow

[<EntryPoint; STAThread>]
let main argv =

    //*****************************Console******************************   
    
    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode
    
    //*****************************WebScraping1******************************   

    let myWebscraping1_DPO x = 
        Console.Clear()
        printfn "Hromadne stahovani aktualnich JR ODIS (vcetne vyluk) dopravce DP Ostrava z webu https://www.dpo.cz"           
        printfn "Datum posledni aktualizace SW: 01-06-2023" 
        printfn "********************************************************************"
        printfn "Nyni je treba vybrat si adresar pro ulozeni JR dopravce DP Ostrava."
        printfn "Pokud ve vybranem adresari existuje nasledujici podadresar, jeho obsah bude nahrazen nove stahnutymi JR."
        printfn "[%s]" <| ODIS.Default.odisDir5       
        printfn "%c" <| char(32) 
        printfn "Precti si pozorne vyse uvedene a stiskni bud ENTER pro vybrani adresare anebo krizkem ukonci aplikaci."
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
       
        printfn "Sqele! Adresar byl vybran. Nyni stiskni cokoliv pro stazeni aktualnich JR dopravce DP Ostrava."
                              
        Console.Clear()

        webscraping1_DPO (string pathToFolder) 
                
        printfn "%c" <| char(32)   
        printfn "Stiskni cokoliv pro ukonceni aplikace."
        Console.ReadKey() |> ignore

    let myWebscraping1_MDPO x = 
        Console.Clear()
        printfn "Hromadne stahovani aktualnich JR ODIS dopravce MDP Opava z webu https://www.mdpo.cz"           
        printfn "Datum posledni aktualizace SW: 02-06-2023" 
        printfn "********************************************************************"
        printfn "Nyni je treba vybrat si adresar pro ulozeni JR dopravce MDP Opava."
        printfn "Pokud ve vybranem adresari existuje nasledujici podadresar, jeho obsah bude nahrazen nove stahnutymi JR."
        printfn "[%s]" <| ODIS.Default.odisDir6       
        printfn "%c" <| char(32) 
        printfn "Precti si pozorne vyse uvedene a stiskni bud ENTER pro vybrani adresare anebo krizkem ukonci aplikaci."
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
              
        printfn "Sqele! Adresar byl vybran. Nyni stiskni cokoliv pro stazeni aktualnich JR dopravce MDP Opava."
                                     
        Console.Clear()

        webscraping1_MDPO (string pathToFolder) 
                       
        printfn "%c" <| char(32)   
        printfn "Stiskni cokoliv pro ukonceni aplikace."
        Console.ReadKey() |> ignore

    let myWebscraping1_KODIS x = 
           Console.Clear()
           printfn "Hromadne stahovani JR ODIS vsech dopravcu v systemu ODIS z webu https://www.kodis.cz"           
           printfn "Datum posledni aktualizace SW: 30-05-2023" 
           printfn "********************************************************************"
           printfn "Nyni je treba vybrat si adresar pro ulozeni JR vsech dopravcu v systemu ODIS."
           printfn "Pokud ve vybranem adresari existuji nasledujici podadresare, jejich obsah bude nahrazen nove stahnutymi JR."
           printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir1
           printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir2
           printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir3
           printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir4  
           printfn "%c" <| char(32) 
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
          
           printfn "Sqele! Adresar byl vybran. Nyni prosim zadej cislici plus ENTER pro vyber varianty."
           printfn "%c" <| char(32)
           printfn "1 = Aktualni JR strikne platne dnesni den, tj. pokud je napr. pro dnesni den"
           printfn "%4cplatny pouze urcity jednodenni vylukovy JR, stahne se tento JR, ne JR platny od dalsiho dne." <| char(32)
           printfn "2 = JR (vcetne vylukovych JR) platne az v budouci dobe, ktere se vsak uz nyni vyskytuji na webu KODISu."
           printfn "3 = Pouze aktualni vylukove JR, JR NAD a JR X linek (kratkodobe i dlouhodobe)."
           printfn "4 = JR teoreticky dlouhodobe platne bez jakykoliv (i dlouhodobych) vyluk ci NAD."
           printfn "%c" <| char(32) 
           printfn "Jakakoliv jina klavesa = KOMPLETNI stahnuti vsech variant JR.\r"        
           printfn "%c" <| char(32) 
           printfn "%c" <| char(32) 
           printfn "Staci stisknout ENTER pro KOMPLETNI stahnuti vsech variant JR."

           let variant = 
               Console.ReadLine()
               |> function 
                   | "1" -> [ CurrentValidity ]
                   | "2" -> [ FutureValidity ]  
                   | "3" -> [ ReplacementService ]
                   | "4" -> [ WithoutReplacementService ]
                   | _   -> [ CurrentValidity; FutureValidity; ReplacementService; WithoutReplacementService ]
                  
           Console.Clear()

           webscraping1_KODIS (string pathToFolder) variant 
           
           printfn "%c" <| char(32)  
           printfn "Udaje KODISu nemaji konzistentni retezec, proto mohlo dojit ke stazeni i neceho, co do daneho vyberu nepatri."
           printfn "JR s chybejicimi udaji o platnosti (napr. NAD bez dalsich udaju) nebyly stazeny."
           printfn "JR s chybnymi udaji o platnosti pravdepodobne nebyly stazeny (zalezi na druhu chyby)."
           printfn "%c" <| char(32)   
           printfn "Stiskni cokoliv pro ukonceni aplikace."
           Console.ReadKey() |> ignore    

    let rec variant() = 

        printfn "Zdravim nadsence do klasickych jizdnich radu. Nyni prosim zadejte cislici plus ENTER pro vyber varianty."        
        printfn "1 = Hromadne stahovani jizdnich radu ODIS pouze dopravce DP Ostrava z webu https://www.dpo.cz"
        printfn "2 = Hromadne stahovani jizdnich radu ODIS pouze dopravce MDP Opava z webu https://www.mdpo.cz"
        printfn "3 = Hromadne stahovani jizdnich radu ODIS vsech dopravcu v systemu ODIS z webu https://www.kodis.cz"

        Console.ReadLine()
        |> function 
            | "1" -> tryWith myWebscraping1_DPO (fun x -> ()) () String.Empty () |> deconstructor
            | "2" -> tryWith myWebscraping1_MDPO (fun x -> ()) () String.Empty () |> deconstructor    
            | "3" -> tryWith myWebscraping1_KODIS (fun x -> ()) () String.Empty () |> deconstructor    
            | _   ->
                     printfn "Varianta nebyla vybrana. Prosim zadej znovu."
                     variant()

    
    //*****************************WebScraping1**********************************************
    variant() 
   
    //*****************************WebScraping2**********************************************
    //normalScraping()

    //*****************************WebScraping3**********************************************
    //webscrapingFromPage()

    //*****************************CodeChallenge*********************************************
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