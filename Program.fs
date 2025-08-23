open System
open System.Net.Http
open System.Collections.Generic
open System.Text
open System.IO

// Register support for legacy encodings like CP1251
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

let httpClient = new HttpClient()

let checkEncoding (path: string) =
    let bytes = File.ReadAllBytes(path)

    // Check for UTF-8 BOM
    if bytes.Length >= 3 &&
       bytes.[0] = 0xEFuy &&
       bytes.[1] = 0xBBuy &&
       bytes.[2] = 0xBFuy then
        Encoding.UTF8
    else
        Encoding.GetEncoding(1251)

let correctSelection (selection: string) =
    let matchList = ["гортензия"; "тест"]
    
    match selection with
    | matchWord when List.exists (fun (word: string) -> 
        matchWord.StartsWith(word, StringComparison.OrdinalIgnoreCase)
        ) matchList ->
            let parts = selection.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
            if parts.Length > 0 then parts.[0] else selection
    | _ -> 
        selection


let postFormAsync (selection: string) (variety: string) =
    task {
        let url = "https://gossortrf.ru/registry/ajaxhandler.php"

        // Form data as key-value pairs
        let formData = Dictionary<string, string>()
        formData.Add("text", variety)
        formData.Add("field", "SORT_NAME")
        formData.Add("culture", selection)
        formData.Add("section", "31")

        let content = new FormUrlEncodedContent(formData)

        let! response = httpClient.PostAsync(url, content)
        response.EnsureSuccessStatusCode() |> ignore

        let! responseBody = response.Content.ReadAsStringAsync()
        let res = responseBody.Substring(13, 4)

        if res = "true" then return  "есть патент"
        else if res = "fals" then return  "свободная продажа"
        else return  "ошибка"
    }


[<EntryPoint>]
let main argv =
    let path = "input.csv"
    let encoding = checkEncoding path
    let lines = File.ReadAllLines (path, encoding)

    File.WriteAllText("result.txt", "")

    let work = task {
        for line in lines |> Array.skip 1 do
            let columns = line.Split([| ','; ';' |], StringSplitOptions.RemoveEmptyEntries)
            if columns.Length >= 2 then
                let selection = correctSelection (columns.[0].Trim())
                let variety = columns.[1].Trim()
                let! result = postFormAsync selection variety
                let resultStr = sprintf "Проверка: %s сорта %s. Результат: %s\n" selection variety result
                printf "%s" resultStr
                File.AppendAllText("result.txt", resultStr)
    }
    work.GetAwaiter().GetResult()
    0