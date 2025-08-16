## Versioning Plugins (must read and know)

<img src="/assets/sunflower.svg" align="right" width="128" height="128">

In Sunflower package exists two critical important
parts. They are `SunFlower.Abstractions.IFlowerSeed` interface 
and `SunFlower.Abstractions.Attributes.FlowerSeedContract` atteibute entity

Sunflower loader kernel checks 
`FlowerSeedContract` versions and unload plugins which not satisfied
current version of Abstractions. 

### Major version
Means big (mostly) changes in DLL or
kernel ABI. Manager of plugins will safely unload
plugins with different Major Version. 

### Minor version
Means big compatible changes mostly in private
API or private software regions which improve
functional and backwards compatibility. 

### Build version (or Fixup increment)
Not means actually build ordinal, but means
important fixes in private software regions. 

Just fixes. 

### Loader Kernel
Doesn't unload plugins with different 
minor and build version. But plugins which differs
with Minor Version in contract will
call message with warning from loader kernel. 

> [!TIP]
> File version or Nuget package versions **of plugin library** doesn't check! This values represents fully for you. 

File version of fundamentals library
(means `SunFlower.Abstractions.dll`) very important
value, but file versions of plugins
are user/dev values which kernel doesnt check.

It would be nice, if Contract and FileVersion
values will be the same and I'll follow this rule. 

### In the End
This note not just for others enthusiasts but also for me too!
I'm just student developer too, but a software
requires a clean instructions to use. And clean
architecure too. 