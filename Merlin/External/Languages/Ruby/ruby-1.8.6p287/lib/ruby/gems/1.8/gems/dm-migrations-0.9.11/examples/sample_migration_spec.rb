require File.dirname(__FILE__) + '/sample_migration'
require File.dirname(__FILE__) + '/../lib/spec/example/migration_example_group'

describe :create_people_table, :type => :migration do

  before do
    run_migration
  end

  it 'should create a people table' do
    repository(:default).should have_table(:people)
  end

  it 'should have an id column as the primary key' do
    table(:people).should have_column(:id)
    table(:people).column(:id).type.should == 'integer'
    #table(:people).column(:id).should be_primary_key
  end

  it 'should have a name column as a string' do
    table(:people).should have_column(:name)
    table(:people).column(:name).type.should == 'character varying'
    table(:people).column(:name).should permit_null
  end

  it 'should have a nullable age column as a int' do
    table(:people).should have_column(:age)
    table(:people).column(:age).type.should == 'integer'
    table(:people).column(:age).should permit_null
  end

end

describe :add_dob_to_people, :type => :migration do

  before do
    run_migration
  end

  it 'should add a dob column as a timestamp' do
    table(:people).should have_column(:dob)
    table(:people).column(:dob).type.should == 'timestamp without time zone'
    table(:people).column(:dob).should permit_null
  end

end
