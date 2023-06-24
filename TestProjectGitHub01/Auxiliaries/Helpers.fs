namespace Helpers

open System
open System.IO
open System.Diagnostics

open TryWith.TryWith
       
    module MyString = 
        
        [<CompiledName "GetString">]  //priklad pouziti: getString(8, "0")//tuple a compiled nazev velkym kvuli DLL pro C#
        let getString (numberOfStrings: int, stringToAdd: string): string =   //P
            let initialString = String.Empty                //initial value of the string
            let listRange = [ 1 .. numberOfStrings ]
            let rec loop list acc auxStringToAdd =
                match list with 
                | []        -> acc
                | _ :: tail -> 
                               let finalString = (+) acc auxStringToAdd
                               loop tail finalString auxStringToAdd //Tail-recursive function calls that have their parameters passed by the pipe operator are not optimized as loops #6984
            loop listRange initialString stringToAdd //Tail-recursive function calls ... viz vyse   
         
   module private TryParserInt =

        let tryParseWith (tryParseFunc: string -> bool * _) =
            tryParseFunc >> function
            | true, value -> Some value
            | false, _    -> None
        let parseInt = tryParseWith <| System.Int32.TryParse  
        let (|Int|_|) = parseInt  
        
   module private TryParserDate = //tohle je pro parsing textoveho retezce do DateTime, ne pro overovani new DateTime()

          let tryParseWith (tryParseFunc: string -> bool * _) =
              tryParseFunc >> function
              | true, value -> Some value
              | false, _    -> None
          let parseDate= tryParseWith <| System.DateTime.TryParse 
          let (|Date|_|) = parseDate     

   module Parsing =

        let f x = 
            let isANumber = x                                          
            isANumber        
        let rec parseMeInt (printError: string -> unit) =
            function            
            | TryParserInt.Int i -> f i 
            | notANumber         ->  
                                    printError notANumber 
                                    -1 
        let f_date x = 
            let isADate = x       
            isADate                    
        let rec parseMeDate (printError: string -> unit) =
            function            
            | TryParserDate.Date d -> f_date d 
            | notADate             -> 
                                      printError notADate
                                      DateTime.MinValue

                                       
   //**************************************************************************************************                                  
   //Toto neni pouzivany kod, ale jen pattern pro tvorbu TryParserInt, TryParserDate atd. Neautorsky kod.
   module private TryParser =

        let tryParseWith (tryParseFunc: string -> bool * _) = 
            tryParseFunc >> function
                            | true, value -> Some value
                            | false, _    -> None

        let parseDate   = tryParseWith <| System.DateTime.TryParse
        let parseInt    = tryParseWith <| System.Int32.TryParse
        let parseSingle = tryParseWith <| System.Single.TryParse
        let parseDouble = tryParseWith <| System.Double.TryParse
        // etc.

        // active patterns for try-parsing strings
        let (|Date|_|)   = parseDate
        let (|Int|_|)    = parseInt
        let (|Single|_|) = parseSingle
        let (|Double|_|) = parseDouble

    