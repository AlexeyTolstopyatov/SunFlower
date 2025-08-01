# Portable Executable - supported headers and data structures

<img src="assets/sunflower.svg" height="128" width="128" align="right"/>

Modern Microsoft Windows NT applications Format. Information about this
segmentation format you can find everywhere, *so more features for analysis
will be added over time*. 

I'm thinking this not so intresting like digging down
in total IBM/Microsoft legacy.  

> [!WARNING]
> I'm not sure about completely support of
> Big Endian linked files. 
> I will definitely deal with this, but only when
> I have fully completed the support of all mandatory PE structures.

### Structures PE32/+

| Structures               | Status |
|--------------------------|--------|
| `IMAGE_DOS_HEADER`       | [x]    |
| `IMAGE_FILE_HEADER`      | [x]    |
| `IMAGE_OPTIONAL_HEADER`s | [x]    |
| `IMAGE_OS2_HEADER`       | [x]    |
| Data Directories         | [x]    |
| Section Headers          | [x]    |
| Static Imports table     | [x]    |
| Imporing Addresses Table | []     |
| Bound Imports Table      | []     |
| Delay Imports Table      | []     |
| Exports Table            | [x]    |
| Certifications Table     | []     |
| Base relocations table   | []     |
| Structured Exceptions    | []     |
| Global Pointers Table    | []     |
| Rich header (MSVC info)  | []     |


### CLR Metadata 

Common Language runtime information exists by
the Virtual Address of `COM Descriptor` data directory.

You can find it in one of PE sections, 
comparing VA of #15 (COM descriptor) directory 
with all sections.

If the required Virtual address
is suddenly located inside one of the sections, it is sufficient
to translate it to an raw offset from the beginning of the file.

File Offset points you to the Start of `IMAGE_COR20_HEADER` 

| Name          | Status |
|---------------|--------|
| CLR Header    | [x]    |
| `#Strings`    | []     |
| `#Blob`       | []     |

### Visual Basic 5/6 data structures

Now we are talking strictly about Visual Basic 5.0 and 6.0 versions.

> [!INFO]
> Earlier versions (VB 4.0 and earlier) of the language and its SDK have completely different

**First thing**, you **must** to know is next:
"Visual Basic (Classic)" modules/objects
**never had** `#15 data directory` (COM Descriptor table).

They are notable for the fact that the imported modules always
contain MSVBVMxx.dll (insert 50 or 60 here) first of all.
(that is, literally 1 item in the import table).

ActiveX controls that are not written in C/++, but use a VB VM,
must export the following list of functions:

| Name                | Ordinal | Address  |
|---------------------|---------|----------|
| DllCanUnloadNow     | @1      | 2FF46858 |
| DllGetClassObject   | @2      | 2FF46858 |
| DllRegisterServer   | @3      | 2FF46858 |
| DllUnregisterServer | @4      | 2FF46858 |

**BUT** this things uses **only** Visual Basic 5.0/6.0 objects.
(little about VB 4.0 and 3.0 you can find [here](SUPPORT_NE.md)) 


> [!INFO]
> Tested Virtual Machine is Oracle VirtualBox machine with Microsoft Windows 2000 (NT 5.0) with installed SDK (Visual Basic 6.0 CCE,), WinDbg, Visual Embedded 6.0.

**Second thing** is a pointer to main `VB_HEADER` structure
follows by the `AddressOfEntryPoint`. Following by this pointer
you find next /translated to x86 Assembly/:

```asm
push *address*      ; Offset to the VB_HEADER
call ThunRtMain     ; Thunder Runtime procedure
```

**Third thing** is a validation of `VB_HEADER`
Signature field must be `VB5!` (not Null-Terminated String)
Interpreter must be `MSVBVM50` or `MSVBVM60`.

| Name                   | Status |
|------------------------|--------|
| Visual Basic 5 Header  | []     |
| COM registration data  | []     |
| COM registration info  | []     |
| COM data               | []     |
| Project Info           | []     |
| COM designer info      | []     |

