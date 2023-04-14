module Csv 

open System
open System.IO
open System.Data
open System.Linq
open ExcelDataReader
open System.Diagnostics

open Settings.MySettings

//TODO try with + validations

//jen pro vyukove ucely, v realne praxi lze pouzit csv type provider

let readDataFromExcel x =  
    
    let filepath = Path.GetFullPath(rcR.path1)   

    let readData() =        
       
        //TODO try with
        use stream = File.Open(filepath, FileMode.Open, FileAccess.Read) //vyjimecne unmanaged scope, aby bylo mozne pouzit use    

        let excelReaderStream =  
            
            let excelReaderXlsxF stream = ExcelReaderFactory.CreateOpenXmlReader(stream)  
            let excelReaderXlsF stream = ExcelReaderFactory.CreateBinaryReader(stream) //pouze pro rozsireni programu o vyber excel souboru         
  
            function 
            | ".xlsx" -> 
                         let myStream = stream |> excelReaderXlsxF  
                         myStream |> Option.ofObj
            | ".xls"  -> 
                         let myStream = stream |> excelReaderXlsF 
                         myStream |> Option.ofObj
            | _       -> 
                         let myStream = None  
                         myStream 
    
        //TODO try with
        let dtXlsxOption: DataTable option  = 

            let fileExt = Path.GetExtension(filepath)

            match excelReaderStream fileExt with 
            | Some excelReader ->    
                                use dtXlsx = excelReader.AsDataSet(                
                                                new ExcelDataSetConfiguration (ConfigureDataTable = 
                                                    fun (_:IExcelDataReader) -> ExcelDataTableConfiguration (UseHeaderRow = true)
                                                )
                                                ).Tables.[rcR.indexOfXlsxSheet] 
                                //excelReader.Close()   
                                //excelReader.Dispose()                                          
                                dtXlsx |> Option.ofObj
            | None             ->                                 
                                do System.Environment.Exit(1) //simulace reseni situace (muze byt napr. nejaka default hodnota)
                                None 
        dtXlsxOption  
  
    let adaptDtXlsx (dtXlsxOption: DataTable option) =    

        let adapt = 
            match dtXlsxOption with 
            | Some dtXlsx ->                                              
                            let numberOfColumns = rc.columnIndex.Length
                            seq { 0 .. numberOfColumns - 1 } 
                            |> Seq.iter(fun item -> dtXlsx.Columns.[rc.columnIndex.[item]].SetOrdinal(item))//Usporadame sloupce
                                                                                       
                            let sequenceGenerator _ = dtXlsx.Columns.Count  
                            let condition = (<) numberOfColumns 
                            let bodyOfWhileCycle _ = do dtXlsx.Columns.RemoveAt(numberOfColumns) //Vymazeme vsechny napravo, co tam nemaji co delat

                            Seq.initInfinite sequenceGenerator 
                            |> Seq.takeWhile condition 
                            |> Seq.iter bodyOfWhileCycle 

                            dtXlsx |> Option.ofObj
            | None       -> None                                 
        adapt 
        
    let readFromExcel =
        let readFromExcel = readData() |> adaptDtXlsx  
        try
            try              
                match File.Exists(filepath) with 
                | true  -> let readFromExcel = readData() |> adaptDtXlsx  
                           readFromExcel                        
                | false -> 
                           do System.Environment.Exit(1) // do error8()
                           None
                                              
            finally
            () //zatim nepotrebne
        with                                                                                               
        | :? System.IO.IOException as ex -> 
                                            do System.Environment.Exit(1) ///simulace reseni situace 
                                            None
        | _ as                        ex -> 
                                            do System.Environment.Exit(1) //simulace reseni situace 
                                            None       
    readFromExcel    

let writeIntoCSV (pathCSV: string) (nameOfCVSFile: string) (dt: DataTable) = //predpokladejme, ze pathCSV a nameOfCVSFile jsou osetrene 
         
    let csvPath = match string (pathCSV.Last()) with 
                  | "\\" -> pathCSV.Remove(pathCSV.Length - 1, 1)
                  | _    -> pathCSV
    let path = sprintf "%s\%s.csv" csvPath nameOfCVSFile

    //TODO try with
    use sw1 = new StreamWriter(Path.GetFullPath(path))
              |> Option.ofObj  
              |> function
                 | Some value -> value
                 | None       -> 
                                 do System.Environment.Exit(1)  //simulace reseni situace (muze byt napr. nejaka default hodnota)                                
                                 new StreamWriter(String.Empty) //whatever

    //TODO try with
    //headers   
    //let join1 = columnNames |> Seq.map (fun item -> sprintf "%s%s" item ";") |> Seq.fold (+) String.Empty
    let join1 =  
        let columnNames = dt.Columns.Cast<System.Data.DataColumn>() |> Seq.map (fun item -> item.Caption)
                         |> Option.ofObj    
                         |> function
                             | Some value -> value
                             | None       -> 
                                             do System.Environment.Exit(1)  //simulace reseni situace (muze byt napr. nejaka default hodnota)                                
                                             Seq.empty //whatever
        columnNames |> Seq.fold (fun acc item -> (+) acc (sprintf "%s%s" item ";")) String.Empty //TODO try with (Seq.fold)
   
    //TODO try with
    do sw1.WriteLine(join1.Remove(join1.Length - 1, 1))                 

    //*******************************  
    //rows
    [ 0 .. dt.Rows.Count - 1 ] 
    |> List.iteri (fun r _ -> 
                            let join =  
                                [ 0 .. dt.Columns.Count - 1 ]
                                |> List.mapi (fun c _ -> 
                                                        dt.Rows[r][c] |> Option.ofObj                                                             
                                                        |> function
                                                            | None when c = 0 -> string dt.Rows.[r].[c]                                                                                        
                                                            | _               -> (string dt.Rows.[r].[c]).Replace(';', ',')
                                                )  
                            let join = sprintf "%s%s" (String.concat <| ";" <| join) ";" 
                            do sw1.WriteLine(join.Remove(join.Length - 1, 1)) 
                            do sw1.Flush()  
                 )    
    0    