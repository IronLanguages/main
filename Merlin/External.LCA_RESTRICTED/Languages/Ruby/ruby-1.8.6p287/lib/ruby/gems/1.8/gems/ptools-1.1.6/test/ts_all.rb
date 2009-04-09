########################################
# ts_all.rb
#
# All tests for the ptools package.
########################################
$LOAD_PATH.unshift(Dir.pwd)
$LOAD_PATH.unshift(Dir.pwd + "/test")

require 'tc_binary'
require 'tc_constants'
require 'tc_head'
require 'tc_image'
require 'tc_middle'
require 'tc_nlconvert'
require 'tc_null'
require 'tc_tail'
require 'tc_touch'
require 'tc_wc'
require 'tc_which'
require 'tc_whereis'
