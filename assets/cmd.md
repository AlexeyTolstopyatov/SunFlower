# Sunflower Report
**Generated at**: 2025-09-06 19:55:40


Sunflower Win16-OS/2 NE IA-32




# Image
Project Name: `CMD`
Description: `cmd.EXE`

### Hardware/Software
 - Operating system: `OS/2`
 - CPU architecture: `I386`
 - LINK.EXE version: 5.10


## Loader requirements
 - Heap=`1FA0`
 - Stack=`1FA0`
 - Swap area=`0000`
 - DOS/2 `CS:IP=0:0000`
 - DOS/2 `SS:SP=0:00B8`
 - Win16-OS/2 `CS:IP=0004:005C` (hex)
 - Win16-OS/2 `SS:SP=0006:0000` (hex)

> [!TIP]
> Segmented EXE Header holds on relative EntryPoint address.
> EntryPoint stores in [#4](decimal) segment with 0x5C offset

## Entities summary
1. Number of Segments - `6`
2. Number of Entry Bundles - `2`
3. Number of Moveable Entries - `0`
4. Number of Automatic Data segments - `6`
5. Number of Resources - `0`
6. Number of `BYTE`s in NonResident names table - `11`
7. Number of Module References - `7`
## Program Flags
### How data is handled?

In 16-bit DOS/Windows terminology, `DGROUP` is a segment class that referring
to segments that are used for data.

Win16 used segmentation to permit a DLL or program to have multiple
instances along with an instance handle and manage multiple data
segments. In example: allowed one `NOTEPAD.EXE` code segment to execute
multiple instances of the notepad application.
 - `SINGLE_DATA` (shared among instances of the same program)

### How application runs?
 - `PROTECTED_MODE_ONLY`
### Extra details?

## Application Flags
This block (field) tells how windowing or not windowing wants to run

## OS/2 Flags
Sunflower plugin shows this section if `e_flagothers` not zero. But I also suppose if appflags has `OS2_FAMILY` or `e_os` equals 0x1, what means OS/2 - you can read this section.
 - `LONG_NAMES` (avoid FAT rule 8.3 convertion)




### Importing modules
All .DLL/.EXE module names which resolved successfully
 - `\{0}\1B`
 - `\04j\{n}\02\06\{0}?\1F?\1F\\\{0}\04\{0}\{0}\{0}\06\{0}\06\{0}\07\{0}\0B\{0}@\{0}p\{0}p\{0}w\{0}?\{0}`
 - `\01\{0}\{0}\{0}\{0}\{t}\{0}\{0}\{0}\01\01\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\01\{0}?[\{0}\{r}?[0\{0}?c\{0}\{r}?cc\{0}?A\{0}\{r}?A?\{0}?\1F\{0}\{r}?\1F?\{0}?\1C\{0}\{r}?\1C?\{0}?\11A\{r}04\03CMD\{0}\{0}\{0}\01\{0}%\{0}.\{0}7\{0};\{0}?\{0}H\{0}`
 - `\{0}X\01\{0}\{0}\{0}\{0}\{t}\{0}\{0}\{0}\01\01\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\01\{0}?[\{0}\{r}?[0\{0}?c\{0}\{r}?cc\{0}?A\{0}\{r}?A?\{0}?\1F\{0}\{r}?\1F?\{0}?\1C\{0}\{r}?\1C?\{0}?\11A\{r}04\03CMD\{0}\{0}\{0}\01\{0}%\{0}.\{0}7\{0};\{0}?\{0}H\{0}\{0}\06SESMGR\{r}DOSSMSETTITLE\0EDOSSMPMPRESENT\08DOSCA`
 - `LS\08KBDCALLS\03MSG\03NLS\08QUECALLS\08VIOCALLS\{0}\{0}\07cmd.EXE\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}`
 - ``
 - ``
 - `\{0}`
 - `@91`
 - `?[0\{0}?c\{0}\{r}?cc\{0}?`
 - `\{0}\{r}?A?\{0}?\1F\{0}\{r}?\1F?\{0}?\1C\{0}\{r}?\1C?\{0}?\11A\{r}04\03CMD\{0}\{0}\{0}\01\{0}%\{0}.\{0}7\{0};\{0}?\{0}H\{0}\{0}\06SESMGR\{r}DOSSMSE`
 - `TITLE\0EDOSSMPMPRESENT\08DOSCALLS\08KBDCALLS\03MSG\03NLS\08QUECALLS\08VIOCALLS\{0}\{0}\07cmd.EXE\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}`
 - ``
 - `?[0\{0}?c\{0}\{r}?cc\{0}?`
 - `\{0}\{r}?A?\{0}?\1F\{0}\{r}?\1F?\{0}?\1C\{0}\{r}?\1C?\{0}?\11A\{r}04\03CMD\{0}\{0}\{0}\01\{0}%\{0}.\{0}7\{0};\{0}?\{0}H\{0}\{0}\06SESMGR\{r}DOSSMSE`
 - `TITLE\0EDOSSMPMPRESENT\08DOSCALLS\08KBDCALLS\03MSG\03NLS\08QUECALLS\08VIOCALLS\{0}\{0}\07cmd.EXE\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}\{0}`
 - `?cc\{0}?A\{0}\{r}?A?\{0}?`
 - `\{0}\{r}?\1F?\{0}?\1C\{0}\{r}?\1C?\{0}?\11A\{r}04\03CMD\{0}\{0}\{0}\01\{0}%\{0}`
 - `\{0}7\{0};\{0}?\{0}H\{0}\{0}\06SESMGR\{r}DOSSMSETTITLE\0EDOSSMPMPRESENT`
 - `DOSCALLS`
 - `KBDCALLS`
 - `MSG`
 - `NLS`
 - `QUECALLS`
 - `VIOCALLS`


### Module References

The module-reference table follows the resident-name table. Each entry contains an offset for the module-name string within the imported names table; each entry is 2 bytes long.
| Reference#:4   | Offset:2   |
|----------------|------------|
| 1              | 0x0001     |
| 2              | 0x0025     |
| 3              | 0x002E     |
| 4              | 0x0037     |
| 5              | 0x003B     |
| 6              | 0x003F     |
| 7              | 0x0048     |





### Segments Table

The segment table contains an entry for each segment in the executable file.
 The number of segment table entries are defined in the segmented EXE header
. The first entry in the segment table is segment number 1. The following is the structure of a segment table entry. 
| Type:s   | #Segment:4   | Offset:2   | Length:2   | Flags:2   | Minimum Allocation:2   | Characteristics:s                 |
|----------|------------|----------|----------|---------|----------------------|-----------------------------------|
| .CODE    | 0x1          | 0x1        | 0x5BCA     | 0xD00     | 0x5BCA                 | WITHIN_RELOCS                     |
| .CODE    | 0x2          | 0x30       | 0x6388     | 0xD00     | 0x6388                 | WITHIN_RELOCS                     |
| .CODE    | 0x3          | 0x63       | 0x41A4     | 0xD00     | 0x41A4                 | WITHIN_RELOCS                     |
| .CODE    | 0x4          | 0x85       | 0x1FB9     | 0xD00     | 0x1FB9                 | WITHIN_RELOCS                     |
| .CODE    | 0x5          | 0x96       | 0x1CBF     | 0xD00     | 0x1CBF                 | WITHIN_RELOCS                     |
| .DATA    | 0x6          | 0xA5       | 0x1191     | 0xD41     | 0x3430                 | WITHIN_RELOCS HAS_MASK PRELOAD    |





### Resident And NonResident Names

The resident-name table follows the resource table, and contains this module's name string and resident exported procedure name strings. The first string in this table is this module's name. 

The nonresident-name table follows the entry table, and contains a module description and nonresident exported procedure name strings. The first string in this table is a module description. These name strings are case-sensitive and are not null-terminated.
| Count   | Name        | Ordinal   | Name Table       |
|---------|-----------|---------|------------------|
| 7       | `cmd.EXE`   | @0        | [Not resident]   |
| 3       | `CMD`       | @0        | [Resident]       |







