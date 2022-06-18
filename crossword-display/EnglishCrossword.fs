module Adacola.CrosswordDisplay.EnglishCrossword

open System
open System.Net
open FSharpPlus
open FSharp.Data

let [<Literal>] private messageWaitingMs = 100

let private headers = ["User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36"]

let private cookieContainer = CookieContainer()

let executeTask (wait: TimeSpan) keyword =
  let readWords = async {
    let keyword = keyword |> String.replace "？" "?" |> String.replace "." "?"
    let! response = Http.AsyncRequestString("https://www.the-crossword-solver.com/search", body = FormValues ["q", keyword], cookieContainer = cookieContainer, headers = headers)
    let document = HtmlDocument.Parse(response)
    return
      HtmlNode.cssSelect (document.Body()) "div#searchresults > span.searchresult" |> filter (fun x ->
        x |> HtmlNode.elements |> exists (HtmlNode.hasClass "searchdefinition")
        && HtmlNode.cssSelect x $"span.matchtype{keyword.Length}" |> List.isEmpty |> not
        && HtmlNode.cssSelect x "span.matchtypeL" |> List.isEmpty |> not)
    |> List.map (HtmlNode.elementsNamed ["a"] >> head >> HtmlNode.innerText >> String.trim [' '] >> String.toLower)
    |> filter (fun x -> (x.Contains(" ") || x.Contains("-") || x.Contains(".") || ([0 .. 9] |> exists (string >> x.Contains))) |> not)
  }

  let waitMs = wait.TotalMilliseconds |> int
  MailboxProcessor<Message>.Start (fun inbox ->
    let rec loopUntilQuit() = async {
      match! inbox.Receive() with
      | Message.Quit replyChannel -> replyChannel.Reply()
      | _ -> return! loopUntilQuit()
    }

    let rec loop totalCount restMs words = async {
      let! quited =
        inbox.TryScan((function
          | Message.Quit replyChannel -> async { replyChannel.Reply() } |> Some
          | _ -> None
        ), 0)
      if quited.IsNone then
        match! inbox.TryReceive 0 with
        | Some(Message.Quit replyChannel) -> replyChannel.Reply()
        | Some Message.Next -> return! loop totalCount 0 words
        | None when restMs <= 0 ->
          match words with
          | word::restWords ->
            do! stdout.WriteLineAsync $"\x1b[1A%4d{totalCount - restWords.Length} / %4d{totalCount} : {word}" |> Async.AwaitTask
            return! loop totalCount (waitMs + restMs) restWords
          | [] ->
            do! stdout.WriteLineAsync $"\x1b[1A{keyword} : 完了                                       " |> Async.AwaitTask
            return! loopUntilQuit()
        | None ->
          do! Async.Sleep messageWaitingMs
          return! loop totalCount (restMs - messageWaitingMs) words
    }
    async {
      let! words = readWords
      let totalCount = words.Length
      do! stdout.WriteLineAsync $"{keyword} : %4d{totalCount} 単語\n" |> Async.AwaitTask
      return! loop totalCount 0 words
    })
