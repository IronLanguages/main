def foo &p
  yield 1,2,3,&p
end

foo { |a,b,c|
  puts a,b,c
}