// CoffeeLake (C) 2026-*
// MiT
//
// This service uses SunFlower.Kernel.FlowerCompatibility
// module API 
module SunFlower.Services.StartupService

open System.Data
open SunFlower.Kernel
open SunFlower.Kernel.Services

let getPluginsTable(): CorList<FlowerVersionInfo> =
    FlowerCompatibility.getFlowerVersionInfoList()

