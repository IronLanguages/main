require 'rubygems' unless defined? IRONRUBY_VERSION
require 'parse_tree' 

pt = ParseTree.new false

def foo
end

puts '- 0 -'

x0 = pt.parse_tree_for_string <<END 
END

x1 = pt.parse_tree_for_string <<END 
  puts 1
END

x2 = pt.parse_tree_for_string <<END 
  puts 1
  puts 1
END

p x0,x1,x2

puts '- 1 -'

p pt.parse_tree_for_string("0.foo(1,2,3)")
p pt.parse_tree_for_string("0.foo(1,2,3, 5 => 6)")
p pt.parse_tree_for_string("0.foo(1,2,3, *4)")
p pt.parse_tree_for_string("0.foo(1,2,3, 5 => 6, *4)")
p pt.parse_tree_for_string("0.foo(1,2,3, 5 => 6, *4) {}")
p pt.parse_tree_for_string("0.foo(1=>2,3=>4,5=>6)")

puts '- 2 -'

p pt.parse_tree_for_string("super")
p pt.parse_tree_for_string("super { }")
p pt.parse_tree_for_string("super()")
p pt.parse_tree_for_string("super() {}")  # bug?
p pt.parse_tree_for_string("super(1)")
p pt.parse_tree_for_string("super(1) { }")

puts '- 3 -'

p pt.parse_tree_for_string("yield")
p pt.parse_tree_for_string("yield()")
p pt.parse_tree_for_string("yield(1)")   # bug?
p pt.parse_tree_for_string("yield(*1)")
p pt.parse_tree_for_string("yield(1,*2)")
p pt.parse_tree_for_string("yield(2=>3)")
p pt.parse_tree_for_string("yield(2=>3,5=>6, *4)")
p pt.parse_tree_for_string("yield(1,2=>3,*4)")

puts '- 4 -'

p pt.parse_tree_for_string("foo")
p pt.parse_tree_for_string("foo()")
p pt.parse_tree_for_string("foo(1)")
p pt.parse_tree_for_string("foo(*1)")
p pt.parse_tree_for_string("foo(0,*1)")

p pt.parse_tree_for_string("x.foo")
p pt.parse_tree_for_string("x.foo()")
p pt.parse_tree_for_string("x.foo(1)")

puts '- 5 -'

p pt.parse_tree_for_string("foo(1,&2)")

puts '- 6 -'

p pt.parse_tree_for_string("x = 1; x")
p pt.parse_tree_for_string("@x = 1; @x")
p pt.parse_tree_for_string("@@x = 1; @@x")
p pt.parse_tree_for_string("$x = 1; $x")
p pt.parse_tree_for_string("C = 1; C")

puts '- 7 -'

p pt.parse_tree_for_string("b[1]")
p pt.parse_tree_for_string("a = b[1]")

p pt.parse_tree_for_string("x[0] = 1")
p pt.parse_tree_for_string("x[*1] = 2")
p pt.parse_tree_for_string("x[0,*1] = 2")

p pt.parse_tree_for_string("x[y[0] = 1] = 2")
p pt.parse_tree_for_string("x[*y[2] = 3] = 4")
p pt.parse_tree_for_string("x[y[0] = 1, *y[2] = 3] = 4")

puts '- 8 -'

p pt.parse_tree_for_string("Q::y = 1")
p pt.parse_tree_for_string("(Q = 0)::y = 1")
p pt.parse_tree_for_string("(Q::z = 0)::y = 1")

puts '- 9 -'
	
puts '> a'
p pt.parse_tree_for_string("foo() { || }")
p pt.parse_tree_for_string("foo() { |x| }")
p pt.parse_tree_for_string("foo() { |*| }")
p pt.parse_tree_for_string("foo() { |*x| }")
p pt.parse_tree_for_string("foo() { |x,*| }")

p pt.parse_tree_for_string("foo() { |x,| }")
p pt.parse_tree_for_string("foo() { |(x,)| }")

puts '> b'
p pt.parse_tree_for_string("foo() { |x,y| }")
p pt.parse_tree_for_string("foo() { |(x,y,)| }")

puts '> c'
p pt.parse_tree_for_string("foo() { |(x,y),| }")
p pt.parse_tree_for_string("foo() { |(x,y,),| }")

puts '> d'
p pt.parse_tree_for_string("foo() { |(x,),| }")

puts '> e'
p pt.parse_tree_for_string("foo() { |x,*| }")
p pt.parse_tree_for_string("foo() { |x,*y| }")

puts '- 10 -'
	
puts '> a'
p pt.parse_tree_for_string("* = 1")
p pt.parse_tree_for_string("*x = 1")
p pt.parse_tree_for_string("x,* = 1")

p pt.parse_tree_for_string("x, = 1")
p pt.parse_tree_for_string("(x,) = 1")

puts '> b'
p pt.parse_tree_for_string("x,y = 1")
p pt.parse_tree_for_string("(x,y,) = 1")

puts '> c'
p pt.parse_tree_for_string("(x,y), = 1")
p pt.parse_tree_for_string("(x,y,), = 1")

puts '> d'
p pt.parse_tree_for_string("(x,), = 1")

puts '> e'
p pt.parse_tree_for_string("x,* = 1")
p pt.parse_tree_for_string("x,*y = 1")


puts '- 11 -'
p pt.parse_tree_for_string("a = 1; (x[b = a], x[b,b = a,a]) = 2")
p pt.parse_tree_for_string("Q::x,y[0],z[*0] = 1")
p pt.parse_tree_for_string("foo() { |Q::x,y[0],z[*0]| }")

puts '- 12 -'
	
p pt.parse_tree_for_string("a = *x")
p pt.parse_tree_for_string("a = x,y")
p pt.parse_tree_for_string("a = x,*y")

puts '- 13 -'
	
puts '> a'

p pt.parse_tree_for_string("* = x")
p pt.parse_tree_for_string("* = *x")
p pt.parse_tree_for_string("* = x,*y")
p pt.parse_tree_for_string("* = x,y")

puts '> b'

p pt.parse_tree_for_string("*a = x")
p pt.parse_tree_for_string("*a = *x")
p pt.parse_tree_for_string("*a = x,*y")
p pt.parse_tree_for_string("*a = x,y")

puts '> c'
p pt.parse_tree_for_string("a,b = x")
p pt.parse_tree_for_string("a,*b = x")
p pt.parse_tree_for_string("a,b,c = x")
p pt.parse_tree_for_string("a,b,*c = x")

puts '> d'
p pt.parse_tree_for_string("a,b = *x")

puts '> e'
p pt.parse_tree_for_string("a,b = x,y")
p pt.parse_tree_for_string("a,b = x,*y")
p pt.parse_tree_for_string("a,b = x,y,z")
p pt.parse_tree_for_string("a,b = x,y,*z")

puts '- 14 -'
p pt.parse_tree_for_string("x &&= y")
p pt.parse_tree_for_string("x ||= y")
p pt.parse_tree_for_string("x += y")

puts '- 15 -'
p pt.parse_tree_for_string("break")
p pt.parse_tree_for_string("break 1")
p pt.parse_tree_for_string("break 1=>2")
p pt.parse_tree_for_string("break 1,2,3")
p pt.parse_tree_for_string("break 1,2,3, 4=>5, *6")

puts '- 16 -'
p pt.parse_tree_for_string("next")
p pt.parse_tree_for_string("next 1")
p pt.parse_tree_for_string("next 1=>2")
p pt.parse_tree_for_string("next 1,2,3")
p pt.parse_tree_for_string("next 1,2,3, 4=>5, *6")

puts '- 17 -'
p pt.parse_tree_for_string("return")
p pt.parse_tree_for_string("return 1")
p pt.parse_tree_for_string("return 1=>2")
p pt.parse_tree_for_string("return 1,2,3")
p pt.parse_tree_for_string("return 1,2,3, 4=>5, *6")

puts '- 18 -'
p pt.parse_tree_for_string("retry")
p pt.parse_tree_for_string("redo")

puts '- 19 -'
p pt.parse_tree_for_string("not x")
p pt.parse_tree_for_string("1 and 2")
p pt.parse_tree_for_string("1 or 2")

p pt.parse_tree_for_string("1 && 2")
p pt.parse_tree_for_string("1 || 2")


puts '- 20 -'
p pt.parse_tree_for_string("self")

p pt.parse_tree_for_string("alias foo bar")
p pt.parse_tree_for_string("alias :foo :bar")
p pt.parse_tree_for_string("alias $x $y")


p pt.parse_tree_for_string("undef :a1")
p pt.parse_tree_for_string("undef :a1, :a2")
p pt.parse_tree_for_string("undef :a1, :a2, :a3")

puts '- 21 -'

puts '> a'
p pt.parse_tree_for_string("nil")
p pt.parse_tree_for_string("true")
p pt.parse_tree_for_string("false")
p pt.parse_tree_for_string("1.0")
p pt.parse_tree_for_string(":'foo'")
p pt.parse_tree_for_string('`x`')
p pt.parse_tree_for_string("'x'")

puts '> b'
p pt.parse_tree_for_string("1..2")
p pt.parse_tree_for_string("1...2")
p pt.parse_tree_for_string("10000000000000000000000000000000..2000000000000000000000000000000")
p pt.parse_tree_for_string("1.0..2.3")
p pt.parse_tree_for_string("false..nil")
p pt.parse_tree_for_string("'x'..'y'")
p pt.parse_tree_for_string("`x`..`y`")

puts '> c'
p pt.parse_tree_for_string("1..:b")
p pt.parse_tree_for_string("1..foo")
p pt.parse_tree_for_string("1..{}")
p pt.parse_tree_for_string("1...{}")

puts '> d'
p pt.parse_tree_for_string("{}")
p pt.parse_tree_for_string("{1,2}")
p pt.parse_tree_for_string("{1=>2}")
p pt.parse_tree_for_string("[]")
p pt.parse_tree_for_string("[1,2]")
p pt.parse_tree_for_string("[1=>2]")

puts '- 22 -'

puts '> a'
p pt.parse_tree_for_string('"#{$x}"')
p pt.parse_tree_for_string('"a#{$x}"')
p pt.parse_tree_for_string('"a#{$x}b"')
p pt.parse_tree_for_string('"a#{$x}b#{$y}"')

puts '> b'
p pt.parse_tree_for_string('"#{}"')
p pt.parse_tree_for_string('"#{x}"')
p pt.parse_tree_for_string('"#{x;y}"')
p pt.parse_tree_for_string('"#{x;y;z}"')

puts '- 23 -'

p pt.parse_tree_for_string(':"#{$x}"')
p pt.parse_tree_for_string(':"a#{$x}"')
p pt.parse_tree_for_string(':"a#{$x}b"')
p pt.parse_tree_for_string(':"a#{$x}b#{$y}"')

puts '- 24 -'

p pt.parse_tree_for_string('`#{$x}`')
p pt.parse_tree_for_string('`a#{$x}`')
p pt.parse_tree_for_string('`a#{$x}b`')
p pt.parse_tree_for_string('`a#{$x}b#{$y}`')

puts '- 25 -'

p pt.parse_tree_for_string('/#{$x}/i')
p pt.parse_tree_for_string('/a#{$x}/i')
p pt.parse_tree_for_string('/a#{$x}b/')
p pt.parse_tree_for_string('/a#{$x}b#{$y}/')

p pt.parse_tree_for_string('//mixsnue')
p pt.parse_tree_for_string('/a#{"#{$x}"}b#{`#{$y}`}/')


puts '- 24 -'

p pt.parse_tree_for_string('$&');
p pt.parse_tree_for_string('$~');
p pt.parse_tree_for_string('$+');
p pt.parse_tree_for_string('$`');
p pt.parse_tree_for_string('$\'');
p pt.parse_tree_for_string('$0');
p pt.parse_tree_for_string('$1');
p pt.parse_tree_for_string('$2');

puts '- 25 -'

p pt.parse_tree_for_string("defined? C")
p pt.parse_tree_for_string("defined? foo")
p pt.parse_tree_for_string("defined? @x")
p pt.parse_tree_for_string("defined? @@x")
p pt.parse_tree_for_string("defined? $x")
p pt.parse_tree_for_string("defined? 1")
p pt.parse_tree_for_string("defined? 1+1")

puts '- 26 -'

p pt.parse_tree_for_string("()")
p pt.parse_tree_for_string("(a)")
p pt.parse_tree_for_string("(a;b)")

puts '- 27 -'

p pt.parse_tree_for_string("begin; end")
p pt.parse_tree_for_string("begin; a; end")
p pt.parse_tree_for_string("begin; a; b; end")

puts '- 28 -'

p pt.parse_tree_for_string("begin; ensure; end")
p pt.parse_tree_for_string("begin; else; end")
p pt.parse_tree_for_string("begin; rescue; end")
p pt.parse_tree_for_string("begin; rescue; else; end")
p pt.parse_tree_for_string("begin; rescue; else; ensure; end")
p pt.parse_tree_for_string("begin; else; ensure; end")

puts '- 29 -'

p pt.parse_tree_for_string("f(begin; a; b; end)")

puts '- 30 -'

p pt.parse_tree_for_string("x = ()")
p pt.parse_tree_for_string("x = (a)")
p pt.parse_tree_for_string("x = (a;b)")

puts '- 31 -'

p pt.parse_tree_for_string("x = begin; end")
p pt.parse_tree_for_string("x = begin; a; end")
p pt.parse_tree_for_string("x = begin; a; b; end")

puts '- 32 -'

p pt.parse_tree_for_string("x = begin; ensure; end")
p pt.parse_tree_for_string("x = begin; else; end")
p pt.parse_tree_for_string("x = begin; rescue; end")
p pt.parse_tree_for_string("x = begin; rescue; else; end")
p pt.parse_tree_for_string("x = begin; rescue; else; ensure; end")
p pt.parse_tree_for_string("x = begin; else; ensure; end")

puts '- 33 -'

x = pt.parse_tree_for_string <<END
  begin
    1
  rescue
    2
  rescue A => e
    3
  rescue A,B => e
    4
  rescue A
    5
  rescue A,B
    6
  else
    7
  ensure
    8
  end
END

p x

x = pt.parse_tree_for_string <<END
  begin
    1  
  end
END

p x

x = pt.parse_tree_for_string <<END
  begin
    1 
  else
    2 
  end
END

p x

x = pt.parse_tree_for_string <<END
  begin
    1 
  ensure
    2 
  end
END

p x

puts '- 34 -'
p pt.parse_tree_for_string("x = 1 rescue return")
p pt.parse_tree_for_string("x = 1 rescue 1")
p pt.parse_tree_for_string("1 rescue return")
p pt.parse_tree_for_string("1 rescue 1")
 
puts '- 35 -'

p pt.parse_tree_for_string("if c; end")
p pt.parse_tree_for_string("if c then; end")
p pt.parse_tree_for_string("if c; x; end")
p pt.parse_tree_for_string("if c; x; y; end")
p pt.parse_tree_for_string("if c; x; else; y; end")
p pt.parse_tree_for_string("if c1; x; elsif c2; y; end")
p pt.parse_tree_for_string("if c1; x; elsif c2; y; elsif c3; z; end")
p pt.parse_tree_for_string("if c1; x; elsif c2; y; elsif c3; z; else; u; end")

puts '- 36 -'

p pt.parse_tree_for_string("unless c; end")
p pt.parse_tree_for_string("unless c then; end")
p pt.parse_tree_for_string("unless c; x; end")
p pt.parse_tree_for_string("unless c; x; y; end")
p pt.parse_tree_for_string("unless c; x; else; y; end")

puts '- 37 -'

p pt.parse_tree_for_string("c ? 1 : 2")

p pt.parse_tree_for_string("c || return")
p pt.parse_tree_for_string("c && return")

p pt.parse_tree_for_string("c ? return : 1")
p pt.parse_tree_for_string("c ? 1 : return")

p pt.parse_tree_for_string("1 if c")
p pt.parse_tree_for_string("1 unless c")
p pt.parse_tree_for_string("c ? next : redo")

puts '- 38 -'

puts '> a'
p pt.parse_tree_for_string("x while c")
p pt.parse_tree_for_string("x until c")

puts '> b'
p pt.parse_tree_for_string("(x;y) while c")

puts '> c'
p pt.parse_tree_for_string("begin; end while c")
p pt.parse_tree_for_string("begin; x; end while c")
p pt.parse_tree_for_string("begin; x;y; end while c")

puts '> d'
p pt.parse_tree_for_string("while c do end")
p pt.parse_tree_for_string("while c do x; end")
p pt.parse_tree_for_string("while c do x;y; end")

puts '> e'
p pt.parse_tree_for_string("until c do end")
p pt.parse_tree_for_string("until c do x; end")
p pt.parse_tree_for_string("until c do x;y; end")

puts '- 39 -'

p pt.parse_tree_for_string("for x in a do; end")
p pt.parse_tree_for_string("for x in a do; s1; end")
p pt.parse_tree_for_string("for x in a do; s1; s2; end")

p pt.parse_tree_for_string("for * in a do; end")
p pt.parse_tree_for_string("for *x in a do; end")
p pt.parse_tree_for_string("for x,*y in a do; end")
p pt.parse_tree_for_string("for x,y in a do; s1; s2; end")

puts '- 40 -'

p pt.parse_tree_for_string("
case v 
	when ca1,ca2: a
    when cb: b
else
    e
end
")

p pt.parse_tree_for_string("
case v 
    when ca:
end
")

p pt.parse_tree_for_string("
case v 
    when ca: a
end
")

p pt.parse_tree_for_string("
case v 
    when ca: a;b
else
end
")

p pt.parse_tree_for_string("
case v 
    when ca:
else
    e1
    e2
end
")

p pt.parse_tree_for_string("
case
    when *x: a
end
")

puts '- 41 -'

p pt.parse_tree_for_string("::A")
p pt.parse_tree_for_string("::A::B")
p pt.parse_tree_for_string("A::B")
p pt.parse_tree_for_string("A::foo()::B::C")

puts '- 42 -'

if defined? IRONRUBY_VERSION
  p pt.parse_tree_for_string("A::foo()::B::C = 1")
  p pt.parse_tree_for_string("(::A,x) = 1,2")
  p pt.parse_tree_for_string("::A = 1")
  p pt.parse_tree_for_string("A::B = 1")
else
  puts 'SEGFAULT'
  puts 'SEGFAULT'
  puts 'SEGFAULT'
  puts 'SEGFAULT'
end

puts '- 43 -'

p pt.parse_tree_for_string("module M;end")
p pt.parse_tree_for_string("module M;x;end")
p pt.parse_tree_for_string("module M;x;y;end")
p pt.parse_tree_for_string("module A::B::C;end")

p pt.parse_tree_for_string("class C;end")
p pt.parse_tree_for_string("class C < D;end")
p pt.parse_tree_for_string("class << x;end")
p pt.parse_tree_for_string("module M; a; rescue; b; end")

puts '- 44 -'

p pt.parse_tree_for_string("def f; end")
p pt.parse_tree_for_string("def s.f; end")
p pt.parse_tree_for_string("def s.f; 1; end")
p pt.parse_tree_for_string("def C::f; 1; end")
p pt.parse_tree_for_string("def f; a; end")
p pt.parse_tree_for_string("def f; a; rescue; b; end")

p pt.parse_tree_for_string("def f(); 1; end")
p pt.parse_tree_for_string("def f(a); 1; end")
p pt.parse_tree_for_string("def f(a,b); 1; end")
p pt.parse_tree_for_string("def f(a,*b); 1; end")
p pt.parse_tree_for_string("def f(a,*b,&p); 1; end")
p pt.parse_tree_for_string("def f(x,y=2,z=3,*a,&b); 1; end")

puts '- 45 -'

puts '> a'
p pt.parse_tree_for_string("1 if 1 .. 1")
p pt.parse_tree_for_string("x if f(x .. x)")

puts '> b'
p pt.parse_tree_for_string("1 if 1 .. $x")
p pt.parse_tree_for_string("1 if 1 .. C")
p pt.parse_tree_for_string("1 if 1 .. x")
p pt.parse_tree_for_string("x if 1 .. 100000000000000000000000000")
p pt.parse_tree_for_string("x if 1 .. /xx/")
p pt.parse_tree_for_string("x if 1 .. {}")
p pt.parse_tree_for_string("x if 1 .. []")
p pt.parse_tree_for_string("x if 1 .. 'y'")
p pt.parse_tree_for_string("x if 1 .. :y")
p pt.parse_tree_for_string("x if 1 .. 1.2")
p pt.parse_tree_for_string("x if 1 .. true")
p pt.parse_tree_for_string("x if 1 .. false")
p pt.parse_tree_for_string("x if 1 .. nil")

puts '> c'
p pt.parse_tree_for_string("x unless a ... b")
p pt.parse_tree_for_string("x while a ... b")
p pt.parse_tree_for_string("x until a ... b")
p pt.parse_tree_for_string("if a ... b; end")
p pt.parse_tree_for_string("unless a ... b; end")
p pt.parse_tree_for_string("while a ... b; end")
p pt.parse_tree_for_string("until a ... b; end")
p pt.parse_tree_for_string("a ... b ? 1 : 2")

puts '> d'
p pt.parse_tree_for_string("(1..2) .. b ? 1 : 2")
p pt.parse_tree_for_string("if ((x..y) ... b); end")
p pt.parse_tree_for_string("if ((1..2) ... b); end")
p pt.parse_tree_for_string("if begin a ... b end; end")
p pt.parse_tree_for_string("(0;1;TRUE..FALSE) ? 1 : 2")
p pt.parse_tree_for_string("begin 0;1;TRUE..FALSE end ? 1 : 2")

puts '> e'
p pt.parse_tree_for_string("if (1;2;begin a ... b end); end")
p pt.parse_tree_for_string("if begin a ... b; rescue; end; end")
p pt.parse_tree_for_string("(1;begin 0;1;TRUE..FALSE end) ? 1 : 2")
p pt.parse_tree_for_string("(1;begin 0;1;TRUE..FALSE; rescue; end) ? 1 : 2")
p pt.parse_tree_for_string("if begin a ... b; rescue; end; end")
p pt.parse_tree_for_string("if (1;2;begin a ... b end); end")

puts '- 46 -'

p pt.parse_tree_for_string("/foo/.=~('foo')")
p pt.parse_tree_for_string('/foo/ =~ "foo"')
p pt.parse_tree_for_string('/fo#{x}o/ =~ "foo"')
p pt.parse_tree_for_string('/foo/ !~ "foo"')
p pt.parse_tree_for_string('/fo#{x}o/ !~ "foo"')
p pt.parse_tree_for_string("1 if /foo/")
p pt.parse_tree_for_string('1 if /fo#{x}o/')

puts '- 47 -'

p pt.parse_tree_for_string("1 if /a/ and /b/")
p pt.parse_tree_for_string("1 if /a/ or /b/")
p pt.parse_tree_for_string("1 if a..b and c..d or e..f")
p pt.parse_tree_for_string("1 if not (a..b)")

puts '- 48 -'

class C
  def foo
    1
  end
  
  def self.goo
    2
  end
  
  class << self
    alias agoo goo
  end
  
  alias afoo foo
end

p pt.parse_tree_for_meth(C, "afoo", false)
p pt.parse_tree_for_meth(C, "agoo", true)

puts '- 49 -'

class C
  def f
  end

  define_method(:b) { }
  
  alias af f
  alias ab b
  
  define_method(:db, instance_method(:b)) 
  define_method(:ddb, instance_method(:db)) 
  define_method(:dab, instance_method(:ab)) 
  
  define_method(:df, instance_method(:f)) 
  define_method(:ddf, instance_method(:df)) 
  
  alias adf df
  alias addf ddf
  
  define_method(:daf, instance_method(:af)) 
  define_method(:dadf, instance_method(:adf))   
end

pt = ParseTree.new false

C.instance_methods(false).each { |m| 
  p pt.parse_tree_for_method(C, m)
}

