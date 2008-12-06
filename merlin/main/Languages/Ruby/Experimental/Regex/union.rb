p Regexp.union(/a/mix, /b/i, /c/)
p Regexp.union(/a/mix, /b/i, "asdasd*")
p Regexp.union()
p Regexp.union(/a/mix)
p Regexp.union("a")
p Regexp.union("", /a/mix)
p Regexp.union(//, /a/mix)
p Regexp.union(nil) rescue p $!