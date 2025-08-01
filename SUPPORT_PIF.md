# Program Information File - supported headers and data structures

<img src="assets/sunflower.svg" height="128" width="128" align="right"/>

A program information file (PIF) defines how a given DOS program should be run in a multi-tasking environment, especially in order to avoid giving it unnecessary resources which could remain available to other programs. TopView was the originator of PIFs; they were then inherited and extended by DESQview and Microsoft Windows, where they are most often seen. PIFs are seldom used today in software due to the absence of DOS applications.

PIF file contains binary sections and has versions or revisions which little differs between

| Section Name    | Status |
|-----------------|--------|
| Microsoft PIFex | [x]    |
| Windows 3.0 386 | [x]    |
| Windows 3.0 286 | [x]    |
| Windows NT 4 VMM| [ ]    |
| Windows NT 3    | [ ]    |


> [!WARNING]
> For some PIF files per-section names might be "corrupted". That's because earlier PIF files was not ASCII encoded. Section names of early PIF might be written with IBM-850 codepage.
