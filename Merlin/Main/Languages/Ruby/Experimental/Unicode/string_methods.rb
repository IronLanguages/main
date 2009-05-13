def get_str(e, str)
  $KCODE = e
  return eval(str)
ensure
  $KCODE = 'BINARY'  
end

b = get_str('BINARY', "'\xe2\x85\x9c'")
s = get_str('SJIS', "'\202\240'")
u = get_str('UTF8', "'\xe2\x85\x9c'")

$KCODE = 'BINARY'

x = [b,s,u]

x.each do |v1|
  x.each do |v2| 
    puts((v1 + v2).dump)
  end
end
