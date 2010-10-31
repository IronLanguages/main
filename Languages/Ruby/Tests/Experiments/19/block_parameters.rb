f = lambda do |a,b=1,*c,&d|
  p a,b,c,d
end

f.(1,2,3) {}

f = lambda do |a,b=1,c,&d|
  p a,b,c,d
end

f.(1,2,3) {}

f = lambda do |a,b,c,*d|
  p a,b,c,d
end

f.(1,2,3,4,5) {}

f = lambda do |a,b,c,*d,&e|
  p a,b,c,d,e
end

f.(1,2,3,4,5) {}