# Linear Executable - supported headers and data structures

<img src="assets/sunflower.svg" height="128" width="128" align="right"/>

Project repo contains information in PDF files (by IBM)
about LE/LX segmented headers. Following this document is
possible to take little bit info about OS/2 and eComStation modules (files). 

LE combined format has 16-bit and 32-bit code segments (objects for now)
and some details in format are not compatible with next OS/2 module format (LX format) 

| Structure                 | Status |
|---------------------------|--------|
| `LE_HEADER`               | [x]    |
| Importing modules table   | [x]    |
| Importing procedures      | [x]    |
| Object table              | [x]    |
| Object Page Table         | [x]    |
| Entries Table             | [x]    |
| Resident Names table      | [x]    |
| Non-resident Names table  | [x]    |
| Module Format Directives  | [x]    |
| Fixup Pages               | [x]    |
| Fixup Records             | [x]    |
| Preload pages             | []     |
| Iterated Data pages       | []     |
| Demand load pages         | []     |
| Debug information         | []     |
| Resource Table            | []     |

### VxD Model

Virtual device drivers contains resident part of flat header (LE header)
and fields which loader not lookup. 

So, Device Driver has own specific data
which system tries to find and resolve

| Structure                 | Status |
|---------------------------|--------|
| `VXD_HEADER`              | [x]    |
| Driver Resources          | [x]    |
| Driver Description Block  | []     |
| Win32 Resource scripts    | []     |

Driver resources structure has many names, in code of sunflower
it calls `VxdResource` like `VxdHeader` see it by `.../plugins/SunFlower.Le/Headers/Le` path

> ![TIP]
> The most popular and right way to define virtual device driver is 
> a non-zero `e32_winres_off` field. This is an absolute (raw file pointer)
> to `VXD_RESOURCE` structure. Data length of this struct `e32_winres_length`
> are optional value (i suggest), and not uses by loader for image definition.

Field `e32_winres_off` names fully like this "Executable Win32 Resources offset"
and this is right for now. This suggestion is right because raw file pointer `e32_winres_off`
points for `.res` file (or compiled resource-script).

```
            | DWORD e32_winres_off |-----+
            | WORD e32_winres_len  |     | This means
            | BYTE e32_ddk_major   |     | not offset from current
            | BYTE e32_ddk_minor   |     | data-structure
            +----------------------+     |
                                         | For real e32_winres_off
VXD_RESOURCES                            | is a raw pointer
+---------------------+ <----------------+
| BYTE r_type         |
| WORD r_id`          |
| BYTE r_name         |
| WORD r_ordinal      |
| WORD r_flags        |
| WORD r_res_length   |
|+-------------------+| 
|| VS_VERSION_INFO   || <-- Win32 Resources starts here
|| and nested res-   ||     and it seems like nested types
|| ources blocks     ||     in this data-struct
|+-------------------+|
+---------------------+ <-- EOF

Usually after this  (when `VS_VERSION_INFO`) ends
stays EOF (or simply driver's image ends)
```

I also suggest, main data what system expects
from virtual device driver is a description block
or `DDB`. (may be it calls "Driver/Device Description Block")

```java
public class DescriptionBlock {
	public int DDB_Next;         /* VMM RESERVED FIELD */
    public short DDB_SDK_Version;     /* INIT <DDK_VERSION> RESERVED FIELD */
    public short DDB_Req_Device_Number;   /* INIT <UNDEFINED_DEVICE_ID> */
    public byte DDB_Dev_Major_Version;    /* INIT <0> Major device number */
    public byte DDB_Dev_Minor_Version;    /* INIT <0> Minor device number */
    public short DDB_Flags;           /* INIT <0> for init calls complete */
    public byte[] DDB_Name;          /* 8 bytes AINIT <"        "> Device name */
    public int DDB_Init_Order;       /* INIT <UNDEFINED_INIT_ORDER> */
    public int DDB_Control_Proc;     /* Offset of control procedure */
    public int DDB_V86_API_Proc;     /* INIT <0> Offset of API procedure */
    public int DDB_PM_API_Proc;      /* INIT <0> Offset of API procedure */
    public int DDB_V86_API_CSIP;     /* INIT <0> CS:IP of API entry point */
    public int DDB_PM_API_CSIP;      /* INIT <0> CS:IP of API entry point */
    public int DDB_Reference_Data;       /* Reference data from real mode */
    public int DDB_Service_Table_Ptr;    /* INIT <0> Pointer to service table */
    public int DDB_Service_Table_Size;   /* INIT <0> Number of services */
    public int DDB_Win32_Service_Table;  /* INIT <0> Pointer to Win32 services */
    public int DDB_Prev;         /* INIT <'Prev'> Ptr to prev 4.0 DDB */
    public int DDB_Reserved0;        /* INIT <0> Reserved */
    public int DDB_Reserved1;        /* INIT <'Rsv1'> Reserved */
    public int DDB_Reserved2;        /* INIT <'Rsv2'> Reserved */
    public int DDB_Reserved3;        /* INIT <'Rsv3'> Reserved */
}
```

And location of it holds in EntryPoints table.
One of not-resident names always names with `_DDB` postfix
and ordinal or this non-resident name is a position
of record (entry point) in entry points bundles (or just in EntryPoints table)

> ![WARNING] this information took from undocumented
> sources and partially by sunflower experiments

```
            | <resident_record>_DDB | @1 | 0xABCD |
            +-----------------------+----+--------+
                                      |
                                      |
                                      |
                                      |
Entry Bundle #1 (32-bit)              |
+----+-----------+---------+ <--------+
| @1 | 0xOFFSET  | 0xFLAGS |-----+
|....| ...       | ...     |     | Offset till this struct
                                 | defines by EntryPoints table
                                 | (see EntryTable in docs)
        +------------------+ <---+
        | DWORD DDB_next   |
        | WORD DDB_sdk_ver |
        | ...              |
        +------------------+
    Sometimes entry-points not just I286+
    instructions. They can be a unsafe structs
    or just pointers to something in segment.

Entry points same with exporting extries (like PE32/+)
```