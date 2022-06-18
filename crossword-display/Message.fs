namespace Adacola.CrosswordDisplay

[<RequireQualifiedAccess>]
type Message = Next | Quit of AsyncReplyChannel<unit>
