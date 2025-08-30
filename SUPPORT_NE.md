# New Executable - supported headers and data structures

<img src="assets/sunflower.svg" height="128" width="128" align="right"/>

Project repo contains information in PDF files (by Microsoft)
about NE segmented headers. Following this document is
possible to take little bit info about `Win16` modules (files). 

| Structures               | Status |
|--------------------------|--------|
| `IMAGE_DOS_HEADER`       | [x]    |
| `IMAGE_OS2_HEADER`       | [x]    |
| Table of Segments        | [x]    |
| Table of entries         | [x]    |
| Table of importing names | [x]    |
| Resident Names table     | [x]    |
| Non-resident Names table | [x]    |
| Resources table          | []     |
| Modules reference table  | [x]    |
| Per-segment table        | [x]    |

> [!WARNING]
> EntryTable fills incorrect if LINK.EXE version earlier than 5.0

### Visual Basic 4 (next plugin)

The only source with little more information
about it is Semi VB Decompiler (by VBGamer 45).

Semi VB decompiler contains data structures
and determination logic but those details
disabled in compiled version.

I'm looking for this information and trying to
find those data structures for beeing sure, 
that is really VB 4.0 application. 


| Name                   | Status |
|------------------------|--------|
| `VB4_OLD_HEADER`       | []     |

Old VB4 header locates in NE segmented image. `VB4_HEADER` locates in PE file image. 

### Visual Basic 3 (next plugin)

It is very difficult to make assumptions about the embedded VB 3.0 structures
in the NE segmented file, since
there are practically no materials on this topic left, and the only possible analysis tools that could be downloaded were obfuscated.

Example of VB 3.0 application in JellyBins repo

At least the number of fields, their order, and their capacity (cell data type) are known, but it will take a lot of experimentation to find out their names.

> [!TIP]
> Tested Virtual Machine is Oracle VirtualBox machine with Microsoft Windows 2000 (NT 5.0) with installed SDK (Visual Basic 3.0, 4.0), WinDbg, Visual Embedded 6.0.
