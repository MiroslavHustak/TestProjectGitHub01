﻿namespace TryWith

open System
open System.IO

open DiscriminatedUnions

module TryWith =

    let tryWith f1 f2 f3 x y = 
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> 
                f3
                Failure (ex.Message, y)  

    let deconstructor =          
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             printfn"%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" ex
                             do Console.ReadKey() |> ignore 
                             do System.Environment.Exit(1) 
                             y    
                              
    let inline optionToSRTP err (srtp: ^a) value = 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value  
            | None       -> 
                            printfn"%s" err
                            srtp

