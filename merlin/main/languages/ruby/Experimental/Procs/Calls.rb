lambda { |x,y| p [x, y] }.call rescue puts 'error'
lambda { |x,y| p [x, y] }.call 1 rescue puts 'error'
lambda { |x,y| p [x, y] }.call 1,2
lambda { |x,y| p [x, y] }.call [] rescue puts 'error'
lambda { |x,y| p [x, y] }.call [1] rescue puts 'error'
lambda { |x,y| p [x, y] }.call [1,2] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[1,2] 
lambda { |x,y| p [x, y] }.call *[[1]] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[[1,2]] rescue puts 'error'
lambda { |x,y| p [x, y] }.call *[[1,2,3]] rescue puts 'error'

puts '---'

Proc.new { |x,y| p [x, y] }.call rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call 1 rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call 1,2
Proc.new { |x,y| p [x, y] }.call [] rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call [1] rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call [1,2] rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call *[1,2] 
Proc.new { |x,y| p [x, y] }.call *[[1]] rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call *[[1,2]] rescue puts 'error'
Proc.new { |x,y| p [x, y] }.call *[[1,2,3]] rescue puts 'error'

puts '---'

def a; yield; end 
def b; yield 1; end 
def c; yield 1,2; end 
def d; yield []; end 
def e; yield [1]; end 
def f; yield [1,2]; end 

a { |x,y| p [x, y] }
b { |x,y| p [x, y] }
c { |x,y| p [x, y] }
d { |x,y| p [x, y] }
e { |x,y| p [x, y] }
f { |x,y| p [x, y] }

puts '---'

Proc.new { |x| puts x.inspect }.call
Proc.new { |x| puts x.inspect }.call 1
Proc.new { |x| puts x.inspect }.call 1,2
Proc.new { |x| puts x.inspect }.call []
Proc.new { |x| puts x.inspect }.call [1]
Proc.new { |x| puts x.inspect }.call [1,2]
Proc.new { |x| puts x.inspect }.call *[1]
Proc.new { |(x,)| puts x.inspect }.call *[]

puts '---'

lambda { |x| puts x.inspect }.call
lambda { |x| puts x.inspect }.call 1
lambda { |x| puts x.inspect }.call 1,2
lambda { |x| puts x.inspect }.call []
lambda { |x| puts x.inspect }.call [1]
lambda { |x| puts x.inspect }.call [1,2]
lambda { |x| puts x.inspect }.call *[1]
lambda { |(x,)| puts x.inspect }.call *[] rescue puts 'error'