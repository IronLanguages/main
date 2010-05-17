require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

include DataMapper::Types

begin
  gem 'bcrypt-ruby', '~>2.0.5'
  require 'bcrypt'
rescue LoadError
  skip_tests = true
end

describe "DataMapper::Types::BCryptHash" do
  unless skip_tests

    before(:each) do
      @clear_password = "DataMapper R0cks!"
      @crypted_password = BCrypt::Password.create(@clear_password)
      @nonstandard_type = 1

      class TestType
        @a = 1
        @b = "Hi There"
      end
      @nonstandard_type2 = TestType.new
    end

    describe ".dump" do
      it "should return a crypted hash as a BCrypt::Password" do
        BCryptHash.dump(@clear_password, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should return a string that is 60 characters long" do
        BCryptHash.dump(@clear_password, :property).to_s.should have(60).characters
      end

      it "should return nil if nil is passed" do
        BCryptHash.dump(nil, :property).should be_nil
      end
    end

    describe ".load" do
      it "should return the password as a BCrypt::Password" do
        BCryptHash.load(@crypted_password, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should return the password as a password which matches" do
        BCryptHash.load(@crypted_password, :property).should == @clear_password
      end

      it "should return nil if given nil" do
        BCryptHash.load(nil, :property).should be_nil
      end
    end

    describe ".typecast" do
      it "should return the crypted_password as a BCrypt::Password" do
        BCryptHash.typecast(@crypted_password, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should match the password as a BCrypt::Password" do
        BCryptHash.typecast(@crypted_password, :property).should == @clear_password
      end

      it "should return the string value of crypted_password as a BCrypt::Password" do
        BCryptHash.typecast(@crypted_password.to_s, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should match the password as a string of the crypted_password" do
        BCryptHash.typecast(@crypted_password.to_s, :property).should == @clear_password
      end

      it "should return the password as a BCrypt::Password" do
        BCryptHash.typecast(@clear_password, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should match the password as clear_password" do
        BCryptHash.typecast(@clear_password, :property).should == @clear_password
      end

      it "should encrypt any type that has to_s" do
        BCryptHash.typecast(@nonstandard_type, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should match the non-standard type" do
        BCryptHash.typecast(@nonstandard_type, :property).should == @nonstandard_type
      end

      it "should encrypt anything passed to it" do
        BCryptHash.typecast(@nonstandard_type2, :property).should be_an_instance_of(BCrypt::Password)
      end

      it "should match user-defined types" do
        BCryptHash.typecast(@nonstandard_type2, :property).should == @nonstandard_type2
      end

      it "should return nil if given nil" do
        BCryptHash.typecast(nil, :property).should be_nil
      end
    end
  else
    it "requires the bcrypt-ruby gem to test"
  end
end
