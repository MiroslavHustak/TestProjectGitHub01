namespace TryWith

open System
open System.IO

open Messages.Messages
open DiscriminatedUnions

module TryWith =

    let tryWith f1 f2 f3 x y = //P
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> 
               f3
               Failure (ex.Message, y)      

    let deconstructorError fn1 fn2 =  
        fn1       
        do Console.ReadKey() |> ignore 
        fn2
        do System.Environment.Exit(1) 

    let deconstructor (printError: string -> unit) =  //I        
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             deconstructorError <| printError (string ex) <| ()                             
                             y   
                              
    let inline optionToSRTP (printError: Lazy<unit>) (srtp: ^a) value = //I 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value  
            | None       -> 
                            printError.Force() 
                            srtp

