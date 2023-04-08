namespace Helpers

open System
open System.Diagnostics
       
    module MyString = 
        //priklad pouziti: getString(8, "0")//tuple a compiled nazev velkym kvuli DLL pro C#
        [<CompiledName "GetString">]
        let getString (numberOfStrings: int, stringToAdd: string): string =   
            let initialString = String.Empty                //initial value of the string
            let listRange = [ 1 .. numberOfStrings ]
            let rec loop list acc auxStringToAdd =
                match list with 
                | []        -> acc
                | _ :: tail -> let finalString = (+) acc auxStringToAdd
                               loop tail finalString auxStringToAdd //Tail-recursive function calls that have their parameters passed by the pipe operator are not optimized as loops #6984
            loop listRange initialString stringToAdd //Tail-recursive function calls that have their parameters passed by the pipe operator are not optimized as loops #6984   
         
   module private TryParserInt =
        let tryParseWith (tryParseFunc: string -> bool * _) =
            tryParseFunc >> function
            | true, value -> Some value
            | false, _    -> None
        let parseInt = tryParseWith <| System.Int32.TryParse  
        let (|Int|_|) = parseInt        

   module Parsing =
        let f x = let isANumber = x                                          
                  isANumber        
        let rec parseMe =
            function            
            | TryParserInt.Int i -> f i 
            | notANumber         ->  
                                    //printfn "Parsovani neprobehlo korektne u teto hodnoty: %s" notANumber 
                                    -1 
   
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

    