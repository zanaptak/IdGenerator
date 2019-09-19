module Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Zanaptak.IdGenerator
open System

let IdTests =
  let characterSet = "BCDFHJKMNPQRSTXZbcdfhjkmnpqrstxz"

  testList "IdTests" [
    testCase "default length" <| fun () ->
      Expect.equal ( IdGenerator().Next().Length ) 16 ""

    testCase "small length" <| fun () ->
      Expect.equal ( IdGenerator( IdSize.Small ).Next().Length ) 16 ""

    testCase "medium length" <| fun () ->
      Expect.equal ( IdGenerator( IdSize.Medium ).Next().Length ) 24 ""

    testCase "large length" <| fun () ->
      Expect.equal ( IdGenerator( IdSize.Large ).Next().Length ) 32 ""

    testCase "extra large length" <| fun () ->
      Expect.equal ( IdGenerator( IdSize.ExtraLarge ).Next().Length ) 40 ""

    testCase "small valid chars" <| fun () ->
      let theId = IdGenerator( IdSize.Small ).Next()
      Expect.isTrue ( theId |> Seq.forall ( fun c -> characterSet |> Seq.contains c ) )  ""

    testCase "medium valid chars" <| fun () ->
      let theId = IdGenerator( IdSize.Medium ).Next()
      Expect.isTrue ( theId |> Seq.forall ( fun c -> characterSet |> Seq.contains c ) )  ""

    testCase "large valid chars" <| fun () ->
      let theId = IdGenerator( IdSize.Large ).Next()
      Expect.isTrue ( theId |> Seq.forall ( fun c -> characterSet |> Seq.contains c ) )  ""

    testCase "extra large valid chars" <| fun () ->
      let theId = IdGenerator( IdSize.ExtraLarge ).Next()
      Expect.isTrue ( theId |> Seq.forall ( fun c -> characterSet |> Seq.contains c ) )  ""

    testCase "timestamp round trip" <| fun () ->
      let time = DateTimeOffset( 2020 , 10 , 11 , 12 , 13 , 14 , TimeSpan.Zero )
      let theId = IdGenerator().Next time
      let extractTime = IdGenerator.ExtractTimestamp theId
      Expect.equal extractTime.Year time.Year ""
      Expect.equal extractTime.Month time.Month ""
      Expect.equal extractTime.Day time.Day ""
      Expect.equal extractTime.Hour time.Hour ""
      Expect.equal extractTime.Minute time.Minute ""
      // Second not guaranteed exact due to precision
      Expect.isTrue ( extractTime.Second = time.Second || extractTime.Second + 1 = time.Second ) ""

  ]

let allTests = testList "All" [
  IdTests
]
