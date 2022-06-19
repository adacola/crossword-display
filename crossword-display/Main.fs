module Adacola.CrosswordDisplay.Main

open System
open System.Text.RegularExpressions
open FSharpPlus
open Argu

[<RequireQualifiedAccess>]
type Arguments =
  | Wait of ms: int
  | Start_Wait of ms: int
  | No_History
with
  interface IArgParserTemplate with
    member x.Usage =
      match x with
      | Wait _ -> "単語ごとの待機時間をミリ秒単位で指定します。省略時は4000"
      | Start_Wait _ -> "最初の単語を表示するまでの待機時間をミリ秒で指定します。省略時は --wait で指定したミリ秒と同じ"
      | No_History -> "前回入力した単語をどこまで使用したかを記憶しません"

[<EntryPoint>]
let main argv =
  let argParser = ArgumentParser.Create<Arguments>(programName = "crossword-display", errorHandler = ProcessExiter())
  let args = argParser.ParseCommandLine argv
  let waitMs = args.GetResult(Arguments.Wait, 4000)
  let startWaitMs = args.GetResult(Arguments.Start_Wait, waitMs)
  let usesHistory = args.Contains Arguments.No_History |> not
  let wait = TimeSpan.FromMilliseconds waitMs
  let startWait = TimeSpan.FromMilliseconds startWaitMs

  let rec loop (history: Map<string, {| TotalCount: int; RestWords: string list|}>) (maybeProcessor: MailboxProcessor<Message> option) =
    match Console.ReadLine().Trim().ToLower() with
    | "q" | "quit" ->
      maybeProcessor |> map (fun processor -> processor.PostAndReply Message.Quit) |> ignore
      0
    | "n" | "next" ->
      maybeProcessor |> iter (fun processor -> processor.Post Message.Next)
      loop history maybeProcessor
    | "r" | "reset" -> loop Map.empty maybeProcessor
    | keyword ->
      let keyword = keyword |> String.replace "？" "?" |> String.replace "." "?"
      let maybeResult = maybeProcessor |> map (fun processor -> processor.PostAndReply Message.Quit)
      let newHistory =
        match usesHistory, maybeResult with
        | true, Some result -> history |> Map.add result.Keyword {| TotalCount = result.TotalCount; RestWords = result.RestWords |}
        | _ -> history
      // 同じ単語を連続で指定した場合に備えて、新しいhistoryをすぐには使わない
      // 新しいhistoryを使ってしまうと、あえて同じ単語を指定したのに続きからになってしまい意味がないので
      let restWords = history |> Map.tryFind keyword
      if Regex.IsMatch(keyword, @"\A[a-z?]+\z") then
        EnglishCrossword.executeTask startWait wait restWords keyword |> Some |> loop newHistory
      else
        JapaneseCrossword.executeTask startWait wait restWords keyword |> Some |> loop newHistory

  loop Map.empty None
