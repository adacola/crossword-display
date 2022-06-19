module Adacola.CrosswordDisplay.Crossword

open System
open FSharpPlus

let [<Literal>] private messageWaitingMs = 100

let executeTask (readWords: string -> Async<string list>) (startWait: TimeSpan) (wait: TimeSpan) (maybeRestWords: {| TotalCount: int; RestWords: string list|} option) keyword =
  let startWaitMs = startWait.TotalMilliseconds |> int
  let waitMs = wait.TotalMilliseconds |> int

  MailboxProcessor<Message>.Start (fun inbox ->
    let rec loopUntilQuit totalCount = async {
      match! inbox.Receive() with
      | Message.Quit replyChannel -> replyChannel.Reply {| Keyword = keyword; TotalCount = totalCount; RestWords = [] |}
      | _ -> return! loopUntilQuit totalCount
    }

    let rec loop isFirst totalCount restMs words = async {
      let! quited =
        inbox.TryScan((function
          | Message.Quit replyChannel -> async { replyChannel.Reply {| Keyword = keyword; TotalCount = totalCount; RestWords = words |} } |> Some
          | _ -> None
        ), 0)
      if quited.IsNone then
        match! inbox.TryReceive 0 with
        | Some(Message.Quit replyChannel) -> replyChannel.Reply {| Keyword = keyword; TotalCount = totalCount; RestWords = words |}
        | Some Message.Next -> return! loop false totalCount 0 words
        | None when restMs <= 0 ->
          match words with
          | word::restWords ->
            do! stdout.WriteLineAsync $"\x1b[1A%4d{totalCount - restWords.Length} / %4d{totalCount} : {word}" |> Async.AwaitTask
            let nextWaitMs = (if isFirst then startWaitMs else waitMs) + restMs
            return! loop false totalCount nextWaitMs restWords
          | [] ->
            do! stdout.WriteLineAsync $"\x1b[1A{keyword} : 完了                                       " |> Async.AwaitTask
            return! loopUntilQuit totalCount
        | None ->
          do! Async.Sleep messageWaitingMs
          return! loop false totalCount (restMs - messageWaitingMs) words
    }
    async {
      match maybeRestWords with
      | None ->
        let! words = readWords keyword
        let totalCount = words.Length
        do! stdout.WriteLineAsync $"{keyword} : %4d{totalCount} 単語\n" |> Async.AwaitTask
        return! loop true totalCount 0 words
      | Some restWords ->
        do! stdout.WriteLineAsync $"{keyword} : %4d{restWords.TotalCount} 単語 (残り {restWords.RestWords.Length})\n" |> Async.AwaitTask
        return! loop true restWords.TotalCount 0 restWords.RestWords
    })
