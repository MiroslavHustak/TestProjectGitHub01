namespace Messages

open System

module Messages =

    let msg1 () = printfn "Zase se někdo vrtal v listu s odkazy a cestami. Je nutná jejich kontrola. Zmáčkni cokoliv pro ukončení programu."
    let msg2 () = printfn "Probíhá stahování a ukládání json souborů do příslušného adresáře."
    let msg3 () = printfn "Dokončeno stahování a ukládání json souborů do příslušného adresáře."
    let msg4 () = printfn "Probíhá filtrace odkazů na neplatné jízdní řády."
    let msg5 () = printfn "Error5"  //TODO dopln popis chyby
    let msg6 () = printfn "Error6c" //TODO dopln popis chyby
    let msg7 () = printfn "Error6b" //TODO dopln popis chyby 
    let msg8 () = printfn "Error6a" //TODO dopln popis chyby 
    let msg9 () = printfn "Error11" //TODO dopln popis chyby 
    let msg10 () = printfn "Dokončena filtrace odkazů na neplatné jízdní řády."
    let msg11 () = printfn "Provedeno mazání všech starých JŘ, pokud existovaly."
    let msg12 () = printfn "Provedeno mazání starých JŘ v dané variantě."
    let msg13 () = printfn "Pravděpodobně někdo daný adresář v průběhu práce tohoto programu smazal."    

    let msgParam1 = printfn "\n%s%s" "No jéje, někde nastala chyba. Zmáčkni cokoliv pro ukončení programu. Popis chyby: \n" 
    let msgParam2 = printfn "\n%s%s" "Jízdní řád s tímto odkazem se nepodařilo stáhnout: \n"  
    let msgParam3 = printfn "Probíhá stahování příslušných JŘ a jejich ukládání do [%s]."  
    let msgParam4 = printfn "Dokončeno stahování příslušných JŘ a jejich ukládání do [%s]."  
    let msgParam5 = printfn "Adresář [%s] neexistuje, příslušné JŘ do něj určené nemohly být staženy." 
    let msgParam6 = printfn "Chyba v řetězci [%s]." 
    let msgParam7 = printfn"%s" //to enable easy mocking for unit tests let msgParam7 = () in the optionToSRTP function
    let msgParam8 = printfn "%s\n" 
    let msgParam9 = printf "%s\r" 
