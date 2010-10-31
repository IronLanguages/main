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

require '../../util/assert.rb'

def m x
    if x > 2
      10
    end
end 

assert_return(10) { m 3 }
assert_return(nil) { m 1 }

def m
    return 1, 2
end 

assert_return([1,2]) { m }

def m
    return [2, 3]
end 

assert_return([2,3]) { m }

