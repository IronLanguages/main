def t; print 'T '; true; end
def f; print 'F '; false; end

puts(t && t)
puts(f && t)
puts(t && f)
puts(f && f)

puts(t || t)
puts(f || t)
puts(t || f)
puts(f || f)

puts(f || f && t && t)