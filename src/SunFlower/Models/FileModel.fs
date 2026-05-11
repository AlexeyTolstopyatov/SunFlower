namespace SunFlower.Models

[<Class>]
type FileModel(name, path) =
    member _.Name: string = name
    member _.Path: string = path
