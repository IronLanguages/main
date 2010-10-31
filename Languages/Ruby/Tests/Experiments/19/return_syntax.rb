begin
  eval('a[return 1, &x]')
rescue SyntaxError
  p $!
end

begin
  eval('puts if return 1')
rescue SyntaxError
  p $!
end

begin
  eval('1 && return 1')
rescue SyntaxError
  p $!
end

begin
  eval('(a ? return 1 : break) while true')
rescue SyntaxError
  p $!
end

begin
  eval('return.foo = bar')
rescue SyntaxError
  p $!
end

begin
  eval('a = 1 rescue return 2')
rescue SyntaxError
  p $!
end

def foo a
  (a ? return : break) while true
  puts 'foo'
end

def foo
  false || return
  puts 'unreachable'
end

def foo
  false rescue return
end

def foo
  false rescue return 2
end

def foo
  a = 1 rescue return 
end

def foo
  return 1
end

def foo
  if cond then retry end
end