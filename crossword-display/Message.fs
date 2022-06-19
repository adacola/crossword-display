namespace Adacola.CrosswordDisplay

[<RequireQualifiedAccess>]
type Message = Next | Quit of AsyncReplyChannel<{| Keyword: string; TotalCount: int; RestWords: string list |}>
