require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

begin
  gem 'bcrypt-ruby', '~>2.0.5'
  require 'bcrypt'
rescue LoadError
  skip_tests = true
end

describe 'DataMapper::Types::BCryptHash' do
  unless skip_tests
    describe "with no options" do
      before(:each) do
        class User
          include DataMapper::Resource

          property :id, Serial
          property :password, BCryptHash
        end
        User.auto_migrate!
        User.create(:password => "DataMapper R0cks!")
      end

      it "should save a password to the DB on creation" do
        repository(:default) do
          User.create(:password => "password1")
        end
        user = User.all
        user[0].password.should == "DataMapper R0cks!"
        user[1].password.should == "password1"
      end

      it "should change the password on attribute update" do
        @user = User.first
        @user.attribute_set(:password, "D@t@Mapper R0cks!")
        @user.save
        @user.password.should_not == "DataMapper R0cks!"
        @user.password.should == "D@t@Mapper R0cks!"
      end

      it "should not change the password on save and reload" do
        @user = User.first
        v1 = @user.password.to_s
        @user.save
        @user.reload
        v2 = @user.password.to_s
        v1.should == v2
      end

      it "should have a cost of BCrypt::Engine::DEFAULT_COST" do
        @user = User.first
        @user.password.cost.should == BCrypt::Engine::DEFAULT_COST
      end
    end
  else
    it "Needs the bcrypt-ruby gem installed"
  end
end
