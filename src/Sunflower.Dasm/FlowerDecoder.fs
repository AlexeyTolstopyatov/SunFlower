//
// CoffeeLake (C) 2026-*
// This module represents special "imaginary" CPU with low-level declarations  
// The syntax rules same with the IA-32 but instruction mnemonics includes
// specific metadata between "<" and ">" braces.
//      00/_    add<Eb, Eb> AL, AL          ; Operation code saves and the microfunction arguments
//      00/_    add<Eb, Ib> AL, 0x10        ; explicitly declares too. 
//      00/_    jmp<Ib> 0x0000              ;
//      00/0    ES:mov<S, Mp, Eb> [BX], AL  ; If instruction has register/memory byte (R/M)
//      00/0    mov<Ev, Iv> AX, 0x1234      ; /N construction holds it. For else, spacing '_' tells  
//      00/0    push<Ev> AX                 ; that byte is missing. (not declared in the IA)
//      00/0    xor<Eb, Eb> AL, AL
//      ??/?    what$N<Mp, Ap> [...] ...    ; Unknown instruction. Known capacity. $N tells capacity
//      XX/Y    what$N<Ev, Iv> [...] ...    ; Look at the opcode. This is undeclared instruction. (e.g. 0x66 in I8086)
//      00/0    inc<Eb> AX
//      ??/?    wha...                      ; Unexpected end of instruction. Opcode saved.
//
// This syntax describes operation codes and Intel decoder for others and
//
// This module uses external "./fiamap.json" for current x86 toolset and has CUSTOM decompiler attribute:
//      .model flower                       ; instead of .model 586 in example 
// @creator: atolstopyatov@vk.com
//
module Sunflower.Dasm.FlowerDecoder

