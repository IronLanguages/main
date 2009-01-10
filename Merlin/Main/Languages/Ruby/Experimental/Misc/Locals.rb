def foo()
    b = 123
	puts "before eval"
	eval("a = b;puts a,b")
	puts "after eval"
	puts a
end

foo