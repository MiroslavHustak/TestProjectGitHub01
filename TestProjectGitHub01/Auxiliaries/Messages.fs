namespace Messages

open System

module Messages =

    let msg1 () = printfn "Zase se nekdo vrtal do listu s odkazy a cestami. Je nutna jejich kontrola. Zmackni cokoliv pro ukonceni programu."
    let msg2 () = printfn "Probiha stahovani a ukladani json souboru do prislusneho adresare."
    let msg3 () = printfn "Dokonceno stahovani a ukladani json souboru do prislusneho adresare."
    let msg4 () = printfn "Probiha filtrace odkazu na neplatne jizdni rady."
    let msg5 () = printfn "Error5"
    let msg6 () = printfn "Error6c"
    let msg7 () = printfn "Error6b"
    let msg8 () = printfn "Error6a"
    let msg9 () = printfn "Error11"
    let msg10 () = printfn "Dokoncena filtrace odkazu na neplatne jizdni rady."
    let msg11 () = printfn "Provedeno mazani vsech starych JR, pokud existovaly."
    let msg12 () = printfn "Provedeno mazani starych JR v dane variante."
    let msg13 () = printfn "Pravdepodobne nekdo dany adresar v prubehu prace tohoto programu smazal."
    

    let msgParam1 ex = printfn "\n%s%s" "No jeje, nekde nastala chyba. Zmackni cokoliv pro ukonceni programu. Popis chyby: \n" (string ex)
    let msgParam2 uri = printfn "\n%s%s" "Jizdni rad s timto odkazem se nepodarilo stahnout: \n" uri  
    let msgParam3 pathToDir = printfn "Probiha stahovani prislusnych JR a jejich ukladani do [%s]." pathToDir 
    let msgParam4 pathToDir = printfn "Dokonceno stahovani prislusnych JR a jejich ukladani do [%s]." pathToDir 
    let msgParam5 dir = printfn "Adresar [%s] neexistuje, prislusne JR do nej urceny nemohly byt stazeny." dir 
    let msgParam6 input = printfn "Chyba v retezci [%s]." input 
    let msgParam7 err = printfn"%s" err

   
  
                                   
    
    
