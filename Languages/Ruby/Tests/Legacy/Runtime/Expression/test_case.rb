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

require '../../util/simple_test.rb'

def divide_by_zero; 1/0; end

describe 'case expression with no argument' do
  it 'empty case expression' do
      def f(v)
          case; when v; end
      end 
      
      f(false).should == nil 
      f(true).should == nil
  end 
  
  it 'empty case expression again' do
     def f(v)
        case
            when false; 1 
            when v
            when true; 2 
        end
     end 
     f(false).should == 2
     f(true).should == nil
  end 

  it 'empty case with result' do
    def f(v)
        case
          when v: 10
        end
    end
    
    f(nil).should == nil
    f(:ab).should == 10
  end
  
  it 'parallel cases' do
    def f(x, y)
        case; when x; $g+=10; 7; end 
        case; when y; $g+=100; 8; end
    end
    
    $g = 1; f(false, false).should == nil; $g.should == 1
    $g = 1; f(true, false).should == nil; $g.should == 11
    $g = 1; f(false, true).should == 8; $g.should == 101
    $g = 1; f(true, true).should == 8; $g.should == 111
  end

  it 'nested cases' do
    def f(x, y)
      case; when x:
        $g+= 10; 14
        case; when y:
          $g += 100; 15
        end
      end
    end
    
    $g = 1; f(false, false).should == nil; $g.should == 1
    $g = 1; f(true, false).should == nil;  $g.should == 11
    $g = 1; f(false, true).should == nil;  $g.should == 1
    $g = 1; f(true, true).should == 15;    $g.should == 111
    
    def f(x, y, z, w)
        case; when x;
            $g += 10 
            case; when y; $g += 100; end
            case; when z;
                $g += 1000
                if w;   $g += 10000; end 
                $g += 1000
            end
            $g += 10
        end
    end
    
    $g = 1; f(false, false, true, true);  $g.should == 1
    $g = 1; f(true, false, false, false); $g.should == 21
    $g = 1; f(true, false, false, true);  $g.should == 21
    $g = 1; f(true, false, true, false);  $g.should == 2021
    $g = 1; f(true, false, true, true);   $g.should == 12021
    $g = 1; f(true, true, false, false);  $g.should == 121
    $g = 1; f(true, true, false, true);   $g.should == 121
    $g = 1; f(true, true, true, false);   $g.should == 2121
    $g = 1; f(true, true, true, true);    $g.should == 12121
  end

  it 'case with empty else' do
    def f(x)
      case when x; 23
      else
      end 
    end

    f(true).should == 23
    f(false).should == nil
  end

  it 'case with else' do
    def f(x)
      case
        when x: 27
      else 28
      end 
    end

    f(true).should == 27
    f(false).should == 28
  end 

  it 'case with 2 whens' do
    def f(x, y)
      case
        when x
          33
        when y
          34
      end
    end
    
    f(false, false).should == nil
    f(false, true).should == 34
    f(true, false).should == 33
    f(true, true).should == 33
  end 

  it 'case with 3 whens' do
    def f(x, y, z)
      case
        when x; 41
        when y; 42
        when z; 43
      end
    end
    
    f(nil, nil, nil).should == nil
    f(nil, nil, 111).should == 43
    f(nil, 222, nil).should == 42
    f(nil, 333, 'a').should == 42
    f('b', nil, nil).should == 41
    f(444, nil, 'c').should == 41
    f(555, 'd', nil).should == 41
    f('e', :fg, 1.0).should == 41    
  end 

  it 'case with when and else' do
    def f(x, y)
        case
          when x; 55
          when y; 56
          else; 57
        end 
    end 
    f(false, false).should ==  57
    f(false, true).should ==  56
    f(true, false).should ==  55
    f(true, true).should ==  55
  end 

  it 'case with exception in test' do    
    def f
      case
        when ($g+=10; divide_by_zero; $g+=100)
          $g += 1000
        else
          $g += 10000
      end
    end 
    $g = 1; begin; f; rescue; $g += 100000; end; $g.should == 100011
  end
  
  it 'case with expection in body' do
    def f(x)
      case
        when x
          $g += 10; divide_by_zero; $g+=100
        else 
          $g += 1000; divide_by_zero; $g+=10000
      end 
    end 
    $g = 1; begin; f true; rescue; $g += 100000; end; $g.should == 100011
    $g = 1; begin; f false; rescue; $g += 100000; end; $g.should == 101001
  end
  
  it 'how it handles redo/break/next/return' do 
    def f1; case; when true; next; end; end 
    def f2; case; when false; else; next; end; end 
    
    def f3; case; when true; redo; end; end 
    def f4; case; when false; else; redo; end; end 

    def f5; case; when true; retry; end; end 
    def f6; case; when false; else; retry; end; end 

    should_raise(LocalJumpError) { f1 }
    should_raise(LocalJumpError) { f2 }
    should_raise(LocalJumpError) { f3 }
    should_raise(LocalJumpError) { f4 }
    should_raise(LocalJumpError) { f5 } 
    should_raise(LocalJumpError) { f6 }
    
    def f(x); case; when x; break 1; else; break 2; end; end
    should_raise(LocalJumpError) { f(true) }
    should_raise(LocalJumpError) { f(false) }
    
    begin
        case; when true; return 1; end 
    rescue LocalJumpError
    else 
        raise
    end 
    
    def f(x); case; when x; return 1; else; return 2; end; $g = 1; end
    $g = 0; f(true).should == 1; $g.should == 0
    $g = 0; f(false).should == 2; $g.should == 0
  end 
  
  it "stop evaluating multiple conditions after hitting true" do 
      def f(x, y)
          case  
              when ($g += 1; false):
                  $g += 10
              when ($g += 100; x), ($g += 1000; y): 
                  $g += 10000
              when ($g += 100000; true):
                  $g += 1000000
          end
      end
      $g = 0; f(true, true); $g.should == 10101
      $g = 0; f(true, false); $g.should == 10101
      $g = 0; f(false, true); $g.should == 11101
      $g = 0; f(false, false); $g.should == 1101101
      
      def f(x, y)
        case 
            when ($g += 1; x) || ($g += 10; y): $g += 100
            else $g += 1000
        end 
      end 
      $g = 0; f(true, true); $g.should == 101
      $g = 0; f(true, false); $g.should == 101
      $g = 0; f(false, true); $g.should == 111
      $g = 0; f(false, false); $g.should == 1011
  end
  
  it "allows then|: after when" do
    def f(x)
        case 
            when x then 1
            when (not x): 2
        end 
    end
    f(true).should == 1
    f(false).should == 2
  end 
  
end

describe 'case with an argument' do
  it 'case calls ===' do
    class Foo
      @@foo = 0
      def === x
        @@foo += 1
        false
      end
      def self.count
        @@foo
      end
    end

    class Foo2
      @@foo2 = 0
      def === x
        @@foo2 += 1
        false
      end
      def self.count
        @@foo2
      end
    end
    
    [Foo.count, Foo2.count].should == [0, 0]
    x = (case Foo.new; when Foo2.new; end)
    [Foo.count, Foo2.count].should == [0, 1]
    y = (case Foo2.new; when Foo.new; end)
    [Foo.count, Foo2.count].should == [1, 1]
    y = (case Foo2.new; when Foo.new; end)
    [Foo.count, Foo2.count].should == [2, 1]
    x = (case Foo.new; when Foo2.new; end)
    [Foo.count, Foo2.count].should == [2, 2]
  end
  
  it 'throws when === is not defined' do 
#    class Foo3
#        undef ===
#    end 
#    x = 1
#    y = Foo3.new
#    should_raise(NoMethodError) do 
#        case x; 
#          when y; 10
#          else; 20
#        end 
#    end 
#    
#    z = case y;
#        when x; 10
#        else; 20
#    end 
#    z.should == 20
  end 
  
  it 'allows the expression to be literal' do
    def f(x)
        case 1; 
            when x; 10
            else; 20
        end 
    end 
    f(1).should == 10
    f(2).should == 20
    
    def f(x)
        case 'abc'
            when x; 10
            else; 20
        end 
    end
    f('bcd').should == 20
    f('abc').should == 10
  end 
  
  it 'basic case usage' do
    def foo(w, x=nil, y=nil, z=nil)
      case w
        when 123: :x123
        when 456: :x456
        when x: x
        when 789, *y: :x789
        when *z: z
      else
        "unknown"
      end
    end

    class Bob
      def === x
        x === 'bob'
      end
    end

    foo(123).should == :x123
    foo(456).should == :x456
    foo(789).should == :x789
    foo(555).should == 'unknown'
    foo(nil).should == nil
    foo('bob', 'bob').should == 'bob'
    b = Bob.new
    foo(b, 'bob').should == 'unknown'
    foo('bob', b).should == b
    b2 = Bob.new
    foo(b, b2).should == b2
    foo(1, nil, nil, [3, 2, 1, 4]).should == [3,2,1,4]
    foo(2, nil, [3, 2, 1, 4], nil).should == :x789
  end  
  
  it 'case value is only evaluated once' do
    def foo x
      $x += 1
      x
    end
    $x = 0
    def bar(x, y=nil)
      case foo(x)
        when 1: 1
        when 2: 2
        when 3: 3
        when 4: 4
        when 5, *[6,7,8,9]: 5
        when y: y
      end
    end
    bar(3);    $x.should == 1
    bar(7);    $x.should == 2
    bar(9, 9); $x.should == 3
    bar('a');  $x.should == 4
  end
  
  it 'stops evaluating after finding one equality, etc' do
    def f(x)
        case x
        when ($g += 1; false): $g += 1
        when ($g += 10; 10), ($g+= 100; 11)
            $g += 1000
        when ($g += 10000; true) then $g += 100000
        else
            $g += 1000000
        end
    end 
    $g = 0; f(10); $g.should == 1011
    $g = 0; f(11); $g.should == 1111
    $g = 0; f(true); $g.should == 110111
    $g = 0; f(1); $g.should == 1010111
  end 
end

finished

