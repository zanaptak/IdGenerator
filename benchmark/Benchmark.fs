open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

open Zanaptak.IdGenerator

type SmallBenchmark () =

  let idSmall = IdGenerator( IdSize.Small )
  let idSmallBuffer100 = IdGenerator( IdSize.Small , bufferCount = 100 )

  [< Benchmark >]
  member this.Small() = idSmall.Next()

  [< Benchmark >]
  member this.SmallBuffer100() = idSmallBuffer100.Next()

type LargeBenchmark () =

  let idLarge = IdGenerator( IdSize.Large )
  let idLargeBuffer100 = IdGenerator( IdSize.Large , bufferCount = 100 )

  [< Benchmark >]
  member this.Large() = idLarge.Next()

  [< Benchmark >]
  member this.LargeBuffer100() = idLargeBuffer100.Next()

[<EntryPoint>]
let main argv =
  BenchmarkSwitcher
    .FromTypes( [| typeof< SmallBenchmark > ; typeof< LargeBenchmark > |] )
    .RunAll()
    |> ignore
  0
