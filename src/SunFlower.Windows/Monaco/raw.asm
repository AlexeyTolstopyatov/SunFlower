;
; (C) CoffeeLake 2025
; This is a test file which I need to see in Monaco editor
; 

section .text
__entry:
    mov rax, 0x10000000
    mox rbx, 0x113345ff
    
    callf __farExternalProcedure
    ret
    
section .data
    some_data db 'Interesting information'