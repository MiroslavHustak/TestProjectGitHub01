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

    let deconstructor =  //I        
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             deconstructorError <| msgParam1 ex <| ()
                             y    
                              
    let inline optionToSRTP err (srtp: ^a) value = //I
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value  
            | None       -> 
                            msgParam7 err
                            srtp

