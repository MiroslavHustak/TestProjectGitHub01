﻿//namespace TestProject

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
open Messages.Messages
open DiscriminatedUnions
open BrowserDialogWindow
open WebScraping1_Helpers
open ErrorFunctions.ErrorFunctions

[<EntryPoint; STAThread>]
let main argv =

    //*****************************Console******************************   
    
    consoleAppProblemFixer()
    
    //*****************************WebScraping1******************************   

    let myWebscraping1_DPO x =
        Console.Clear()
        printfn "Hromadné stahování aktuálních JŘ ODIS (včetně výluk) dopravce DP Ostrava z webu https://www.dpo.cz"
        printfn "Datum poslední aktualizace SW: 14-06-2023"
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ dopravce DP Ostrava."
        printfn "Pokud ve vybraném adresáři existuje následující podadresář, jeho obsah bude nahrazen nově staženými JŘ."
        printfn "[%s]" <| ODIS.Default.odisDir5
        printfn "%c" <| char(32)
        printfn "Přečti si pozorně výše uvedené a stiskni buď ENTER pro výběr adresáře anebo křížkem ukonči aplikaci."
        Console.ReadKey() |> ignore

        let pathToFolder =
            let (str, value) = openFolderBrowserDialog()
            match value with
            | false                           -> str
            | true when (<>) str String.Empty ->
                                                Console.Clear()
                                                deconstructorError <| msgParam1 str <| ()
                                                String.Empty
            | _                               ->
                                                Console.Clear()
                                                deconstructorError <| printfn "\nNebyl vybrán adresář. Zmačkni cokoliv pro ukončení programu. \n" <| ()
                                                String.Empty

        Console.Clear()

        printfn "Skvěle! Adresář byl vybrán. Nyní stiskni cokoliv pro stažení aktuálních JŘ dopravce DP Ostrava."

        Console.Clear()

        webscraping1_DPO (string pathToFolder)

        printfn "%c" <| char(32)
        printfn "Stiskni cokoliv pro ukončení aplikace."
        Console.ReadKey() |> ignore

    let myWebscraping1_MDPO x = 
        Console.Clear()
        printfn "Hromadné stahování aktuálních JŘ ODIS dopravce MDP Opava z webu https://www.mdpo.cz"           
        printfn "Datum poslední aktualizace SW: 14-06-2023" 
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ dopravce MDP Opava."
        printfn "Pokud ve vybraném adresáři existuje následující podadresář, jeho obsah bude nahrazen nově staženými JŘ."
        printfn "[%s]" <| ODIS.Default.odisDir6       
        printfn "%c" <| char(32) 
        printfn "Přečti si pozorně výše uvedené a stiskni buď ENTER pro výběr adresáře anebo křížkem ukonči aplikaci."
        Console.ReadKey() |> ignore 
           
        let pathToFolder = 
            let (str, value) = openFolderBrowserDialog()
            match value with
            | false                           -> str       
            | true when (<>) str String.Empty -> 
                                                Console.Clear()
                                                deconstructorError <| msgParam1 str <| ()   
                                                String.Empty
            | _                               -> 
                                                Console.Clear()
                                                deconstructorError <| printfn "\nNebyl vybrán adresář. Zmačkni cokoliv pro ukončení programu. \n" <| ()
                                                String.Empty  
        
        Console.Clear()
              
        printfn "Skvěle! Adresář byl vybrán. Nyní stiskni cokoliv pro stažení aktuálních JŘ dopravce MDP Opava."
                                     
        Console.Clear()
    
        webscraping1_MDPO (string pathToFolder) 
                       
        printfn "%c" <| char(32)   
        printfn "Stiskni cokoliv pro ukončení aplikace."
        Console.ReadKey() |> ignore
    
    let myWebscraping1_KODIS x = 
        Console.Clear()
        printfn "Hromadné stahování JŘ ODIS všech dopravců v systému ODIS z webu https://www.kodis.cz"           
        printfn "Datum poslední aktualizace SW: 14-06-2023" 
        printfn "********************************************************************"
        printfn "Nyní je třeba vybrat si adresář pro uložení JŘ všech dopravců v systému ODIS."
        printfn "Pokud ve vybraném adresáři existují následující podadresáře, jejich obsah bude nahrazen nově staženými JŘ."
        printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir1
        printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir2
        printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir3
        printfn "%4c[%s]" <| char(32) <| ODIS.Default.odisDir4  
        printfn "%c" <| char(32) 
        printfn "Přečti si pozorně výše uvedené a buď stiskni ENTER pro výběr adresáře anebo křížkem ukonči aplikaci."
        Console.ReadKey() |> ignore 
      
        let pathToFolder = 
            let (str, value) = openFolderBrowserDialog()
            match value with
            | false                           -> str       
            | true when (<>) str String.Empty -> 
                                                Console.Clear()
                                                deconstructorError <| msgParam1 str <| ()   
                                                String.Empty
            | _                               -> 
                                                Console.Clear()
                                                deconstructorError <| printfn "\nNebyl vybrán adresář. Zmačkni cokoliv pro ukončení programu. \n" <| ()
                                                String.Empty  
    
        Console.Clear()
    
          
        printfn "Skvěle! Adresář byl vybrán. Nyní prosím vyber variantu (číslice plus ENTER, příp. jen ENTER pro kompletně všechno)."
        printfn "%c" <| char(32)
        printfn "1 = Aktuální JŘ, které striktně platí dnešní den, tj. pokud je např. pro dnešní den"
        printfn "%4cplatný pouze určitý jednodenní výlukový JŘ, stáhne se tento JŘ, ne JŘ platný od dalšího dne." <| char(32)
        printfn "2 = JŘ (včetně výlukových JŘ), platné až v budoucí době, které se však už nyní vyskytují na webu KODISu."
        printfn "3 = Pouze aktuální výlukové JŘ, JŘ NAD a JŘ X linek (krátkodobé i dlouhodobé)."
        printfn "4 = JŘ teoreticky dlouhodobě platné bez jakýchkoliv (i dlouhodobých) výluk či NAD."
        printfn "%c" <| char(32) 
        printfn "Jakákoliv jiná klávesa = KOMPLETNÍ stažení všech variant JŘ.\r"        
        printfn "%c" <| char(32) 
        printfn "%c" <| char(32) 
        printfn "Stačí stisknout ENTER pro KOMPLETNÍ stažení všech variant JŘ."
           
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
        printfn "Údaje KODISu nemají konzistentní řetězec, proto mohlo dojít i ke stažení něčeho, co do daného výběru nepatří."
        printfn "JŘ s chybějícími údaji o platnosti (např. NAD bez dalších údajů) nebyly staženy."
        printfn "JŘ s chybnými údaji o platnosti pravděpodobně nebyly staženy (záleží na druhu chyby)."
        printfn "%c" <| char(32)   
        printfn "Stiskni cokoliv pro ukončení aplikace."
        Console.ReadKey() |> ignore    
           
    let rec variant() = 
           
        printfn "Zdravím nadšence do klasických jízdních řádů. Nyní prosím zadejte číslici plus ENTER pro výběr varianty."        
        printfn "1 = Hromadné stahování jízdních řádů ODIS pouze dopravce DP Ostrava z webu https://www.dpo.cz"
        printfn "2 = Hromadné stahování jízdních řádů ODIS pouze dopravce MDP Opava z webu https://www.mdpo.cz"
        printfn "3 = Hromadné stahování jízdních řádů ODIS všech dopravců v systému ODIS z webu https://www.kodis.cz"
           
        Console.ReadLine()
        |> function 
            | "1" -> tryWith myWebscraping1_DPO (fun x -> ()) () String.Empty () |> deconstructor msgParam1
            | "2" -> tryWith myWebscraping1_MDPO (fun x -> ()) () String.Empty () |> deconstructor msgParam1    
            | "3" -> tryWith myWebscraping1_KODIS (fun x -> ()) () String.Empty () |> deconstructor msgParam1   
            | _   ->
                     printfn "Varianta nebyla vybrána. Prosím zadej znovu."
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