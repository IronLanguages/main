require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe DataMapper::Validate::UniquenessValidator do

    before do
      class ::Organisation
        include DataMapper::Resource
        property :id, Serial
        property :name, String
        property :domain, String #, :unique => true

        validates_is_unique :domain, :allow_nil => true
      end

      class ::User
        include DataMapper::Resource
        property :id, Serial
        property :organisation_id, Integer
        property :user_name, String

        belongs_to :organisation #has :organisation, n..1

        validates_is_unique :user_name, :when => :testing_association, :scope => [:organisation]
        validates_is_unique :user_name, :when => :testing_property, :scope => [:organisation_id]
      end

      Organisation.auto_migrate!
      User.auto_migrate!

      repository do
         Organisation.new(:id=>1, :name=>'Org One', :domain=>'taken').save
         Organisation.new(:id=>2, :name=>'Org Two', :domain=>'two').save

         User.new(:id=>1, :organisation_id=>1, :user_name=>'guy').save
      end
    end

    it 'should validate the uniqueness of a value on a resource' do
      repository do
        o = Organisation.get!(1)
        o.should be_valid

        o = Organisation.new(:id=>20, :name=>"Org Twenty", :domain=>nil)
        o.should be_valid
        o.save

        o = Organisation.new(:id=>30, :name=>"Org Thirty", :domain=>nil)
        o.should be_valid
      end
    end

    it "should not even check if :allow_nil is true" do
      repository do
        o = Organisation.get!(1)
        o.should be_valid

        o = Organisation.new(:id=>2, :name=>"Org Two", :domain=>"taken")
        o.should_not be_valid
        o.errors.on(:domain).should include('Domain is already taken')

        o = Organisation.new(:id=>2, :name=>"Org Two", :domain=>"not_taken")
        o.should be_valid
      end
    end

    it 'should validate uniqueness on a string key' do
      class ::Department
        include DataMapper::Resource
        property :name, String, :key => true

        validates_is_unique :name
        auto_migrate!
      end

      hr = Department.create(:name => "HR")
      hr2 = Department.new(:name => "HR")
      hr2.valid?.should == false
    end

    it 'should validate the uniqueness of a value with scope' do
      repository do
        u = User.new(:id => 2, :organisation_id=>1, :user_name => 'guy')
        u.should_not be_valid_for_testing_property
        u.errors.on(:user_name).should include('User name is already taken')
        u.should_not be_valid_for_testing_association
        u.errors.on(:user_name).should include('User name is already taken')


        u = User.new(:id => 2, :organisation_id => 2, :user_name  => 'guy')
        u.should be_valid_for_testing_property
        u.should be_valid_for_testing_association
      end
    end
  end
end
