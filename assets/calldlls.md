# Sunflower Report
**Generated at**: 2025-09-08 09:02:56


Sunflower Windows PE32/+ IA-32(e)



# Image
This image is Windows NT linked module (Portable Executable format). Project name and description temporary unable to find. No one way to tell.
Instead New executable (`NE`) or Linear executables (`LX/LE`) formats, this one not holds standard fields about module.
## Hardware/Software
 - Target OS: `Windows NT`
 - Expected WinNT version: `4.0`
 - Minimum WinNT version: `4.0`
 - Linker version: `4.0`
### Windows Subsystem
Windows NT architecture includes subsystems like scopes at the userland (or ring-3)They are provide a support of I/O, net, GDI and other features, and PE module holdsa value, which subsystem's client will be called for running it.
 - `0x2`
 - `WIN32_CUI (Win32 Console Application). Tells loader, entry point is traditional `DWORD main(/*args*/)` function.`
### Characteristics
File characteristics always check by loader and helps it to run application correctly.
 - `image_file_executable` This indicates that the image file is valid and can be run. _If this flag is not set_, it indicates a linker error.
 - `image_file_linenums_stripped` COFF line numbers have been removed. This flag is deprecated and should be zero
 - `image_file_local_syms_stripped` COFF symbol table entries for local symbols have been removed. This flag is deprecated and should be zero.
 - `image_file_bytes_reverse_lo` **Little endian:** the least significant bit (LSB) precedes the most significant bit (MSB) in memory. _This flag is deprecated and should be zero_
 - `image_file_32bit_machine` Machine is based on a 32-bit-word architecture.
 - `image_file_bytes_reverse_hi` **Big endian:** the MSB precedes the LSB in memory. _This flag is deprecated and should be zero_.
### DLL Characteristics
Describe any PE linked module and any PE module holds `WORD DllCharacteristics` field. Not only `.DLL`s.
### Loader requirements
> [!TIP] 
> Address of an EntryPoint for this program is `0x00001054`


### DOS/2 Executable
| Segment         | Value   |
|-----------------|---------|
| e_sign          | 5A4D    |
| e_lastb         | 90      |
| e_fbl           | 3       |
| e_relc          | 0       |
| e_pars          | 4       |
| e_minep         | 0       |
| e_maxep         | FFFF    |
| ss              | 0       |
| sp              | B8      |
| e_check         | 0       |
| ip              | 0       |
| cs              | 0       |
| e_reltableoff   | 40      |
| e_overnum       | 0       |
| e_oemid         | 0       |
| e_oeminfo       | 0       |
| e_lfanew        | 80      |


### Portable Executable
| Segment                | Value      |
|------------------------|------------|
| Machine                | 14C        |
| NumberOfSections       | 5          |
| TimeDateStamp          | 6848403E   |
| PointerToSymbolTable   | 0          |
| NumberOfSymbols        | 0          |
| SizeOfOptionalHeader   | E0         |
| Characteristics        | 818E       |


### Optional Part (32-bit fields)
| Segment                       | Value    |
|-------------------------------|----------|
| Magic                         | 10B      |
| MajorLinkerVersion            | 4        |
| MinorLinkerVersion            | 0        |
| SizeOfCode                    | 6000     |
| SizeOfInitializedData         | 0        |
| SizeOfUninitializedData       | 1000     |
| AddressOfEntryPoint           | 1054     |
| BaseOfCode                    | 1000     |
| BaseOfData                    | 7000     |
| ImageBase                     | 400000   |
| SectionAlignment              | 1000     |
| FileAlignment                 | 200      |
| MajorOperatingSystemVersion   | 4        |
| MinorOperatingSystemVersion   | 0        |
| MajorSubsystemVersion         | 4        |
| MinorSubsystemVersion         | 0        |
| MajorImageVersion             | 1        |
| MinorImageVersion             | 0        |
| Win32VersionValue             | 0        |
| SizeOfImage                   | C000     |
| SizeOfHeaders                 | 400      |
| CheckSum                      | 8CE0     |
| Subsystem                     | 2        |
| DllCharacteristics            | 0        |
| SizeOfStackReserve            | 100000   |
| SizeOfStackCommit             | 1000     |
| SizeOfHeapReserve             | 100000   |
| SizeOfHeapCommit              | 1000     |
| LoaderFlags                   | 0        |
| NumberOfRvaAndSizes           | 10       |




### Sections Table

Each row of the section table is, in effect, a section header. This table immediately follows the optional header, if any. This positioning is required because the file header does not contain a direct pointer to the section table. Instead, the location of the section table is determined by calculating the location of the first byte after the headers. Make sure to use the size of the optional header as specified in the file header.

The number of entries in the section table is given by the `NumberOfSections` field in the file header. Entries in the section table are numbered starting from one. The code and data memory section entries are in the order chosen by the linker.
| Name:s   | VirtualAddress:4   | VirtualSize:4   | SizeOfRawData:4   | *RawData:4   | *Relocs:4   | *Line#:4   | #Relocs:2   | #LineNumbers:2   | Characteristics:4   |
|----------|------------------|---------------|-----------------|------------|-----------|----------|-----------|----------------|---------------------|
| .text    | 0x1000             | 0x6000          | 0x6000            | 0x400        | 0x0         | 0x0        | 0x0         | 0x0              | 0x60000020          |
| .bss     | 0x7000             | 0x1000          | 0x0               | 0x0          | 0x0         | 0x0        | 0x0         | 0x0              | 0xC0000080          |
| .rsrc    | 0x8000             | 0x2000          | 0x1200            | 0x6400       | 0x0         | 0x0        | 0x0         | 0x0              | 0x40000040          |
| .idata   | 0xA000             | 0x1000          | 0x200             | 0x7600       | 0x0         | 0x0        | 0x0         | 0x0              | 0x40000040          |
| .reloc   | 0xB000             | 0x1000          | 0x400             | 0x7800       | 0x0         | 0x0        | 0x0         | 0x0              | 0x42000040          |



### Statically Importing Procedures

Usually the import information begins with the import directory table, which describes the remainder of the import information. The import directory table contains address information that is used to resolve fixup references to the entry points within a DLL image. The import directory table consists of an array of import directory entries, one entry for each DLL to which the image refers. The last directory entry is empty (filled with null values), which indicates the end of the directory table.
| Module:s      | Procedure:s   | Ordinal:2   | Hint:2   | Address:8            |
|---------------|-------------|-----------|--------|----------------------|
| VB40032.DLL   | @617          | @617        | 0x0000   | 0x0000000080000269   |
| VB40032.DLL   | @645          | @645        | 0x0000   | 0x0000000080000285   |
| VB40032.DLL   | @660          | @660        | 0x0000   | 0x0000000080000294   |
| VB40032.DLL   | @100          | @100        | 0x0000   | 0x0000000080000064   |
| VB40032.DLL   | @187          | @187        | 0x0000   | 0x00000000800000BB   |
| VB40032.DLL   | @186          | @186        | 0x0000   | 0x00000000800000BA   |
| VB40032.DLL   | @199          | @199        | 0x0000   | 0x00000000800000C7   |
| VB40032.DLL   | @606          | @606        | 0x0000   | 0x000000008000025E   |
| VB40032.DLL   | @608          | @608        | 0x0000   | 0x0000000080000260   |
| VB40032.DLL   | @607          | @607        | 0x0000   | 0x000000008000025F   |





