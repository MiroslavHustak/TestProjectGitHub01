﻿namespace PatternBuilders

module PattternBuilders =
    (*
    let private (>>=) condition nextFunc = 
        match fst condition with
        | false -> ()
        | true  -> nextFunc() 

    type private MyBuilder = MyBuilder with            
        member _.Bind(condition, nextFunc) = (>>=) <| condition <| nextFunc 
        member _.Return x = x
   *)
    
    let private (>>=) condition nextFunc =
        match fst condition with
        | false -> snd condition
        | true  -> nextFunc()  
    
    [<Struct>]
    type MyBuilder = MyBuilder with    
        member _.Bind(condition, nextFunc) = (>>=) <| condition <| nextFunc
        member _.Return x = x