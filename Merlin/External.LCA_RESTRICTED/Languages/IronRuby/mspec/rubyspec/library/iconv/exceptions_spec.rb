require File.dirname(__FILE__) + '/../../spec_helper'
require 'iconv'

# These are all in one file at the moment. They do not use a shared fixture
# becaues the examples might diverge once more details examples are added
# for the type of arguments, etc

describe "Iconv::BrokenLibrary" do
  it "inherits from RuntimeError" do
    Iconv::BrokenLibrary.superclass.should == RuntimeError
  end
  
  it "includes Iconv::Failure" do
    Iconv::BrokenLibrary.ancestors.should include(Iconv::Failure)
  end
  
  it "can be instantiated with 3 arguments" do
    Iconv::BrokenLibrary.new(101, 102, 103).should_not be_nil
  end
end

describe "Iconv::InvalidEncoding" do
  it "inherits from ArgumentError" do
    Iconv::InvalidEncoding.superclass.should == ArgumentError
  end
  
  it "includes Iconv::Failure" do
    Iconv::InvalidEncoding.ancestors.should include(Iconv::Failure)
  end
  
  it "can be instantiated with 3 arguments" do
    Iconv::InvalidEncoding.new(101, 102, 103).should_not be_nil
  end
end

describe "Iconv::InvalidCharacter" do
  it "inherits from ArgumentError" do
    Iconv::InvalidCharacter.superclass.should == ArgumentError
  end
  
  it "includes Iconv::Failure" do
    Iconv::InvalidCharacter.ancestors.should include(Iconv::Failure)
  end
  
  it "can be instantiated with 3 arguments" do
    Iconv::InvalidCharacter.new(101, 102, 103).should_not be_nil
  end
end

describe "Iconv::IllegalSequence" do
  it "inherits from ArgumentError" do
    Iconv::IllegalSequence.superclass.should == ArgumentError
  end
  
  it "includes Iconv::Failure" do
    Iconv::IllegalSequence.ancestors.should include(Iconv::Failure)
  end
  
  it "can be instantiated with 3 arguments" do
    Iconv::IllegalSequence.new(101, 102, 103).should_not be_nil
  end
end

describe "Iconv::OutOfRange" do
  it "inherits from RuntimeError" do
    Iconv::OutOfRange.superclass.should == RuntimeError
  end
  
  it "includes Iconv::Failure" do
    Iconv::OutOfRange.ancestors.should include(Iconv::Failure)
  end
  
  it "can be instantiated with 3 arguments" do
    Iconv::OutOfRange.new(101, 102, 103).should_not be_nil
  end
end
