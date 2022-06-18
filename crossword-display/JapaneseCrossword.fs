module Adacola.CrosswordDisplay.JapaneseCrossword

open System
open System.Text
open System.Web
open System.Net.Http
open FSharpPlus
open FSharp.Data
open Umayadia.Kana

let [<Literal>] private messageWaitingMs = 100

let private encoding =
  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
  Encoding.GetEncoding("euc-jp")

let executeTask (wait: TimeSpan) keyword =
  let readWords = async {
    let keyword = keyword |> KanaConverter.ToKatakana |> String.replace "?" "_" |> String.replace "？" "_"
    let encodedKeyword = HttpUtility.UrlEncode(keyword, encoding)
    let requestBody = $"keyword={encodedKeyword}&rev="
    use requestContent = new StringContent(requestBody, encoding, "application/x-www-form-urlencoded")
    let! response = HttpClient.client.PostAsync("http://xword.s44.xrea.com/?m=xwordsearch", requestContent) |> Async.AwaitTask
    let! responseBody = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
    let document = HtmlDocument.Parse(encoding.GetString(responseBody))
    match HtmlNode.cssSelect (document.Body()) "table" with
    | _::table::_ ->
      return HtmlNode.cssSelect table "tr" |> List.tail |> map (HtmlNode.elementsNamed ["td"] >> head >> HtmlNode.innerText >> String.trim [' '] >> KanaConverter.ToHiragana)
    | _ -> return []
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
