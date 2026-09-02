namespace SunFlower.Abstractions
//
// CoffeeLake (C) - 2026-*
// SunFlower 5 changes the modeling process. The plugin (Flower-derived class)
// is now oriented at the subject scope. All public fields which are marked as [Seed] attribute
// will be placed into Markdown file.
//
// Simple listing:
// 
// [Flower(SeedTarget.Data)]
// [FlowerContract(5, 0, 0)]
// public class PortableExecutableFlower : /* parents */ {
//     [Seed(
//         name: "Microsoft COFF file sections",
//         description: @"MSCOFF file sections are presents the file scopes where code/data are store"
//     )]
//     public IMAGE_SECTION_HEADER[] Sections { get; private set; } // <-- handles by FlowerReflection::GetListTable
//
//     [Seed(
//         name: "PE header",
//         description: "Portable Executable header"
//     )] 
//     public IMAGE_FILE_HEADER FileHeader { get; private set; } // <-- handles by FlowerReflection::GetNameValueTable
//
//     public int SectionAlignment { get; private set; } // X-- ignored by kernel API. ([Seed] is missing)
//
//     // Factory method (uses by Kernel API)
//     public async Task CreateAsync(string path) {
//         throw new NotImplementedException();
//     }
// }
//
// This is an opposite for IFlowerSeed plugin model. And I see it more attractive and more usable
// because the SunFlower initialization/deserialization processes stay hided by user,
// user stays concentrated at main subject problem.
//
open System
open System.Runtime.InteropServices

/// <summary>
/// IFlower derivative class Field. If field marks as <c>[Seed]</c>,
/// the Markdown/Code renderer will show it as a markdown table or pseudocode listing 
/// </summary>
[<Class>]
[<Sealed>]
[<AttributeUsage(AttributeTargets.Property ||| AttributeTargets.Field)>]
type SeedAttribute(name: string, description: string, [<Optional>] skip: bool) =
    inherit Attribute()
    
    new() =
        SeedAttribute(String.Empty, String.Empty, false)
    
    member public _.Name = name
    member public _.Description = description
    member public _.Skip = skip