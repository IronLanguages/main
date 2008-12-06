def y; yield; end

l = lambda { 1 }
puts l[]

p = Proc.new { 1 }
puts p[]

l = lambda { next 1; 2 }
puts l[]                       # nil (bug ???)

p = Proc.new { next 1; 2 }
puts p[]

l = lambda { 1 }
puts y(&l)

p = Proc.new { 1 }
puts y(&p)

l = lambda { next 1; 2 }
puts y(&l)

p = Proc.new { next 1; 2 }
puts y(&p)
