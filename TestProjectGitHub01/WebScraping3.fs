module WebScraping3

//#r "nuget:canopy"
open canopy
open canopy.classic
open canopy.configuration

//CANOPY

//FOR TESTING PURPOSES ONLY

//TODO try-with blocks 
//TODO validations  
//TODO Option.ofObj

open System

// Download chromedriver.exe from
// https://chromedriver.chromium.org/downloads

let webscrapingFromPage() = 

    canopy.configuration.chromeDir <- "c:/temp/driver"
    
    start chrome    

    url "https://www.kodis.cz/lines/region?tab=232-293"

    let linksShown () = (elements ".Card_wrapper__ZQ5Fp").Length >= 11 //podminka pro waitFor - cekat na to, az se to zobrazi
       
    compareTimeout <- 10.0 //nutne u pomalejsich pripojeni

    //waitForElement ".Card_wrapper__ZQ5Fp"

    waitFor linksShown

    let result = 
        elements "a" 
        |> List.map (fun item -> 
                                let href = string <| item.GetAttribute("href")
                                match href.EndsWith("pdf") with
                                | true  -> 
                                            printfn "%s" href
                                            href
                                | false -> String.Empty
                    )

    let filteredResult = result |> Set.ofList //vyhodime pripadne totozne polozky
    
    click (elementWithText "a" "Další")
    //click "Další" //aji toto funguje
      

