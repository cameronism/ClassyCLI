foo

00 03 foo


foo bar

00 03 foo
04 03 bar


foo bar bop

00 03 foo
04 03 bar
08 03 bop


foo  bar   bop

00 03 foo
05 03 bar
11 03 bop


foo  

00 03 foo


foo 

00 03 foo


foo  bar  

00 03 foo
05 03 bar


foo bar 

00 03 foo
04 03 bar


 foo bar 

01 03 foo
05 03 bar


 foo 

01 03 foo


  foo  bar  

02 03 foo
07 03 bar


  foo  

02 03 foo


"foo bar bop"

01 11 foo bar bop


"foo bar bop" baz

01 11 foo bar bop
14 03 baz


"foo bar bop" baz a

01 11 foo bar bop
14 03 baz
18 01 a


"foo bar bop" "baz" a

01 11 foo bar bop
15 03 baz
20 01 a


"foo bar bop" "baz" "a"

01 11 foo bar bop
15 03 baz
21 01 a


"foo bar bop" " baz" "a"

01 11 foo bar bop
15 04  baz
22 01 a


"foo bar bop" "baz " "a"

01 11 foo bar bop
15 04 baz 
22 01 a


"foo bar bop" "baz" "a b"

01 11 foo bar bop
15 03 baz
21 03 a b


'foo bar bop'

01 11 foo bar bop


'foo bar bop' baz

01 11 foo bar bop
14 03 baz


'foo bar bop' baz a

01 11 foo bar bop
14 03 baz
18 01 a


'foo bar bop' 'baz' a

01 11 foo bar bop
15 03 baz
20 01 a


'foo bar bop' 'baz' 'a'

01 11 foo bar bop
15 03 baz
21 01 a


'foo bar bop' ' baz' 'a'

01 11 foo bar bop
15 04  baz
22 01 a


'foo bar bop' 'baz ' 'a'

01 11 foo bar bop
15 04 baz 
22 01 a


'foo bar bop' 'baz' 'a b'

01 11 foo bar bop
15 03 baz
21 03 a b


""

01 00 


 ""

02 00 


 "" 

02 00 


"" ""

01 00 
04 00 


"" "" ""

01 00 
04 00 
07 00 


''

01 00 


 ''

02 00 


 '' 

02 00 


'' ''

01 00 
04 00 


'' '' ''

01 00 
04 00 
07 00 


' ' " " ' '

01 01  
05 01  
09 01  


' ' " " '"' "'"

01 01  
05 01  
09 01 "
13 01 '


  foo  ' bar '  "  bop  "

02 03 foo
08 05  bar 
17 07   bop  


foo 'bar

00 03 foo
05 03 bar


foo "bar

00 03 foo
05 03 bar


' bar

01 04  bar


" bar

01 04  bar


' bar 

01 05  bar 


" bar 

01 05  bar 


