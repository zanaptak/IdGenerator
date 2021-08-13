module Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Zanaptak.IdGenerator
open System

let characterSet = "BCDFHJKMNPQRSTXZbcdfhjkmnpqrstxz"
let epochTicks = DateTimeOffset( 2019 , 9 , 19 , 0 , 0 , 0 , 0 , TimeSpan.Zero ).UtcTicks

let testPrecisionWithMilliIntervals precision intervalMilliseconds itemCount =

    let gen = IdGenerator( timePrecision = precision )
    let startTicks = 637045381230040000L  // 2019-09-20 01:02:03.004

    let ticksPerMillisecond = 10000L

    let intervalTicks =
        Seq.init itemCount ( fun i -> int64 i * intervalMilliseconds * ticksPerMillisecond + startTicks )

    let power =
        match precision with
        | IdTimePrecision.Second -> 23.
        | IdTimePrecision.Millisecond -> 15.
        | IdTimePrecision.Microsecond -> 7.
        | _ -> failwith "bad precision"

    // mask to closest 2^N tick interval (i.e. clear low N bits)
    let mask = ~~~ ( 2. ** power - 1. |> uint64 )
    let maskedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> t - epochTicks )
        |> Seq.map ( fun t -> uint64 t &&& mask |> int64 )
        |> Seq.map ( fun t -> t + epochTicks )
        |> Seq.toArray

    let encodedAndExtractedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> gen.Next t )
        |> Seq.map ( fun id -> IdGenerator.ExtractTicks( id , timePrecision = precision ) )
        |> Seq.toArray

    Expect.equal encodedAndExtractedIntervals maskedIntervals "encoded/extraced ticks should match masked ticks"

let testPrecisionWithMicroIntervals precision intervalMicroseconds itemCount =

    let gen = IdGenerator( timePrecision = precision )
    let startTicks = 637045381230040000L  // 2019-09-20 01:02:03.004

    let ticksPerMicrosecond = 10L

    let intervalTicks =
        Seq.init itemCount ( fun i -> int64 i * intervalMicroseconds * ticksPerMicrosecond + startTicks )

    let power =
        match precision with
        | IdTimePrecision.Second -> 23.
        | IdTimePrecision.Millisecond -> 15.
        | IdTimePrecision.Microsecond -> 7.
        | _ -> failwith "bad precision"

    // mask to closest 2^N tick interval (i.e. clear low N bits)
    let mask = ~~~ ( 2. ** power - 1. |> uint64 )
    let maskedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> t - epochTicks )
        |> Seq.map ( fun t -> uint64 t &&& mask |> int64 )
        |> Seq.map ( fun t -> t + epochTicks )
        |> Seq.toArray

    let encodedAndExtractedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> gen.Next t )
        |> Seq.map ( fun id -> IdGenerator.ExtractTicks( id , timePrecision = precision ) )
        |> Seq.toArray

    Expect.equal encodedAndExtractedIntervals maskedIntervals "encoded/extraced ticks should match masked ticks"

let testPrecisionWithTickIntervals precision intervalTicks itemCount =

    let gen = IdGenerator( timePrecision = precision )
    let startTicks = 637045381230040000L  // 2019-09-20 01:02:03.004

    let intervalTicks =
        Seq.init itemCount ( fun i -> int64 i * intervalTicks + startTicks )

    let power =
        match precision with
        | IdTimePrecision.Second -> 23.
        | IdTimePrecision.Millisecond -> 15.
        | IdTimePrecision.Microsecond -> 7.
        | _ -> failwith "bad precision"

    // mask to closest 2^N tick interval (i.e. clear low N bits)
    let mask = ~~~ ( 2. ** power - 1. |> uint64 )
    let maskedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> t - epochTicks )
        |> Seq.map ( fun t -> uint64 t &&& mask |> int64 )
        |> Seq.map ( fun t -> t + epochTicks )
        |> Seq.toArray

    let encodedAndExtractedIntervals =
        intervalTicks
        |> Seq.map ( fun t -> gen.Next t )
        |> Seq.map ( fun id -> IdGenerator.ExtractTicks( id , timePrecision = precision ) )
        |> Seq.toArray

    Expect.equal encodedAndExtractedIntervals maskedIntervals "encoded/extraced ticks should match masked ticks"

let commonTests =
    testList "commonTests" [

        testCase "default length" <| fun () ->
            Expect.equal ( IdGenerator().Next().Length ) 24 ""

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
            let extractTime = IdGenerator.ExtractDate theId
            Expect.equal extractTime.Year time.Year "Year should be equal"
            Expect.equal extractTime.Month time.Month "Month should be equal"
            Expect.equal extractTime.Day time.Day "Day should be equal"
            Expect.equal extractTime.Hour time.Hour "Hour should be equal"
            Expect.equal extractTime.Minute time.Minute "Minute should be equal"
            // Second not guaranteed exact due to precision
            Expect.isTrue ( extractTime.Second = time.Second || extractTime.Second + 1 = time.Second ) "Second should be within one"

        testCase "timestamp round trip binary" <| fun () ->
            let time = DateTimeOffset( 2021 , 3 , 4 , 5 , 6 , 7 , TimeSpan.Zero )
            let theId = IdGenerator().NextBinary time
            let extractTime = IdGenerator.ExtractDate theId
            Expect.equal extractTime.Year time.Year "Year should be equal"
            Expect.equal extractTime.Month time.Month "Month should be equal"
            Expect.equal extractTime.Day time.Day "Day should be equal"
            Expect.equal extractTime.Hour time.Hour "Hour should be equal"
            Expect.equal extractTime.Minute time.Minute "Minute should be equal"
            // Second not guaranteed exact due to precision
            Expect.isTrue ( extractTime.Second = time.Second || extractTime.Second + 1 = time.Second ) "Second should be within one"

        testCase "two string ids different" <| fun () ->
            // infinitesimal chance of collision
            let idGen = IdGenerator()
            let id1 , id2 = idGen.Next() , idGen.Next()
            Expect.notEqual id1 id2 ""

        testCase "two binary ids different" <| fun () ->
            // infinitesimal chance of collision
            let idGen = IdGenerator()
            let id1 , id2 = idGen.NextBinary() , idGen.NextBinary()
            Expect.notEqual id1 id2 ""

        testCase "string binary string round trip" <| fun () ->
            let stringId = IdGenerator().Next()
            let binaryId = IdGenerator.ConvertToBinary stringId
            let stringIdRoundTrip = IdGenerator.ConvertToString binaryId
            Expect.equal stringId stringIdRoundTrip ""

        testCase "binary string binary round trip" <| fun () ->
            let binaryId = IdGenerator().NextBinary()
            let stringId = IdGenerator.ConvertToString binaryId
            let binaryIdRoundTrip = IdGenerator.ConvertToBinary stringId
            Expect.equal binaryId binaryIdRoundTrip ""

        testCase "Second precision, 2345 ms separated timestamps" <| fun () ->
            testPrecisionWithMilliIntervals IdTimePrecision.Second 2345L 999

        testCase "Second precision, 321 ms separated timestamps" <| fun () ->
            testPrecisionWithMilliIntervals IdTimePrecision.Second 321L 999

        testCase "Millisecond precision, 5 ms separated timestamps" <| fun () ->
            testPrecisionWithMilliIntervals IdTimePrecision.Millisecond 5L 999

        testCase "Millisecond precision, 1 ms separated timestamps" <| fun () ->
            testPrecisionWithMilliIntervals IdTimePrecision.Millisecond 1L 999

        testCase "Microsecond precision, 1 ms separated timestamps" <| fun () ->
            testPrecisionWithMilliIntervals IdTimePrecision.Microsecond 1L 999

        testCase "Second precision bits from ticks" <| fun () ->

            let precision = IdTimePrecision.Second

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_10000000_00000000_00000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_00000000_00000000_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_10000000_00000000_00000000L + epochTicks

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next ticksExactBits
            let idOutsideBits = gen.Next ticksOutsideBits
            let idLoChange = gen.Next ticksLoChange
            let idHiChange = gen.Next ticksHiChange

            let extractExactBits = IdGenerator.ExtractTicks( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractTicks( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractTicks( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractTicks( idHiChange , precision )

            Expect.equal extractOutsideBits extractExactBits "extra should equal exact"
            Expect.notEqual extractLoChange extractExactBits "low change should not equal exact"
            Expect.notEqual extractHiChange extractExactBits "high change should not equal exact"

        testCase "Millisecond precision bits from ticks" <| fun () ->

            let precision = IdTimePrecision.Millisecond

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_11111111_10000000_00000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_11111111_00000000_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_11111111_10000000_00000000L + epochTicks

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next ticksExactBits
            let idOutsideBits = gen.Next ticksOutsideBits
            let idLoChange = gen.Next ticksLoChange
            let idHiChange = gen.Next ticksHiChange

            let extractExactBits = IdGenerator.ExtractTicks( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractTicks( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractTicks( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractTicks( idHiChange , precision )

            Expect.equal extractOutsideBits extractExactBits "extra should equal exact"
            Expect.notEqual extractLoChange extractExactBits "low change should not equal exact"
            Expect.notEqual extractHiChange extractExactBits "high change should not equal exact"

        testCase "Microsecond precision bits from ticks" <| fun () ->

            let precision = IdTimePrecision.Microsecond

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_11111111_11111111_10000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_11111111_11111111_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_11111111_11111111_10000000L + epochTicks

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next ticksExactBits
            let idOutsideBits = gen.Next ticksOutsideBits
            let idLoChange = gen.Next ticksLoChange
            let idHiChange = gen.Next ticksHiChange

            let extractExactBits = IdGenerator.ExtractTicks( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractTicks( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractTicks( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractTicks( idHiChange , precision )

            Expect.equal extractOutsideBits extractExactBits "extra should equal exact"
            Expect.notEqual extractLoChange extractExactBits "low change should not equal exact"
            Expect.notEqual extractHiChange extractExactBits "high change should not equal exact"

    ]

let netOnlyTests =
    testList "netOnlyTests" [

        // Test less than 1 ms resolution only on .NET

        testCase "Millisecond precision, 789 us separated timestamps" <| fun () ->
            testPrecisionWithMicroIntervals IdTimePrecision.Millisecond 789L 999

        testCase "Microsecond precision, 23 us separated timestamps" <| fun () ->
            testPrecisionWithMicroIntervals IdTimePrecision.Microsecond 23L 999

        testCase "Microsecond precision, 5 us separated timestamps" <| fun () ->
            testPrecisionWithMicroIntervals IdTimePrecision.Microsecond 5L 999

        testCase "Microsecond precision, 3 tick separated timestamps" <| fun () ->
            testPrecisionWithTickIntervals IdTimePrecision.Microsecond 3L 999


        // Test bits via DateTimeOffset only on .NET, can't test on Fable since they get rounded to milliseconds.

        testCase "Second precision bits from DateTimeOffset" <| fun () ->

            let precision = IdTimePrecision.Second

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_10000000_00000000_00000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_00000000_00000000_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_10000000_00000000_00000000L + epochTicks

            let dateExactBits = DateTimeOffset( ticksExactBits , TimeSpan.Zero )
            let dateOutsideBits = DateTimeOffset( ticksOutsideBits , TimeSpan.Zero )
            let dateLoChange = DateTimeOffset( ticksLoChange , TimeSpan.Zero )
            let dateHiChange = DateTimeOffset( ticksHiChange , TimeSpan.Zero )

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next dateExactBits
            let idOutsideBits = gen.Next dateOutsideBits
            let idLoChange = gen.Next dateLoChange
            let idHiChange = gen.Next dateHiChange

            let extractExactBits = IdGenerator.ExtractDate( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractDate( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractDate( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractDate( idHiChange , precision )

            Expect.equal extractOutsideBits.UtcTicks extractExactBits.UtcTicks "extra should equal exact"
            Expect.notEqual extractLoChange.UtcTicks extractExactBits.UtcTicks "low change should not equal exact"
            Expect.notEqual extractHiChange.UtcTicks extractExactBits.UtcTicks "high change should not equal exact"


        testCase "Millisecond precision bits from DateTimeOffset" <| fun () ->

            let precision = IdTimePrecision.Millisecond

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_11111111_10000000_00000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_11111111_00000000_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_11111111_10000000_00000000L + epochTicks

            let timestampExactBits = DateTimeOffset( ticksExactBits , TimeSpan.Zero )
            let timestampOutsideBits = DateTimeOffset( ticksOutsideBits , TimeSpan.Zero )
            let timestampLoChange = DateTimeOffset( ticksLoChange , TimeSpan.Zero )
            let timestampHiChange = DateTimeOffset( ticksHiChange , TimeSpan.Zero )

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next timestampExactBits
            let idOutsideBits = gen.Next timestampOutsideBits
            let idLoChange = gen.Next timestampLoChange
            let idHiChange = gen.Next timestampHiChange

            let extractExactBits = IdGenerator.ExtractDate( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractDate( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractDate( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractDate( idHiChange , precision )

            Expect.equal extractOutsideBits.UtcTicks extractExactBits.UtcTicks "extra should equal exact"
            Expect.notEqual extractLoChange.UtcTicks extractExactBits.UtcTicks "low change should not equal exact"
            Expect.notEqual extractHiChange.UtcTicks extractExactBits.UtcTicks "high change should not equal exact"


        testCase "Microsecond precision bits from DateTimeOffset" <| fun () ->

            let precision = IdTimePrecision.Microsecond

            // Set up artificial timestamps to be encoded.
            // We add the epochTicks, since encoder will subtract them internally, resulting in our artificial timestamp to be encoded.
            // Differing bits outside the precision range should not change the encoded timestamp.
            // Differing bits inside the precision range should change the encoded timestamp.
            // It doesn't matter that we are outside the 114 year window, it will get masked and we only care about the results relative to each other, not whether they match the original.

            // Bits representing the exact precision range expected to be encoded
            let ticksExactBits = 0b00011000_01111111_11111111_11111111_11111111_11111111_11111111_10000000L + epochTicks

            // Extra bits outside precision range, should resolve to same timestamp as exact
            let ticksOutsideBits = 0b00011000_11111111_11111111_11111111_11111111_11111111_11111111_11111111L + epochTicks

            // Changed low bit inside precision range, should resolve to different timestamp
            let ticksLoChange = 0b00011000_01111111_11111111_11111111_11111111_11111111_11111111_00000000L + epochTicks

            // Changed high bit inside precision range, should resolve to different timestamp
            let ticksHiChange = 0b00011000_00111111_11111111_11111111_11111111_11111111_11111111_10000000L + epochTicks

            let timestampExactBits = DateTimeOffset( ticksExactBits , TimeSpan.Zero )
            let timestampOutsideBits = DateTimeOffset( ticksOutsideBits , TimeSpan.Zero )
            let timestampLoChange = DateTimeOffset( ticksLoChange , TimeSpan.Zero )
            let timestampHiChange = DateTimeOffset( ticksHiChange , TimeSpan.Zero )

            let gen = IdGenerator( timePrecision = precision )

            let idExactBits = gen.Next timestampExactBits
            let idOutsideBits = gen.Next timestampOutsideBits
            let idLoChange = gen.Next timestampLoChange
            let idHiChange = gen.Next timestampHiChange

            let extractExactBits = IdGenerator.ExtractDate( idExactBits , precision )
            let extractOutsideBits = IdGenerator.ExtractDate( idOutsideBits , precision )
            let extractLoChange = IdGenerator.ExtractDate( idLoChange , precision )
            let extractHiChange = IdGenerator.ExtractDate( idHiChange , precision )

            Expect.equal extractOutsideBits.UtcTicks extractExactBits.UtcTicks "extra should equal exact"
            Expect.notEqual extractLoChange.UtcTicks extractExactBits.UtcTicks "low change should not equal exact"
            Expect.notEqual extractHiChange.UtcTicks extractExactBits.UtcTicks "high change should not equal exact"

    ]

let allTests = testList "All" [
    commonTests

    #if ! FABLE_COMPILER
    netOnlyTests
    #endif
]

