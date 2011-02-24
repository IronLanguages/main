def foo
  break
rescue
  puts "E: #{$!}"
end

foo
foo { }