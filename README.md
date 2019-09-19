# IdGenerator

A unique id generator for .NET and Fable, using timestamp plus random data, with multiple strength and precision options. Flexible alternative to UUID/GUID.

## Overview

Generates universally unique string ids consisting of a sortable timestamp component followed by randomly generated data. The generator can be configured for different size and precision levels.
 
## Examples

### Small id

```
BFKdJmcFtSqbSmrJ
[-time-][random]
```

### Medium id

```
BFKdJmcFjMjrqhJxZPDhSstF
[-time-][----random----]
```

### Large id

```
BFKdJmcFXsrJpKJdtCTNhCBsmfzqXrTb
[-time-][--------random--------]
```

### Extra large id

```
BFKdJmcFNpBXhNNrRscZkSZJXNhTmMmTHJRhrZMx
[-time-][------------random------------]
```

## Properties

### Size, resolution, and bit distribution

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 6.6 ms | 40 | 40  | 80  |
| **Medium**      | 24 characters | 6.6 ms | 40 | 80  | 120 |
| **Large**       | 32 characters | 6.6 ms | 40 | 120 | 160 |
| **Extra Large** | 40 characters | 6.6 ms | 40 | 160 | 200 |

### Timestamp

* The default 40 bit timestamp has approximately 6.6 millisecond resolution. That is, ids generated within the same 6.6 ms interval will have the same timestamp portion. It has a range of over 228 years before cycling, good until the year 2247 (starting from 2019).
* The time precision is configurable to increase or decrease the resolution, trading off random bits to do so. See [Adjustable time precision].

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

The timestamp is based on the number of 100 nanosecond intervals (ticks) since a fixed epoch date of 2019-01-01, taken as a 56 bit (7 byte) integer which can store a range of over 228 years. (2^56 100 ns ticks = 7205759404 seconds = approximately 228.3 years)

Then, depending on desired precision, anywhere from the high 4 bytes to all 7 bytes of that value are stored at the beginning of the id in big-endian order.

The number of random bits used in the id are adjusted depending on the precision to maintain a consistent overall string length.

### Tick precision

56 bit timestamp, 100 nanosecond resolution.

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 100 ns | 56 | 24  | 80  |
| **Medium**      | 24 characters | 100 ns | 56 | 64  | 120 |
| **Large**       | 32 characters | 100 ns | 56 | 104 | 160 |
| **Extra Large** | 40 characters | 100 ns | 56 | 144 | 200 |

### Microsecond precision

48 bit timestamp, approximately 26 microsecond resolution. (100 ns * 2^8 = 25.6 us)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 26 us | 48 | 32  | 80  |
| **Medium**      | 24 characters | 26 us | 48 | 72  | 120 |
| **Large**       | 32 characters | 26 us | 48 | 112 | 160 |
| **Extra Large** | 40 characters | 26 us | 48 | 152 | 200 |

### Millisecond precision (default)

40 bit timestamp, approximately 6.6 millisecond resolution. (100 ns * 2^16 = 6.5536 ms)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 6.6 ms | 40 | 40  | 80  |
| **Medium**      | 24 characters | 6.6 ms | 40 | 80  | 120 |
| **Large**       | 32 characters | 6.6 ms | 40 | 120 | 160 |
| **Extra Large** | 40 characters | 6.6 ms | 40 | 160 | 200 |

### Second precision

32 bit timestamp, approximately 1.7 second resolution. (100 ns * 2^24 = 1.6777216 s)

|   | String length | Time resolution | Time bits | Random bits | Total bits |
| --- | :---: | :---: | :---: | :---: | :---: |
| **Small**       | 16 characters | 1.7 s | 32 | 48  | 80  |
| **Medium**      | 24 characters | 1.7 s | 32 | 88  | 120 |
| **Large**       | 32 characters | 1.7 s | 32 | 128 | 160 |
| **Extra Large** | 40 characters | 1.7 s | 32 | 168 | 200 |

### Notes

* Time precision is subject to the system clock resolution of the target environment. The extra bits for a more precise timestamp will be wasted if the system clock does not update at that resolution.
* The precision options do not affect the initial bits of the timestamp. The same first 32 bits of the timestamp are used regardless of precison, so the first 6 characters (30 encoded bits) of an id will be consistent under any precision option.
* With Fable, the Tick and Microsecond precision options are not supported and will fall back to Millisecond precision.

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
var timestamp = IdGenerator.ExtractTimestamp(myId);

// Create a generator instance for Large ids
var idGenLarge = new IdGenerator(IdSize.Large);

// Create a generator instance for Large ids using Microsecond time precision
var idGenMicro = new IdGenerator(IdSize.Large, IdTimePrecision.Microsecond);

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
let timestamp = IdGenerator.ExtractTimestamp(myId)

// Create a generator instance for Large ids
let idGenLarge = IdGenerator(IdSize.Large)

// Create a generator instance for Large ids using Microsecond time precision
let idGenMicro = IdGenerator(IdSize.Large, IdTimePrecision.Microsecond)

// Create a generator that buffers 100 random data blocks between calls to the crypto RNG source
let idBuffer = IdGenerator(bufferCount=100)
```
