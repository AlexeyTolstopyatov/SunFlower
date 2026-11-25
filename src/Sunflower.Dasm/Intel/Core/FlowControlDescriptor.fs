namespace Sunflower.Dasm.Intel.Core

open System.Text.Json.Serialization

type InterruptArg =
    { [<JsonPropertyName("size")>] Size: int
      [<JsonPropertyName("byte")>] ArgByte: string
      [<JsonPropertyName("return_flow")>] ReturnFlow: bool
      [<JsonPropertyName("comment")>] Comment: string }

type FlowControlDescriptor =
    { [<JsonPropertyName("int")>] Code: string
      [<JsonPropertyName("args")>] Args: InterruptArg[] }