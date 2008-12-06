$var = 123

alias $true_bang $!
alias $! $var

begin
  raise IOError.new
rescue
  puts 'rescue'  
  p $true_bang
  p $!
ensure
  # oldEx = GetCurrentException - nil here
  puts 'ensure'
  p $true_bang
  p $!

  $true_bang = Exception.new "1"
  $! = Exception.new "2"

  # SetCurrentException(oldEx) - restores nil
end

puts 'end'
p $true_bang              # restored by ensure
p $!                      # not restored by ensure