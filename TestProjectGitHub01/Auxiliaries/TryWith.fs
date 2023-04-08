namespace TryWith

open System
open System.IO

open DiscriminatedUnions

module TryWith =

    let inline tryWith f1 f2 x y = //x se ne vzdy pouziva, ale z duvodu jednotnosti ponechano 
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

    let deconstructor2 =          
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> printfn"%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" ex
                             do Console.ReadKey() |> ignore 
                             do System.Environment.Exit(1) 
                             y          

        (*

    let deconstructor3 title message y =  
        function
        | Success x  -> x                                                   
        | Failure ex -> error6 <| title <| message ex
                        y

    let deconstructor4 y =  
        function
        | Success x  -> x                                                   
        | Failure ex -> error4 ex
                        y
    
    let optionToArraySort str1 str2 x = 
        function
        | Some value -> Array.sort (Array.ofSeq (value)) 
        | None       -> error3 str1 str2      

    let optionToGenerics str1 str2 x = 
        match x with 
        | Some value -> value 
        | None       -> error3 str1 str2 |> Array.head

    let optionToDirectoryInfo str (x: DirectoryInfo option) = 
        match x with 
        | Some value -> value
        | None       -> error4 str //ukonci program
                        new DirectoryInfo(String.Empty) //whatever of DirectoryInfo type

    let optionToGenerics2 str x = 
        function
        | Some value -> value
        | None       -> error4 str                                   
                        x //whatever of the particular type   
                                      
                    


*)


