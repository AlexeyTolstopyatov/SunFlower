namespace SunFlower

// This is a once module what you can legal copy //


/// <summary>
/// Common Runtime type Redefinition for
/// standard .NET List generic (not F# List)
/// </summary>
type CorList<'Any> = System.Collections.Generic.List<'Any>

module Say =
    // CorDllMain already exists
    let hello name =
        
        printfn "Hello %s" name