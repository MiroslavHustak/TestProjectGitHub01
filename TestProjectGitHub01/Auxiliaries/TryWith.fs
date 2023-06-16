namespace TryWith

open System
open System.IO

open Messages.Messages
open DiscriminatedUnions
open ErrorFunctions.ErrorFunctions

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

    let deconstructor (printError: string -> unit) =  //I        
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             deconstructorError <| printError (string ex) <| ()                             
                             y   
                              
    let inline optionToSRTP (printError: Lazy<unit>) (srtp: ^a) value = //I (ale bez printError by to bylo P)
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value  
            | None       -> 
                            printError.Force() 
                            srtp

