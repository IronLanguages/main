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

# all args
def test_normal_arg
    def m0; return; end
    def m1 a; return a; end
    def m2 a,b; return a, b; end
    def m3 a,b,c; return a, b, c; end
    def m4 a,b,c,d; return a, b, c, d; end
    def m5 a,b,c,d,e; return a, b, c, d, e; end
    def m6 a,b,c,d,e,f; return a, b, c, d, e, f; end
    def m7 a,b,c,d,e,f,g; return a, b, c, d, e, f, g; end
    def m8 a,b,c,d,e,f,g,h; return a, b, c, d, e, f, g, h; end
    def m9 a,b,c,d,e,f,g,h,i; return a, b, c, d, e, f, g, h, i; end
    
    m9 1,2,4,5,6,7,8,9
end 


def test_defaultvalue_arg
    def m3 a,b,c; return a, b, c; end
    def m3 a,b,c=1; return a, b, c; end
    def m3 a,b=1,c=2; return a, b, c; end
    def m3 a=1,b=2,c=3; return a, b, c; end
    
    def m8 a,b,c,d,e,f,g,h; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c,d,e,f,g,h=1; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c,d,e,f,g=1,h=2; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c,d,e,f=1,g=2,h=3; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c,d,e=1,f=2,g=3,h=4; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c,d=1,e=2,f=3,g=4,h=5; return a, b, c, d, e, f, g, h; end
    def m8 a,b,c=1,d=2,e=3,f=4,g=5,h=6; return a, b, c, d, e, f, g, h; end
    def m8 a,b=1,c=2,d=3,e=4,f=5,g=6,h=7; return a, b, c, d, e, f, g, h; end
    def m8 a=1,b=2,c=3,d=4,e=5,f=6,g=7,h=8; return a, b, c, d, e, f, g, h; end
end 