module Adacola.CrosswordDisplay.EnglishCrossword

open System.Net
open System.Text.RegularExpressions
open FSharpPlus
open FSharp.Data

let private headers = ["User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36"]

let private cookieContainer = CookieContainer()

let private alphabetsMatcher = Regex @"\A[a-z]+\z"

let private readWords keyword = async {
  let! response = Http.AsyncRequestString("https://www.the-crossword-solver.com/search", body = FormValues ["q", keyword], cookieContainer = cookieContainer, headers = headers)
  let document = HtmlDocument.Parse(response)
  // 検索ワードがついている単語はよく使われる良質な単語として先に出てくるようにする
  let goodWordNodes, otherWordNodes =
    HtmlNode.cssSelect (document.Body()) "div#searchresults > span.searchresult" |> filter (fun x ->
      HtmlNode.cssSelect x $"span.matchtype{keyword.Length}" |> List.isEmpty |> not
      && HtmlNode.cssSelect x "span.matchtypeL" |> List.isEmpty |> not)
    |> List.partition (HtmlNode.elements >> exists (HtmlNode.hasClass "searchdefinition"))
  return
    goodWordNodes @ otherWordNodes
    |> List.map (HtmlNode.elementsNamed ["a"] >> head >> HtmlNode.innerText >> String.trim [' '] >> String.toLower)
    |> filter alphabetsMatcher.IsMatch
}

let executeTask startWait wait maybeRestWords keyword = Crossword.executeTask readWords startWait wait maybeRestWords keyword
