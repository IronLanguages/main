x = class C
  return 1
rescue Exception => e
  puts e                             # unexpected return
end