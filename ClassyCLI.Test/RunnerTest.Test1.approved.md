# strings are easy

## Running E1.O1 42

`E1.O1`

### s System.String

```
"42"
```


# other types

## Running E1.O2 42

`E1.O2`

### i System.Int32

```
42
```


## Running E1.O3 42

`E1.O3`

### i System.Nullable<System.Int32>

```
42
```


## Running E1.O4 1

`E1.O4`

### d System.DayOfWeek

```
"Monday"
```


## Running E1.O4 Sunday

`E1.O4`

### d System.DayOfWeek

```
"Sunday"
```


## Running E1.O5 tuesday

`E1.O5`

### d System.Nullable<System.DayOfWeek>

```
"Tuesday"
```


## Running E1.O5 

`E1.O5`

### d System.Nullable<System.DayOfWeek>

```
null
```


## Running E1.O3 

`E1.O3`

### i System.Nullable<System.Int32>

```
null
```


## Running E1.O6

`E1.O6`

### d System.Nullable<System.DayOfWeek>

```
"Friday"
```


## Running E1.O7 2017-10-28 Thursday

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Thursday"
```


## Running E1.O7 2017-10-28

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Friday"
```


## Running E1.O8 -

`E1.O8`

### s System.IO.Stream



## Running E1.O9 -

`E1.O9`

### s System.IO.Stream



## Running E1.O9

`E1.O9`

### s System.IO.Stream

```
null
```


## Running E1.OA -

`E1.OA`

### t System.IO.TextReader



## Running E1.O8 ./test.tmp.txt

`E1.O8`

### s System.IO.Stream



## Running E1.O9 ./test.tmp.txt

`E1.O9`

### s System.IO.Stream



## Running E1.OA ./test.tmp.txt

`E1.OA`

### t System.IO.TextReader



## Running E1.OB ./test.tmp.txt

`E1.OB`

### f System.IO.FileInfo



## Running E1.OC .

`E1.OC`

### d System.IO.DirectoryInfo



## Running E1.OD -

`E1.OD`

### t System.IO.TextWriter



# Do not allow TextWriter to open existing file (by default)

## Running E1.OD ./test.tmp.txt

## Exception System.IO.IOException


# let the named arguments begin

## Running E1.O1 -s hello

`E1.O1`

### s System.String

```
"hello"
```


## Running E1.O7 -d 2017-10-28 Thursday

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Thursday"
```


## Running E1.O7 -d 2017-10-28 -w Thursday

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Thursday"
```


## Running E1.O7 -w Thursday -d 2017-10-28

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Thursday"
```


## Running E1.O7 -w Thursday 2017-10-28

`E1.O7`

### d System.DateTime

```
"2017-10-28T00:00:00"
```

### w System.Nullable<System.DayOfWeek>

```
"Thursday"
```


# let me put param-ish weird characters in my string

## Running E1.OE -- -a -b

`E1.OE`

### a System.String

```
"-a"
```

### b System.String

```
"-b"
```


## Running E1.OE -b bbbbb -- -a

`E1.OE`

### a System.String

```
"-a"
```

### b System.String

```
"bbbbb"
```


## Running E1.OE -a aaaaa -- -b

`E1.OE`

### a System.String

```
"aaaaa"
```

### b System.String

```
"-b"
```


# params methods should be easy to invoke

## Running E1.OF a b c

`E1.OF`

### ss System.String[]

```
[
  "a",
  "b",
  "c"
]
```


## Running E1.OG a b c d

`E1.OG`

### oo System.Collections.Generic.List<System.Object>

```
[
  "a",
  "b",
  "c",
  "d"
]
```


## Running E1.OH 1 2

`E1.OH`

### ii System.Collections.Generic.IEnumerable<System.Int32>

```
[
  1,
  2
]
```


## Running E1.OI s 2018-01-01 2019-01-01

`E1.OI`

### s System.String

```
"s"
```

### d System.Collections.Generic.IList<System.DateTime>

```
[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]
```


## Running E1.OI -s s 2018-01-01 2019-01-01

`E1.OI`

### s System.String

```
"s"
```

### d System.Collections.Generic.IList<System.DateTime>

```
[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]
```


## Running E1.OI s -d 2018-01-01

`E1.OI`

### s System.String

```
"s"
```

### d System.Collections.Generic.IList<System.DateTime>

```
[
  "2018-01-01T00:00:00"
]
```


## Running E1.OI -s s -d 2018-01-01

`E1.OI`

### s System.String

```
"s"
```

### d System.Collections.Generic.IList<System.DateTime>

```
[
  "2018-01-01T00:00:00"
]
```


## Running E1.OI -d 2018-01-01 -s s -d 2019-01-01

`E1.OI`

### s System.String

```
"s"
```

### d System.Collections.Generic.IList<System.DateTime>

```
[
  "2018-01-01T00:00:00",
  "2019-01-01T00:00:00"
]
```


## Running E1.OJ

`E1.OJ`

### ii System.Collections.Generic.IEnumerable<System.Int32>

```
null
```


## Running E1.OJ 1

`E1.OJ`

### ii System.Collections.Generic.IEnumerable<System.Int32>

```
[
  1
]
```


# multiple candidate classes

## Running E2.OJ 1

`E2.OJ`

### ii System.Collections.Generic.IEnumerable<System.Int32>

```
[
  1
]
```


## Running E2.TwoVoid

`E2.TwoVoid`


## Running E1.OK true

`E1.OK`

### b System.Boolean

```
true
```


## Running E1.OL true

`E1.OL`

### b System.Nullable<System.Boolean>

```
true
```


## Running E1.OL null

`E1.OL`

### b System.Nullable<System.Boolean>

```
null
```


## Running E1.OM foo

`E1.OM`

### ct ClassyCLI.Test.RunnerTest+CustomType

```
{
  "Value": "foo"
}
```


## Running E1.OP foo

`E1.OP`

### ct System.Text.RegularExpressions.Regex

```
{
  "Pattern": "foo",
  "Options": "None"
}
```


# ambiguous argument name

## Running E1.OQ -foo 42

stderr:
```
Unknown parameter: foo
E1.OQ                  
  -foo1                Int32
  -foo2                Int32

```

# tasks

## Running E1.ON

`E1.ON`


RanToCompletion


## Running E1.OO

`E1.OO`


RanToCompletion


