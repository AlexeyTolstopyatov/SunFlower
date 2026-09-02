namespace SunFlower.Abstractions

open System
open System.Threading.Tasks

type IFlower =
    interface
        /// <summary>
        /// Say my name
        /// </summary>
        abstract member Name : string
        /// <summary>
        /// Starts constructor method asynchronously
        /// </summary>
        abstract member CreateAsync : filePath:string -> Task
    end