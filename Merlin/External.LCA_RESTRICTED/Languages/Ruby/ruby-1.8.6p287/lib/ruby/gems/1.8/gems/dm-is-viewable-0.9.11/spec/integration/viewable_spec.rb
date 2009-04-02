require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'
require Pathname(__FILE__).dirname.expand_path.parent + 'data/location'
require Pathname(__FILE__).dirname.expand_path.parent + 'data/person'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe 'DataMapper::Is::Viewable' do
    before :all do
      DataMapper.auto_migrate!
      colors  = %w(red green blue purple black)
      names   = %w(john jane mary michael dookmaster)
      [
        'New York', 'Los Angeles', 'Dallas', 'Tampa', 'Miami', 'Orlando', 'Gainesville', 'Tallahassee',
        'Fort Myers', 'Columbus', 'Charlotte'
      ].each{|l| Location.create(:name => l)}

      Location.all(:name  => 'Fort Myers').update!(:description => 'sucky')

      10.times{|x| Person.create(:name => names.random, :favorite_color => colors.random, :favorite_number => rand(200))}
    end

    after :all do
      Location.all().destroy!
      Person.all().destroy!
    end

    it 'should respond to Resource.create_view' do
      Person.respond_to?(:create_view).should be(true)
      Location.respond_to?(:create_view).should be(true)
    end

    it 'should respond to Resource.view' do
      Person.respond_to?(:view).should be(true)
      Location.respond_to?(:view).should be(true)
    end

    it 'should be able to store views' do
      Person.create_view :city_folk, :location => ['Los Angeles','New York']
      Person.create_view :serial_killers, :favorite_color => 'black'
      Location.create_view :florida, :name => [
        'Tampa',        'Miami',
        'Orlando',      'Gainesville',
        'Tallahassee',  'Fort Myers'
      ]

    end

    it 'should not share the views between Resource descendents' do
      Location.views.length.should be(1)
      Person.views.length.should be(3)
    end

    it 'should merge query parameters' do
      Location.view(:florida).length.should be(6)
      Location.view(:florida, :description => 'sucky').length.should be(1)
    end
  end
end
