namespace TryWith

open System
open System.IO

open DiscriminatedUnions

module TryWith =

    let tryWith f1 f2 x y = //x se ne vzdy pouziva, ale z duvodu jednotnosti ponechano 
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> Failure (ex.Message, y)  

    let deconstructor =          
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> printfn"%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" ex
                             do Console.ReadKey() |> ignore 
                             do System.Environment.Exit(1) 
                             y    
                              
    let optionToGenerics err (gen: 'a) value = 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value  
            | None       -> printfn"%s" err
                            gen

    
(*
*)


