begin

foo & bar                  # warning: useless use of & in void context 
foo &bar                   # warning: `&' interpreted as argument prefix
foo&bar                    # warning: useless use of & in void context 
foo& bar                   # warning: useless use of & in void context 
x = foo & bar
x = foo &bar               # warning: `&' interpreted as argument prefix
x = foo&bar 
x = foo& bar

rescue

end
