class P < Proc
end

q = P.new { puts 'foo' }
l1 = lambda &q
l2 = proc &q

p q.class, l1.class, l2.class

