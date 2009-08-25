system "ruby testcustom.rb"
system "ruby testbase.rb"
system "ruby testdefault.rb"
for i in 1..4
  system "ruby testxmlconf.rb #{i}"
end
