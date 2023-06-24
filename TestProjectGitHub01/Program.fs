//namespace TestProject

open System
open System.Data

open Csv
open Settings

open WebScraping2
open WebScraping3
open CodeChallenge
open TryWith.TryWith
open DiscriminatedUnions
open BrowserDialogWindow

[<EntryPoint; STAThread>]
let main argv =

    //*****************************WebScraping2**********************************************
    normalScraping()

    //*****************************WebScraping3**********************************************
    //webscrapingFromPage()

    //*****************************CodeChallenge*********************************************
    codeChallenge()

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