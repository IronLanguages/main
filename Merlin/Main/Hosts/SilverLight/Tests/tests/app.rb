#
# Test harness for running Microsoft::Scripting::Silverlight tests. 
#

$: << 'lib/eggs/lib'
require 'eggs'

require 'debugger'

require 'helper'

Eggs.config(
  :unit => %W(
    console
    dynamic_application
    extension_types
    package
    window
  ),
  :integration => [
    '01',
    'args',
    'auto_addref',
    'error_handle',
    'execfile',
    'issubclass',
    #'modules',
    #'multi_import',
    #'name',
    #'net',
    #'querystring',
    #'re',
    #'smoke',
    #'sys_path',
    #'s_clock_rb',
    #'s_dlr_console',
    #'s_fractulator',
    #'thread',
    #'utf8',
    #'xamlloader',
    #'xcode',
    #'xcode_semantics',
    #'x_attribute_error',
    #'x_devidebyzero',
    #'x_import_1',
    #'x_import_2',
    #'x_import_3',
    #'x_rethrow',
    #'x_syntax_error',
    #'x_typeerror'
  ]
)

Eggs.run
