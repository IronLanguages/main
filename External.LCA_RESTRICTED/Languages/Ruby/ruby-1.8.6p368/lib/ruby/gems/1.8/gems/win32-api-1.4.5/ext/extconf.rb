##########################################################################
# extconf.rb
# 
# The Windows::API binary should be built using the Rake task, i.e.
# 'rake build' or 'rake install'.
##########################################################################
require 'mkmf'

have_func('strncpy_s')

create_makefile('win32/api', 'win32')
