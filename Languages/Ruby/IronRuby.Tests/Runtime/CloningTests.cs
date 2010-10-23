/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

namespace IronRuby.Tests {
    public partial class Tests {
        public void Clone1() {
            AssertOutput(delegate() {
                CompilerTest(@"
objects = [{}, [], '', Regexp.new('foo'), Object.new, Module.new, Class.new]

objects.each do |x| 
  puts x.class.name
  x.taint
  
  class << x
    CONST = 1
  
    def foo
      3
    end    
    
    instance_variable_set(:@iv_singleton_x, 2);
  end  

  x.instance_variable_set(:@iv_x, 4);
  y = x.clone
  
  raise '0' unless y.tainted?

  class << y
    raise '1' unless CONST == 1                                     # singleton constants copied
    raise '2' unless instance_variables.size == 0                   # singleton instance variables not copied
  end
  
  raise '3' unless y.foo == 3                                       # singleton methods copied
  raise '4' unless y.instance_variable_get(:@iv_x) == 4             # instance variables copied
end
");
            }, @"
Hash
Array
String
Regexp
Object
Module
Class
");
        }
        
        public void Dup1() {
            AssertOutput(delegate() {
                CompilerTest(@"
objects = [{}, [], '', Regexp.new('foo'), Object.new]

objects.each do |x| 
  puts x.class.name
  
  class << x
    CONST = 1
  
    def foo
      3
    end    
    
    instance_variable_set(:@iv_singleton_x, 2);
  end  

  x.instance_variable_set(:@iv_x, 4);
  y = x.dup
  
  class << y
    raise unless ((CONST;false) rescue true)          # constants NOT copied
    raise unless ((foo;false) rescue true)            # singleton class methods not copied
    raise unless instance_variables.size == 0         # instance variables on singleton not copied
  end
  
  raise unless ((y.foo;false) rescue true)            # methods NOT copied
  raise unless y.instance_variable_get(:@iv_x) == 4   # instance variables copied
end
");
            }, @"
Hash
Array
String
Regexp
Object
");
        }
        
        public void StructDup1() {
            AssertOutput(delegate() {
                CompilerTest(@"
class St < Struct
end

p X = St.dup
p Y = X.new(:a,:b)
p Z = Y.dup
p U = Z[1,2]
p V = U.dup
p V.members
");
            }, @"
X
Y
Z
#<struct Z a=1, b=2>
#<struct Z a=1, b=2>
[:a, :b]
");
        }
    }
}
