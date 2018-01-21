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


