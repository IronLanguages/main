require 'pathname'
require Pathname(__FILE__).dirname.expand_path + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe 'DataMapper::Qutery' do
    class User
      include DataMapper::Resource

      property :id, Serial
      property :name, String
      property :age, Integer
      property :rating, Integer

      auto_migrate!
    end

    User.create(:name => 'john', :age => 20)
    User.create(:name => 'mark', :age => 41)
    User.create(:name => 'jane', :age => 62)

    it "should work just as usual query" do
      User.all{age == 62}.length.should == 1
      User.all{age > 20}.length.should == 2
      User.all{age >= 20}.length.should == 3
      User.all{age < 30}.length.should == 1
      User.all{age <= 20}.length.should == 1
      User.all{age < 45}.length.should == 2

      User.all{age == 62 && name == 'mark'}.length.should == 0
      User.all{age == 62 && name == 'jane'}.length.should == 1
      User.all{name == ['mark','jane'] && age > 20}.length.should == 2

      User.all{age > 10}[1,2].length.should == 2

      User.all{name =~ 'j%'}.length.should == 2

      User.first{name ~ 'mark'}.id.should == 1

      User.all{name == ['jane','john']}.all{age > 30}.length.should == 1

    end

  end
end
