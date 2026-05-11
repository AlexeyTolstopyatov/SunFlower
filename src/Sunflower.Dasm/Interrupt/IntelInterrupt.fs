namespace Sunflower.Dasm.Interrupt

open System.Text.Json.Serialization

type IntelInterruptArg =
    { [<JsonPropertyName("byte")>] ArgByte: string
      [<JsonPropertyName("return_flow")>] ReturnFlow: bool
      [<JsonPropertyName("comment")>] Comment: string }

type IntelInterrupt =
    { [<JsonPropertyName("int")>] Code: string
      [<JsonPropertyName("args")>] Args: IntelInterruptArg array }