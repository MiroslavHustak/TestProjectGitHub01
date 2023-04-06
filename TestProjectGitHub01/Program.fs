namespace TestProject

open System

open Csv
open System.Data

open WebScraping1
open WebScraping2
open WebScraping3
open WebScraping4
open CodeChallenge

module Program = 

    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode

    //*****************************WebScraping1******************************
    webscraping1() 

    //*****************************WebScraping2******************************
    //normalScraping()

    //*****************************WebScraping3****************************** 
    //webscrapingWithSplittingJson()

    //*****************************WebScraping4****************************** 
    //webscrapingFromPage()

    //*****************************CodeChallenge*****************************
    //codeChallenge()

    //*****************************Vyukovy kod (priklad s csv) ****************************** 
    
    let example() = 

        let readDataFromExcel = 
            readDataFromExcel() 
            |> function
                | Some value -> value
                | None       -> do System.Environment.Exit(1)  //simulace reseni situace (muze byt napr. nejaka default hodnota)                                 
                                new DataTable() //whatever  

        readDataFromExcel |> writeIntoCSV @"e:\E\Mirek po osme hodine a o vikendech\" @"LT-15381 az LT-17691 DGSada 03-04-2023" |> ignore

        Console.ReadLine() |> ignore

    //example()

    //****************************************************************************************

