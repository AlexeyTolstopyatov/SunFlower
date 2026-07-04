namespace Sunflower.Dasm.Intel.Core
//
// CoffeeLake (C) 2026
// @creator: atolstopyatov2017@vk.com
//
type Operation =
    { code: string
      register: int option
      mnemonic: string
      operands: string list
      modRM: bool
      prefix: bool }
