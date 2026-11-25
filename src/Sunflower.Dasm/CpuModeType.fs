namespace Sunflower.Dasm

/// 
/// Possible Decoder API which Sunflower.Dasm assembly presents
/// 
type CpuModeType =
    | I186 = 0
    | I286 = 2
    | I286Protected = 3
    | I386 = 4
    | I386Protected = 5
    | I686 = 6
    | I686Protected = 7
    | Amd64 = 9
    | Aarch32 = 10
    | Aarch64 = 11