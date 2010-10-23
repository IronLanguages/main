# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

# raise string - creates a new RuntimeError exception, setting its message to the given string

require '../../util/assert.rb'

def bad_thing(s)
  raise s 
end 

["", "a", "a bc", "\n34\7", "abc \r\ndef gh\r\n\r\n ijk"].each do |s|
    $g = 1
    begin 
        bad_thing s
    rescue
        $g += 10
        assert_isinstanceof($!, RuntimeError)
        assert_equal($!.message, s)
    else
        $g += 100
    end 
    assert_equal($g, 11)
end 

