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

## Top-level methods are available to the public, but only for private use. 
# a private method cannot have a receiver specified. Whenever no receiver is specified, whatever object made
# the call is considered the receiver.

def top_level_method
    100
end 

class C
    def call_tlm
        top_level_method
    end 
end 

x = C.new 
assert_equal(x.call_tlm, 100)
#assert_raise(NoMethodError) { x.top_level_method }
#assert_true { x.private_methods.include? 'top_level_method' }

assert_equal(top_level_method, 100)
#assert_raise(NoMethodError) { self.top_level_method }

## top_level_class_method

def self.top_level_class_method;
    200
end 
assert_equal(self.top_level_class_method, 200)
assert_equal(self::top_level_class_method, 200)
assert_equal(top_level_class_method, 200)

#assert_false { x.methods.include? 'top_level_class_method' }
#assert_true { self.public_methods.include? 'top_level_class_method' }


## locals/variables

top_level_variable = 200

assert_equal(top_level_variable, 200)
assert_raise(NoMethodError) { self.top_level_variable }

def method
    top_level_variable
end 
#assert_raise(NameError) { method }

class C
    def call_tlv
        top_level_variable
    end 
end   
#assert_raise(NameError) { C.new.call_tlv }


