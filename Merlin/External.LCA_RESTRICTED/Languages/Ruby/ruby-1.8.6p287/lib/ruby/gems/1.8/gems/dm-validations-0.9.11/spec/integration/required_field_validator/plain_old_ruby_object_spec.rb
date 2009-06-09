require 'pathname'
__dir__ = Pathname(__FILE__).dirname.expand_path

require __dir__.parent.parent + 'spec_helper'
require __dir__ + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe "A plain old Ruby object (not a DM resource)" do
    before do
      class PlainClass
        extend DataMapper::Validate::ClassMethods
        include DataMapper::Validate
        attr_accessor :accessor
        validates_present :here, :empty, :nil, :accessor
        def here;  "here" end
        def empty; ""     end
        def nil;   nil    end
      end

      @pc = PlainClass.new
    end

    it "should fail validation with empty, nil, or blank fields" do
      @pc.should_not be_valid
      @pc.errors.on(:empty).should    include("Empty must not be blank")
      @pc.errors.on(:nil).should      include("Nil must not be blank")
      @pc.errors.on(:accessor).should include("Accessor must not be blank")
    end

    it "giving accessor a value should remove validation error" do
      @pc.accessor = "full"
      @pc.valid?
      @pc.errors.on(:accessor).should be_nil
    end
  end
end
