# IdGenerator

A unique id generator for .NET and Fable, using timestamp plus random data, with multiple strength and precision options. Flexible alternative to UUID/GUID.

## Overview

Generates universally unique string ids consisting of a sortable timestamp component followed by randomly generated data. The generator can be configured for different size and precision levels.
 
## Examples

### Small id

```
BBBbpQsdtSqbSmrJ
[-time-][random]
```

### Medium id (default)

```
BBBbpQsdjMjrqhJxZPDhSstF
[-time-][----random----]
```

### Large id

```
BBBbpQsdXsrJpKJdtCTNhCBsmfzqXrTb
[-time-][--------random--------]
```

### Extra large id

```
BBBbpQsdNpBXhNNrRscZkSZJXNhTmMmTHJRhrZMx
[-time-][------------random------------]
```

## Properties

### Size, resolution, and bit distribution

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 3.3 ms | 40 | 40  | 80  |
| **Medium**      | 24 characters | 3.3 ms | 40 | 80  | 120 |
| **Large**       | 32 characters | 3.3 ms | 40 | 120 | 160 |
| **Extra Large** | 40 characters | 3.3 ms | 40 | 160 | 200 |

### Timestamp

* The default 40 bit timestamp has approximately 3.3 millisecond resolution. That is, ids generated within the same 3.3 ms interval will have the same timestamp component. It has a range of over 114 years before cycling, good until the year 2133 (starting from 2019).
* The time precision is configurable to increase or decrease the resolution, trading off random bits to do so. See [Adjustable time precision](#adjustable-time-precision).

### Random data

* The random data portion is generated from a cryptographic source when available, for improved statistical quality.
  * For .NET, [`Cryptography.RandomNumberGenerator`](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=netstandard-2.0) is used.
  * For Fable, [`window.crypto.getRandomValues`](https://developer.mozilla.org/en-US/docs/Web/API/Crypto/getRandomValues) is used if available, otherwise falling back to `Math.random`.
    * When compiling a Node.js app with Fable, set the `ZANAPTAK_NODEJS_CRYPTO` compilation symbol to use [`crypto.randomFillSync`](https://nodejs.org/api/crypto.html#crypto_crypto_randomfillsync_buffer_offset_size) instead of `Math.random`.

### String format

* The id is formatted as a base 32 encoded string; each text character represents 5 bits of binary data.
  * No binary version is provided. The simplicity and ubiquity of a canonical string format is favored over the space savings of a binary representation.
* Custom base 32 character set consisting of: `BCDFHJKMNPQRSTXZbcdfhjkmnpqrstxz`
  * Digits and several confusable characters are excluded.
  * Vowels and vowel-like characters are excluded to reduce the chance of accidental word patterns.
  * Case-sensitive.
* Ids will sort in increasing timestamp order when using ordinal string sort.

## Adjustable time precision

The timestamp is based on the number of 100 nanosecond intervals (ticks) since a fixed epoch date of 2019-09-19 00:00:00 UTC, masked to the low 55 bits. This provides a range of just over 114 years. (100 ns * 2<sup>55</sup> = 3602879702 seconds = about 114.17 years)

Depending on the desired precision, the high 32, 40, or 48 bits of that value are stored at the beginning of the generated id in big-endian order.

The number of random bits used in the id are adjusted depending on the precision to maintain a consistent overall string length.

### Microsecond precision

48 bit timestamp, approximately 13 microsecond resolution. (100 ns * 2<sup>55</sup> / 2<sup>48</sup> = 12.8 us)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 13 us | 48 | 32  | 80  |
| **Medium**      | 24 characters | 13 us | 48 | 72  | 120 |
| **Large**       | 32 characters | 13 us | 48 | 112 | 160 |
| **Extra Large** | 40 characters | 13 us | 48 | 152 | 200 |

### Millisecond precision (default)

40 bit timestamp, approximately 3.3 millisecond resolution. (100 ns * 2<sup>55</sup> / 2<sup>40</sup> = 3.2768 ms)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 3.3 ms | 40 | 40  | 80  |
| **Medium**      | 24 characters | 3.3 ms | 40 | 80  | 120 |
| **Large**       | 32 characters | 3.3 ms | 40 | 120 | 160 |
| **Extra Large** | 40 characters | 3.3 ms | 40 | 160 | 200 |

### Second precision

32 bit timestamp, approximately 0.84 second resolution. (100 ns * 2<sup>55</sup> / 2<sup>32</sup> = 0.8388608 s)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 0.84 s | 32 | 48  | 80  |
| **Medium**      | 24 characters | 0.84 s | 32 | 88  | 120 |
| **Large**       | 32 characters | 0.84 s | 32 | 128 | 160 |
| **Extra Large** | 40 characters | 0.84 s | 32 | 168 | 200 |

### Notes

* Time precision is subject to the system clock resolution of the target environment. The extra bits for a more precise timestamp will be wasted if the system clock does not update at that resolution.
* The precision options do not affect the initial bits of the timestamp. The same first 32 bits of the timestamp are used regardless of precison, so the first 6 characters (30 encoded bits) of an id will be consistent under any precision option.
* In Fable, dates are subject to the 1 ms JavaScript date resolution rather than 100 ns tick resolution of .NET. Therefore, in Fable:
  * The Microsecond precision option is not supported; Millisecond will be used instead.
  * Extracting dates from ids will not produce an exact result since conversion to milliseconds will discard up to 9999 ticks of the encoded value. Alternatively, the exact tick value can be extracted accurately with `ExtractTicks`, for parity with .NET (but will still lose the extra tick precision if subsequently converted to a date in Fable).


## Usage

Add the [NuGet package](https://www.nuget.org/packages/Zanaptak.IdGenerator) to your project:

```
dotnet add package Zanaptak.IdGenerator
```

### C#

```cs
using Zanaptak.IdGenerator;

// Create a generator instance
var idGenerator = new IdGenerator();

// Generate an id
var myId = idGenerator.Next();

// Get the timestamp from an id as a DateTimeOffset
var date = IdGenerator.ExtractDate(myId);

// Create a generator instance for Small ids
var idGenSmall = new IdGenerator(IdSize.Small);

// Create a generator instance for Large ids using Microsecond time precision
var idGenLargeMicro = new IdGenerator(IdSize.Large, IdTimePrecision.Microsecond);

// Create a generator that buffers 100 random data blocks between calls to the crypto RNG source
var idBuffer = new IdGenerator(bufferCount: 100);
```

### F#

```fs
open Zanaptak.IdGenerator

// Create a generator instance
let idGenerator = IdGenerator()

// Generate an id
let myId = idGenerator.Next()

// Get the timestamp from an id as a DateTimeOffset
let date = IdGenerator.ExtractDate(myId)

// Create a generator instance for Small ids
let idGenSmall = IdGenerator(IdSize.Small)

// Create a generator instance for Large ids using Microsecond time precision
let idGenLargeMicro = IdGenerator(IdSize.Large, IdTimePrecision.Microsecond)

// Create a generator that buffers 100 random data blocks between calls to the crypto RNG source
let idBuffer = IdGenerator(bufferCount=100)
```
