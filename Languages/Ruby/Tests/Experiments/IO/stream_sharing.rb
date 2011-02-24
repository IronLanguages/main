f = File.new("a.txt", "w")

x = IO.new(f.to_i, "w")
x.puts('hello')
x.flush
x.close

f.puts('bye' * 10000) rescue p $!  # Ruby doesn't throw here if we write less than internal buffer size - bug?
