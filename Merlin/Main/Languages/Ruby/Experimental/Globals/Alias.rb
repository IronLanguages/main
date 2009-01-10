vars = [

#regular:
'foo',

#special:
'_',
'!',
'@',
'~',
',',
';',
'/',
'\\',
'*',
'$',
'?',
'=',
':',
'"',
'<',
'>',
'.',
'0',

#backrefs:
'&',
'+',
'`',
"'",
'+',
'~',

#nrefs:
'1',

]

puts "Failed:"

vars.each { |v|
  vars.each { |w|    
      a = "alias $#{v} $#{w}"
      begin
        eval(a) 
      rescue SyntaxError
        puts a
      end  
  }
}
