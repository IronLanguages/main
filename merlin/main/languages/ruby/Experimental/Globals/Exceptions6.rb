$foo = 123

p $! = IOError.new

# hides real $!
alias $! $foo

p $!  #123

begin
  raise $! rescue puts 'Cast error'
  raise                                          # uses real $!, not alias
rescue IOError => e
  p e.class   # fixnum (uses alias)
  p $!.class  # fixnum (uses alias)
  
  p $@        # real stack trace (doesn't use alias)  
end


