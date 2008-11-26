class Exception
  remove_method "backtrace"
end

puts 'Hello'

begin
  raise   # crash
rescue
  p $@
end

puts 'Bye'