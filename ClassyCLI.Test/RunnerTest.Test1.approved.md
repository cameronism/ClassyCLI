strings are easy

# Running E1 O1 42

# Invoked E1 O1

## s System.String

"42"


other types

# Running E1 O2 42

# Invoked E1 O2

## i System.Int32

42


# Running E1 O3 42

# Invoked E1 O3

## i System.Nullable<System.Int32>

42


# Running E1 O4 1

# Invoked E1 O4

## d System.DayOfWeek

"Monday"


# Running E1 O4 Sunday

# Invoked E1 O4

## d System.DayOfWeek

"Sunday"


# Running E1 O5 tuesday

# Invoked E1 O5

## d System.Nullable<System.DayOfWeek>

"Tuesday"


# Running E1 O5 

# Invoked E1 O5

## d System.Nullable<System.DayOfWeek>

null


# Running E1 O3 

# Invoked E1 O3

## i System.Nullable<System.Int32>

null


# Running E1 O6

# Invoked E1 O6

## d System.Nullable<System.DayOfWeek>

"Friday"


# Running E1 O7 2017-10-28 Thursday

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Thursday"


# Running E1 O7 2017-10-28

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Friday"


# Running E1 O8 -

# Invoked E1 O8

## s System.IO.Stream



# Running E1 O9 -

# Invoked E1 O9

## s System.IO.Stream



# Running E1 O9

# Invoked E1 O9

## s System.IO.Stream

null


# Running E1 OA -

# Invoked E1 OA

## t System.IO.TextReader



# Running E1 O8 ./test.tmp.txt

# Invoked E1 O8

## s System.IO.Stream



# Running E1 O9 ./test.tmp.txt

# Invoked E1 O9

## s System.IO.Stream



# Running E1 OA ./test.tmp.txt

# Invoked E1 OA

## t System.IO.TextReader



# Running E1 OB ./test.tmp.txt

# Invoked E1 OB

## f System.IO.FileInfo



# Running E1 OC .

# Invoked E1 OC

## d System.IO.DirectoryInfo



# Running E1 OD -

# Invoked E1 OD

## t System.IO.TextWriter



Do not allow TextWriter to open existing file (by default)
# Running E1 OD ./test.tmp.txt

# Exception System.IO.IOException


# Running E1 O1 -s hello

# Invoked E1 O1

## s System.String

"hello"


# Running E1 O7 -d 2017-10-28 Thursday

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Thursday"


# Running E1 O7 -d 2017-10-28 -w Thursday

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Thursday"


# Running E1 O7 -w Thursday -d 2017-10-28

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Thursday"


# Running E1 O7 -w Thursday 2017-10-28

# Invoked E1 O7

## d System.DateTime

"2017-10-28T00:00:00"

## w System.Nullable<System.DayOfWeek>

"Thursday"


# Running E1 OE -- -a -b

# Invoked E1 OE

## a System.String

"-a"

## b System.String

"-b"


# Running E1 OE -b bbbbb -- -a

# Invoked E1 OE

## a System.String

"-a"

## b System.String

"bbbbb"


# Running E1 OE -a aaaaa -- -b

# Invoked E1 OE

## a System.String

"aaaaa"

## b System.String

"-b"


# Running E1 OF a b c

# Invoked E1 OF

## ss System.String[]

[
  "a",
  "b",
  "c"
]


# Running E1 OG a b c d

# Invoked E1 OG

## oo System.Collections.Generic.List<System.Object>

[
  "a",
  "b",
  "c",
  "d"
]


# Running E1 OH 1 2

# Invoked E1 OH

## ii System.Collections.Generic.IEnumerable<System.Int32>

[
  1,
  2
]


# Running E1 OI s 2018-01-01 2019-01-01

# Invoked E1 OI

## s System.String

"s"

## dd System.Collections.Generic.IList<System.DateTime>

[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]


# Running E1 OI -s s 2018-01-01 2019-01-01

# Invoked E1 OI

## s System.String

"s"

## dd System.Collections.Generic.IList<System.DateTime>

[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]


# Running E1 OI s -d 2018-01-01

# Invoked E1 OI

## s System.String

"s"

## dd System.Collections.Generic.IList<System.DateTime>

[
  "2018-01-01T00:00:00"
]


# Running E1 OI -s s -d 2018-01-01

# Invoked E1 OI

## s System.String

"s"

## dd System.Collections.Generic.IList<System.DateTime>

[
  "2018-01-01T00:00:00"
]


# Running E1 OI -d 2018-01-01 -s s -d 2019-01-01

# Invoked E1 OI

## s System.String

"s"

## dd System.Collections.Generic.IList<System.DateTime>

[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]


# Running E1 OJ

# Invoked E1 OJ

## ii System.Collections.Generic.IEnumerable<System.Int32>

null


# Running E1 OJ 1

# Invoked E1 OJ

## ii System.Collections.Generic.IEnumerable<System.Int32>

[
  1
]


# Running E2 OJ 1

# Invoked E2 OJ

## ii System.Collections.Generic.IEnumerable<System.Int32>

[
  1
]


# Running E2 TwoVoid

# Invoked E2 TwoVoid


# Running E1 OK true

# Invoked E1 OK

## b System.Boolean

true


# Running E1 OL true

# Invoked E1 OL

## b System.Nullable<System.Boolean>

true


# Running E1 OL null

# Invoked E1 OL

## b System.Nullable<System.Boolean>

null


# Running E1 OM foo

# Invoked E1 OM

## ct ClassyCLI.Test.RunnerTest+CustomType

{
  "Value": "foo"
}


