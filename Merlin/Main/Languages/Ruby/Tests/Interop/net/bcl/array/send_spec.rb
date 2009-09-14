require File.dirname(__FILE__) + "/../../spec_helper"

describe ".NET Array's" do
  it "can be used as parameters to Kernel#send" do
    obj = Object.new
    class << obj
      def foo
        'foo'
      end

      def bar(*args)
        args
      end
    end

    arr1 = System::Array[Fixnum].new(1,1)
    arr2 = System::Array[Object].new(0)
    arr3 = System::Array[Object].new(1,1)

    [arr1, arr2, arr3].each do |arr|
      lambda {obj.send(:foo, arr)}.should raise_error
      lambda {obj.__send__(:foo, arr)}.should raise_error
    end

    [arr1, arr2, arr3].each do |arr|
      obj.send(:foo, arr).should == arr
      obj.__send__(:foo, arr).should == arr
    end
  end
end
