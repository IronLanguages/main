a = "はうはう "

begin
  a[0]
  a =~ /foo/
  puts "Pass"
rescue => e
  puts e.class
end
