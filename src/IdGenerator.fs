namespace Zanaptak.IdGenerator

open System

module private Buffer =

#if FABLE_COMPILER
  open Fable.Core
  open Fable.Core.JsInterop
  let [<Global>] window: obj = jsNative
  [<Emit("typeof $0 !== 'undefined'")>]
  let isNotTypeofUndefined (x: 'a) : bool = jsNative
#if ZANAPTAK_NODEJS_CRYPTO
  let crypto : obj = importDefault "crypto"
#endif
#endif

  type CryptoRandom ( bufferSize : uint16 ) =
    let bufferSize = int bufferSize
    let lockObj = obj()

    let byteBuffer : byte array = Array.zeroCreate ( bufferSize )
    let mutable bufferPos = bufferSize

#if FABLE_COMPILER
#if ZANAPTAK_NODEJS_CRYPTO
    let fillRandomBytes =
      fun ( bytes : byte array ) -> crypto?randomFillSync( bytes )
#else
    let fillRandomBytes =
      if isNotTypeofUndefined window && window?crypto && window?crypto?getRandomValues then
        fun ( bytes : byte array ) -> window?crypto?getRandomValues( bytes )
      else
        let systemRng = System.Random()
        fun ( bytes : byte array ) -> systemRng.NextBytes( bytes )
#endif
#else
    let fillRandomBytes =
      let cryptoRng = System.Security.Cryptography.RandomNumberGenerator.Create()
      fun ( bytes : byte array ) -> cryptoRng.GetBytes( bytes )
#endif

    let refillBuffer() =
      fillRandomBytes byteBuffer
      bufferPos <- 0

    let ensureBuffer numBytes =
      if bufferSize - bufferPos < numBytes then refillBuffer()

    member this.NextBytes ( bytes : byte array ) ( index : int ) ( length : int ) =
      lock lockObj ( fun () ->
        ensureBuffer length
        Array.Copy ( byteBuffer , bufferPos , bytes , index , length )
        bufferPos <- bufferPos + length
      )

open System.Runtime.InteropServices

type IdSize =
  /// 16 character, 80 bit id (with 40 random bits). This is the default size.
  | Small = 0
  /// 24 character, 120 bit id (with 80 random bits).
  | Medium = 1
  /// 32 character, 160 bit id (with 120 random bits).
  | Large = 2
  /// 40 character, 200 bit id (with 160 random bits).
  | ExtraLarge = 3

type IdTimePrecision =
  /// 32 bit time precision, 0.84 second resolution. Increases amount of random data by 8 bits from the default.
  | Second = 0
  /// 40 bit time precision, 3.3 millisecond resolution. This is the default precision.
  | Millisecond = 1
  /// 48 bit time precision, 13 microsecond resolution. Decreases amount of random data by 8 bits from the default.
  | Microsecond = 2

///<summary>Creates a string id generator.</summary>
///<param name='size'>Specifies the size of id this generator instance will create. Default is IdSize.Small.</param>
///<param name='timePrecision'>Specifies the precision of the timestamp portion of generated ids. Default is IdTimePrecision.Millisecond.</param>
///<param name='bufferCount'>Specifies the number of ids that the generator will buffer random data for between calls to the cryptographic RNG source.
/// Minimum and default value is 10 ids. Maximum buffer size is 65535 bytes regardless of id count.</param>
type IdGenerator
  (
    [< Optional ; DefaultParameterValue( IdSize.Small ) >] size : IdSize
    , [< Optional ; DefaultParameterValue( IdTimePrecision.Millisecond ) >] timePrecision : IdTimePrecision
    , [< Optional ; DefaultParameterValue( 10 ) >] bufferCount : int
  ) =

  static let epochTicks = DateTimeOffset( 2019 , 9 , 19 , 0 , 0 , 0 , 0 , TimeSpan.Zero ).Ticks
  static let base32 = Zanaptak.BinaryToTextEncoding.Base32( "BCDFHJKMNPQRSTXZbcdfhjkmnpqrstxz" )

  // timeBitShift (low bits to discard) , timeByteCount , randomByteAdjust
  // timeBitShift + (8 * timeByteCount) = using 55 bits of the tick value = 114 year range
  static let getCalcParams ( timePrecision : IdTimePrecision ) =
    match timePrecision with
    #if ! FABLE_COMPILER
    | IdTimePrecision.Microsecond -> 7 , 6 , -1 // not supported in Fable
    #endif
    | IdTimePrecision.Second -> 23 , 4 , 1
    | _ -> 15 , 5 , 0 // default IdTimePrecision.Millisecond

  let timeBitShift , timeByteCount , randomByteAdjust = getCalcParams timePrecision

  let fillTimeBytesInArray ( bytes : byte array ) ( timestamp : DateTimeOffset ) =
    let time = ( timestamp.UtcTicks - epochTicks ) >>> timeBitShift
    for i in 0 .. timeByteCount - 1 do
      bytes.[ i ] <- time >>> ( timeByteCount - i - 1 ) * 8 |> byte

  let randomByteCount =
    (
      match size with
      | IdSize.ExtraLarge -> 20
      | IdSize.Large -> 15
      | IdSize.Medium -> 10
      | _ -> 5 // default IdSize.Small
    )
    + randomByteAdjust

  let totalByteCount = timeByteCount + randomByteCount

  let cryptoRng = Buffer.CryptoRandom ( ( max bufferCount 10 ) * randomByteCount |> min ( int UInt16.MaxValue ) |> uint16 )

  /// Returns a string id constructed using the current system time and additional random data.
  member this.Next() =
    let bytes = Array.create totalByteCount 0uy
    cryptoRng.NextBytes bytes timeByteCount randomByteCount
    fillTimeBytesInArray bytes DateTimeOffset.UtcNow
    base32.Encode( bytes )

  /// Returns a string id constructed using the provided time value and additional random data.
  member this.Next( timestamp : DateTimeOffset ) =
    let bytes = Array.create totalByteCount 0uy
    cryptoRng.NextBytes bytes timeByteCount randomByteCount
    fillTimeBytesInArray bytes timestamp
    base32.Encode( bytes )

  /// Returns a DateTimeOffset of the timestamp that is encoded in the provided string id.
  static member ExtractTimestamp( id : string , [< Optional ; DefaultParameterValue( IdTimePrecision.Millisecond ) >] timePrecision : IdTimePrecision ) =
    try
      let timeBitShift , timeByteCount , _ = getCalcParams timePrecision
      let timeBytes = base32.Decode id |> Array.take timeByteCount
      let prefixBytes : byte array = Array.zeroCreate ( 8 - timeByteCount )
      let timeValue =
        Array.append prefixBytes timeBytes
        |> fun a -> if BitConverter.IsLittleEndian then Array.rev a else a
        |> fun a -> BitConverter.ToInt64( a , 0 )
      let ticks = timeValue <<< timeBitShift
      DateTimeOffset( epochTicks + ticks , TimeSpan.Zero )
    with
    | ex -> raise ( FormatException( "Invalid id" , ex ) )
