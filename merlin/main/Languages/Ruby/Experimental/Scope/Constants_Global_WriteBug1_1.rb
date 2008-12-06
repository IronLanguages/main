::C = 'on anonymous module'       # BUG? goes to the anonymous module
puts Module.nesting[0].constants  # C

puts C          # OK
puts ::C        # BUG? goes to Object

