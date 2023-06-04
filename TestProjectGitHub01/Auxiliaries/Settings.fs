module Settings

open System

//[<Struct>]  //vhodne pro 16 bytes => 4096 characters
type ODIS = 
    {        
        odisDir1: string
        odisDir2: string
        odisDir3: string
        odisDir4: string
        odisDir5: string
        odisDir6: string
    }
    static member Default = 
        {          
            odisDir1 = "JR_ODIS_aktualni_vcetne_vyluk"
            odisDir2 = "JR_ODIS_pouze_budouci_platnost"
            odisDir3 = "JR_ODIS_pouze_vyluky"
            odisDir4 = "JR_ODIS_kompletni_bez_vyluk" 
            odisDir5 = "JR_ODIS_pouze_linky_dopravce_DPO_(aktualni_vcetne_vyluk)" 
            odisDir6 = "JR_ODIS_pouze_linky_dopravce_MDPO_(aktualni)" 
        }
    
[<Struct>]  //vhodne pro 16 bytes => 4096 characters
type Common_Settings = 
    {        
        columnIndex: int[]
    }
    static member Default = 
        {
            columnIndex = [| 0..12 |]          
        }

[<Struct>]
type ReadingDataFromExcel_Settings = 
    {
        path1: string  
        indexOfXlsxSheet: int  
    }
    static member Default = 
        {
            path1 = $@"......"                   
            indexOfXlsxSheet = 0<1> //Sheet v poradi 0 jako prvni, 1 = druhy, atd.
        } 

[<Struct>]
type OpenIrfanView_Settings = 
    {        
       path: string      //viz modul DU     
       path2: string     //viz modul DU         
       numberOfScannedFileDigits: int
       prefix: string    
       stringZero: string  
       suffixAndExtLength: int  
    }
    static member Default = 
        {
            path =  $@"..."
            path2 = $@"..."
            numberOfScannedFileDigits = 5<1>
            prefix = "LT-"
            stringZero = "0"
            suffixAndExtLength = 10<1> //delka retezce _00001.jpg
        }  

    module MySettings = 
     
     let rc = Common_Settings.Default

     let rcR = {
                   ReadingDataFromExcel_Settings.Default with 
                       path1 = $@"e:\E\Mirek po osme hodine a o vikendech\LT-15381 az LT-17691 DGSada 27-04-2022.xlsx"  //pouze pro testovani u sebe na pocitaci                              
               }

     let rcO = { 
                   OpenIrfanView_Settings.Default with 
                       path =  $@"e:\E\Mirek po osme hodine a o vikendech\Kontroly skenu\rozhazovani\" //pouze pro testovani u sebe na pocitaci
                       path2 = $@"....." //pouze pro testovani u sebe na pocitaci
               }
                   
     

