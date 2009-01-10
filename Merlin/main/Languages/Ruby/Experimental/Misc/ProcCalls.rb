def foo &p
  puts 'foo.begin'
  p[]
  puts 'foo.end'
end

foo {
  puts 'block'
  break
}