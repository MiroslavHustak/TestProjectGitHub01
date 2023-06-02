module CodeChallenge

open System

open PatternBuilders.PattternBuilders

//code challenge assignment
let codeChallenge() = 

    let str = "(1 + (2*3) + ((8 - (3 - 2))/4)) + 1" //ukol je stanovit hloubku zavorek s co nejmensim mnozstvim kodu

    printfn "Depth = %A" str

    //vitezne reseni (ne moje)
    let depthBrackets0 =  
        Seq.map (function '(' -> 1 | ')' -> -1 | _ -> 0) 
        >> Seq.scan (+) 0
        >> Seq.max
    printfn "Depth0 = %i" (str |> depthBrackets0)

    //rozepsane vitezne reseni
    let strSeq = str |> Seq.filter (fun x -> x = '(' || x = ')') //string = sequence of characters

    let result1 = 
        strSeq |> Seq.map (
                              function 
                              | '(' ->  1
                              | ')' -> -1  
                              | _   ->  0
                          ) 
    let depthBrackets1 = result1 |> Seq.scan (+) 0 |> Seq.max
    printfn "Depth1 = %i" depthBrackets1

    //rozepsane vitezne reseni
    let result3 = 
        strSeq 
        |> Seq.map (fun item -> 
                              match item with 
                              | '(' ->  1
                              | ')' -> -1  
                              | _   ->  0
                   ) 

    let depthBrackets3 = result3 |> Seq.scan (+) 0 |> Seq.max
    printfn "Depth3 = %i" depthBrackets1

    //muj pokus o reseni, sice funguje, ale toho kodu.......
    let result77 =         
        strSeq
        |> Seq.mapi (fun i item ->  
                                 MyBuilderCC
                                    {
                                        let!_ = i < (strSeq |> Seq.length) - 1 
                                        let!_ = (item = '(' && strSeq |> Seq.item (i + 1) <> ')') || (item = ')' && strSeq |> Seq.item (i + 1) = '(')
                                        return item
                                    }                                 
                    ) 
     
    let depthBrackets = result77 |> Seq.filter (fun x -> (<>) x '0')

    printfn "Depth = %i" (depthBrackets |> Seq.length)

    Console.ReadLine() |> ignore

