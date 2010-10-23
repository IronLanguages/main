def foo
  yield
  raise
end

def goo(a = foo { puts 'zoo' })
  puts a
rescue
  puts 'rescued in goo'
ensure
  puts 'ensure in goo'
end

begin
  goo
rescue 
  puts 'rescued out'
  puts $!.backtrace
end

