module Adacola.CrosswordDisplay.JapaneseCrossword

open System.Text
open System.Text.RegularExpressions
open System.Web
open System.Net.Http
open FSharpPlus
open FSharp.Data
open Umayadia.Kana

let private smallProbabilityMatcher = Regex @"[つやゆよ]"
let private smallMatcher = Regex @"[っゃゅょぁぃぅぇぉ]"

let private encoding =
  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
  Encoding.GetEncoding("euc-jp")

let private readWords keyword = async {
  let keyword = keyword |> KanaConverter.ToKatakana |> String.replace "?" "_"
  let encodedKeyword = HttpUtility.UrlEncode(keyword, encoding)
  let requestBody = $"keyword={encodedKeyword}&rev="
  use requestContent = new StringContent(requestBody, encoding, "application/x-www-form-urlencoded")
  let! response = HttpClient.client.PostAsync("http://xword.s44.xrea.com/?m=xwordsearch", requestContent) |> Async.AwaitTask
  let! responseBody = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
  let document = HtmlDocument.Parse(encoding.GetString(responseBody))
  match HtmlNode.cssSelect (document.Body()) "table" with
  | _::table::_ ->
    // つやゆよが含まれている場合、真の単語は小さいっゃゅょの可能性があるので後回しにする
    let otherWords, goodWords =
      HtmlNode.cssSelect table "tr" |> List.tail |> map (HtmlNode.elementsNamed ["td"] >> head >> HtmlNode.innerText >> String.trim [' '] >> KanaConverter.ToHiragana)
      |> List.partition (fun x -> smallProbabilityMatcher.IsMatch x && (smallMatcher.IsMatch x |> not))
    return goodWords @ otherWords
  | _ -> return []
}

let executeTask startWait wait maybeRestWords keyword = Crossword.executeTask readWords startWait wait maybeRestWords keyword
