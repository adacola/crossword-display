module Adacola.CrosswordDisplay.Main

open System
open System.Text.RegularExpressions
open FSharpPlus
open Argu

[<RequireQualifiedAccess>]
type Arguments =
  | Wait of ms: int
with
  interface IArgParserTemplate with
    member x.Usage =
      match x with
      | Wait _ -> "待機時間をミリ秒単位で指定します。省略時は4000"

[<EntryPoint>]
let main argv =
  let argParser = ArgumentParser.Create<Arguments>(programName = "crossword-display", errorHandler = ProcessExiter())
  let args = argParser.ParseCommandLine argv
  let waitMs = args.GetResult(Arguments.Wait, 4000)
  let wait = TimeSpan.FromMilliseconds waitMs

  let rec loop (maybeProcessor: MailboxProcessor<Message> option) =
    match Console.ReadLine().Trim().ToLower() with
    | "q" | "quit" ->
      maybeProcessor |> iter (fun processor -> processor.PostAndReply Message.Quit)
      0
    | "n" | "next" ->
      maybeProcessor |> iter (fun processor -> processor.Post Message.Next)
      loop maybeProcessor
    | keyword ->
      maybeProcessor |> iter (fun processor -> processor.PostAndReply Message.Quit)
      if Regex.IsMatch(keyword, @"\A[a-z?？.]+\z") then
        EnglishCrossword.executeTask wait keyword |> Some |> loop
      else
        JapaneseCrossword.executeTask wait keyword |> Some |> loop

  loop None
