module WebScraping2

open FSharp.Data

open HtmlAgilityPack
open Fizzler.Systems.HtmlAgilityPack

//just testing web scraping

//TODO try-with blocks 
//TODO validations  
//TODO Option.ofObj


 //FOR TESTING PURPOSES ONLY       
let normalScraping() = 

    //********************FSharp.Data********************************
    let url1 = "https://scrapeme.live/shop/"
    let document1 = FSharp.Data.HtmlDocument.Load(url1) //nazvy knihovny se biju s HtmlAgilityPack

    let productHTMLElements1 = document1.CssSelect("li.product")
       
    productHTMLElements1
    |> List.iter (fun item ->
                            let querySelector (tag: string) = (item.Descendants (string tag)) |> Seq.head
                                                                         
                            let url = (querySelector "a").AttributeValue "href"    
                            let image = (querySelector "img").AttributeValue "src" 
                            let name = (querySelector "h2").InnerText()
                            let price = (querySelector "span").InnerText()
                        
                            printfn "productHTMLElements1 %s%s%s%s" url image name price    
                 )

    let url2 = "https://www.bbc.com/news" 
    let document2 = FSharp.Data.HtmlDocument.Load(url2)

    let links =
        document2.Descendants "a"
        |> Seq.choose (fun x ->
                              x.TryGetAttribute("href")
                              |> Option.map (fun a -> string <| x.InnerText(), string <| a.Value())                                            
                      )
        //|> Seq.truncate 10
        |> Seq.filter (fun (item1, _) -> item1.Contains "War in Ukraine")
        |> Seq.map (fun (_, item2)    -> sprintf"%s%s" "https://www.bbc.com/" item2 )
        |> Seq.toList

    printfn "War in Ukraine %A" links 


    //*******************HtmlAgilityPack (.NET) *********************************         
            
    // creating the HAP object 
    let web = new HtmlWeb()
      
    // visiting the target web page 
    let document = web.Load("https://www.kodis.cz/lines/region?tab=232-293")
      
    // getting the list of HTML product nodes 
    let productHTMLElements42 = document.DocumentNode.QuerySelectorAll("li")
  
    productHTMLElements42 
    |> Seq.iter (fun item ->   
                           let url = string <| HtmlEntity.DeEntitize(item.QuerySelector("a").Attributes["href"].Value)                   
                           printfn "productHTMLElements %s" url
                )
 

              

