C = 'on Object'

def foo
  puts C, ::C                    # goes to Object
  eval('::C = "in eval"')        # BUG? defines C in the anonymous module
end

load 'Constants_Global_WriteBug2_2.rb', true