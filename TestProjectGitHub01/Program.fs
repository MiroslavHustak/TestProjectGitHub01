namespace TestProject

open System

open Csv
open System.Data

open WebScraping1
open WebScraping2
open WebScraping3
open CodeChallenge

module Program = 

    do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)

    Console.BackgroundColor <- ConsoleColor.Blue 
    Console.ForegroundColor <- ConsoleColor.White 
    Console.InputEncoding   <- System.Text.Encoding.Unicode
    Console.OutputEncoding  <- System.Text.Encoding.Unicode

    //let str = @"https://kodis-files.s3.eu-central-1.amazonaws.com/801_2023_04_03_2023_06_11_v_582dde9c0b.pdf"

    
    //*****************************WebScraping1******************************
    do webscraping1() ()
    do Console.ReadKey() |> ignore
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