puts '-1-'
p "BB=====BB".unpack('ma*')
p "B====B=====BB".unpack('ma*')
p "BBBB".unpack('ma*')
p "B=B=c=d=e=f=g=".unpack('ma*')
p "B=B=".unpack('ma*')
p "B=".unpack('ma*')
p "B==".unpack('ma*')
p "B===".unpack('ma*')

puts '-2-'
p "$$$".unpack('ma*')
p "$$$=$$$".unpack('ma*')
p "$$$B$$$=$$$".unpack('ma*')
p "$$$B$$$B$$$=$$$".unpack('ma*')
p "$$$B$$$$B$$$$B$$$=$$$".unpack('ma*')
p "$$$B$$$B$$$B$$$B$$$=$$$".unpack('ma*')

puts '-3-'
p "".unpack('ma*')
p "=".unpack('ma*')
p "B=".unpack('ma*')
p "B$B$B=".unpack('ma*')
p "BBBB=".unpack('ma*')
p "BB=".unpack('ma*')

puts '-4-'
p "Z".unpack('ma*')
p "ZZ".unpack('ma*')
p "ZZZ".unpack('ma*')
p "ZZZZ".unpack('ma*')
p "ZZZZZ".unpack('ma*')
p "ZZZZZZ".unpack('ma*')
p "ZZZZZZZ".unpack('ma*')

puts '-5-'
p "Z=".unpack('ma*')
p "ZZ=".unpack('ma*')
p "ZZZ=".unpack('ma*')
p "ZZZZ=".unpack('ma*')
p "ZZZZZ=".unpack('ma*')
p "ZZZZZZ=".unpack('ma*')
p "ZZZZZZZ=".unpack('ma*')