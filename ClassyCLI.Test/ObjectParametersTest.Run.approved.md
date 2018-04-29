## Running my.exe a.b -foo 1 -bar 2

MethodInvoked

ClassyCLI.Test.ObjectParametersTest+BC

```
{
  "Foo": 1,
  "Bar": "2"
}
```

## Running my.exe a.b -foo foo -bar 2

ArgumentConversionFailed



```
null
```

stderr:
```
Fail to parse parameter: Foo
A.B                    
  -Foo                 Int32
  -Bar                 String

```

## Running my.exe a.b --help

Help



```
null
```

stdout:
```
A.B                    
  -Foo                 Int32
  -Bar                 String

```

## Running my.exe a.c --help

Help



```
null
```

stdout:
```
A.C                    
  -Foo                 This is foo
  -Bar                 This is bar

```

## Running my.exe a.d

ArgumentValidationFailed



```
null
```

stderr:
```
The Foo field is required.
The Bar field is required.
```

## Running my.exe a.d -foo 42 -bar 

ArgumentValidationFailed



```
null
```

stderr:
```
The Bar field is required.
```

## Running my.exe a.d -foo 21 -bar bop

MethodInvoked

ClassyCLI.Test.ObjectParametersTest+DC

```
{
  "Foo": 21,
  "Bar": "bop"
}
```

