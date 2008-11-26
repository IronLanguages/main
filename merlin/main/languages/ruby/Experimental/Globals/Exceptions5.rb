# rescue ignores $! aliases

class E < Exception
end

$goo = 123
alias $! $goo
  
def foo
  puts 'compared with foo'
  IOError
end

def bar
  puts 'compared with bar'
  E
end

begin
  raise E.new  
rescue foo
  puts 'rescued foo'
rescue bar                 
  puts 'rescued bar'
rescue
  puts 'rescued default'
end