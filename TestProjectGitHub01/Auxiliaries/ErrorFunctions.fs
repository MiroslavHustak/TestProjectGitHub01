﻿namespace ErrorFunctions

open System
open System.IO

module ErrorFunctions =    
    
    let deconstructorError fn1 fn2 =        

        fn1       
        do Console.ReadKey() |> ignore 
        fn2
        do System.Environment.Exit(1) 
       
                              
    let optionToSRTPError err = printfn"%s" err
        
                            


