## Unambiguous types: C1 vs C2

```
c2.
   ^
```

- C2.M1
- C2.M2


```
c2
  ^
```

- C2.M1
- C2.M2


## Ambiguous type names: C2 vs C20

```
c2.
   ^
```

- C2.M1
- C2.M2


```
c2
  ^
```

- C2.
- C20.


## Nested class at same level as methods

```
c4.
   ^
```

- C4.M1
- C4.C5.


## Method completions

```
c2.m
    ^
```

- C2.M1
- C2.M2


```
c2.m1
     ^
```

- C2.M1


```
C1.
   ^
```

- C1.M1


```
C1.m
    ^
```

- C1.M1


```
C1.M
    ^
```

- C1.M1


```
C1.m1
     ^
```

- C1.M1


```
C2.
   ^
```

- C2.M1
- C2.M2


```
C2.m1
     ^
```

- C2.M1


```
C2.x
    ^
```

_no completions_


```
C
 ^
```

- C1.
- C2.


```
c1
  ^
```

- C1.M1


```
c1.
   ^
```

- C1.M1


## Should return no completions

```
xx
  ^
```

_no completions_


```
xx.
   ^
```

_no completions_


```
xx.yy
     ^
```

_no completions_


## Parameter name completion

```
C3.M1 -
       ^
```

- -foo
- -bar


```
C3.M1 -f
        ^
```

- -foo


```
C3.M1 -foo
          ^
```

- -foo


```
C3.M1 -FOO
          ^
```

- -foo


```
C3.M1 -b
        ^
```

- -bar


```
C3.M1 -bar
          ^
```

- -bar


```
C3.M1 -BaR
          ^
```

- -bar


## Track which parameters have already been used

```
C3.M1 -foo 1 -b
               ^
```

- -bar


```
C3.M1 -foo 1 -
              ^
```

- -bar


```
C3.M1 -bar 1 -
              ^
```

- -foo


## Don't try to complete numbers

```
C3.M1 1
       ^
```

_no completions_


```
C3.M1 1 2
         ^
```

_no completions_


```
C3.M1 11
        ^
```

_no completions_


```
C3.M1 11 22
           ^
```

_no completions_


## No parameter names after `--`

```
C3.M1 -- 
         ^
```

_no completions_


```
C3.M1 -- -
          ^
```

_no completions_


```
C3.M1 -- -f
           ^
```

_no completions_


```
C3.M1 1 -- 
           ^
```

_no completions_


```
C3.M1 1 -- -
            ^
```

_no completions_


```
C3.M1 1 -- -f
             ^
```

_no completions_


```
C3.M1 11 -- 
            ^
```

_no completions_


```
C3.M1 11 -- -
             ^
```

_no completions_


```
C3.M1 11 -- -f
              ^
```

_no completions_


```
C3.M1 -bar 1 -- 
                ^
```

_no completions_


```
C3.M1 -bar 1 -- -
                 ^
```

_no completions_


```
C3.M1 -bar 1 -- -f
                  ^
```

_no completions_


```
C3.M1 -bar 11 -- 
                 ^
```

_no completions_


```
C3.M1 -bar 11 -- -
                  ^
```

_no completions_


```
C3.M1 -bar 11 -- -f
                   ^
```

_no completions_


## Value completion

```
C3.M2 -- 
         ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


```
C3.M2 S
       ^
```

- Sunday
- Saturday


```
C3.M2 s
       ^
```

- Sunday
- Saturday


```
C3.M3 -- 
         ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


```
C3.M3 S
       ^
```

- Sunday
- Saturday


```
C3.M3 s
       ^
```

- Sunday
- Saturday


```
C3.M3 -d 
         ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


```
C3.M3 -d S
          ^
```

- Sunday
- Saturday


```
C3.M3 -d s
          ^
```

- Sunday
- Saturday


```
C3.M2 
      ^
```

- -d


```
C3.M3 
      ^
```

- -d


## Mid word completion

```
C3.M3 -d szz
         ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


```
C3.M3 -d szz
          ^
```

- Sunday
- Saturday


```
C3.M3 -d szz
           ^
```

_no completions_


## Bool completion

```
C3.M4 -b t
          ^
```

- true


```
C3.M4 -b 
         ^
```

- true
- false


## Null completion

```
C3.M5 -b n
          ^
```

- null


```
C3.M5 -b 
         ^
```

- true
- false
- null


## Params(-ish) parameters can be used multiple times

```
C3.M6 -d 
         ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


```
C3.M6 -d Sunday -d 
                   ^
```

- Sunday
- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday


## Handle similarly named parameters

```
C3.M7 -
       ^
```

- -foo1
- -foo2


```
C3.M7 -f
        ^
```

- -foo1
- -foo2


```
C3.M7 -foo
          ^
```

- -foo1
- -foo2


```
C3.M7 -foo1
           ^
```

- -foo1


