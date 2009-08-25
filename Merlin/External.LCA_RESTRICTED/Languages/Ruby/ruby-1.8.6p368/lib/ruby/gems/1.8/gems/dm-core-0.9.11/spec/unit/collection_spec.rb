require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

# ensure the Collection is extremely similar to an Array
# since it will be returned by Respository#all to return
# multiple resources to the caller
describe DataMapper::Collection do
  before do
    @property = mock('property')
    @model    = mock('model', :inheritance_property => [ @property ], :key => [ @property ])
    @query    = mock('query', :kind_of? => true, :fields => [ @property ], :model => @model)

    @collection = DataMapper::Collection.new(@query) {}
  end

  it 'should provide #<<' do
    @collection.should respond_to(:<<)
  end

  it 'should provide #all' do
    @collection.should respond_to(:all)
  end

  it 'should provide #at' do
    @collection.should respond_to(:at)
  end

  it 'should provide #build' do
    @collection.should respond_to(:build)
  end

  it 'should provide #clear' do
    @collection.should respond_to(:clear)
  end

  it 'should provide #collect!' do
    @collection.should respond_to(:collect!)
  end

  it 'should provide #concat' do
    @collection.should respond_to(:concat)
  end

  it 'should provide #create' do
    @collection.should respond_to(:create)
  end

  it 'should provide #delete' do
    @collection.should respond_to(:delete)
  end

  it 'should provide #delete_at' do
    @collection.should respond_to(:delete_at)
  end

  it 'should provide #destroy!' do
    @collection.should respond_to(:destroy!)
  end

  it 'should provide #each' do
    @collection.should respond_to(:each)
  end

  it 'should provide #each_index' do
    @collection.should respond_to(:each_index)
  end

  it 'should provide #eql?' do
    @collection.should respond_to(:eql?)
  end

  it 'should provide #fetch' do
    @collection.should respond_to(:fetch)
  end

  it 'should provide #first' do
    @collection.should respond_to(:first)
  end

  it 'should provide #freeze' do
    @collection.should respond_to(:freeze)
  end

  it 'should provide #get' do
    @collection.should respond_to(:get)
  end

  it 'should provide #get!' do
    @collection.should respond_to(:get!)
  end

  it 'should provide #insert' do
    @collection.should respond_to(:insert)
  end

  it 'should provide #last' do
    @collection.should respond_to(:last)
  end

  it 'should provide #load' do
    @collection.should respond_to(:load)
  end

  it 'should provide #loaded?' do
    @collection.should respond_to(:loaded?)
  end

  it 'should provide #pop' do
    @collection.should respond_to(:pop)
  end

  it 'should provide #push' do
    @collection.should respond_to(:push)
  end

  it 'should provide #properties' do
    @collection.should respond_to(:properties)
  end

  it 'should provide #reject' do
    @collection.should respond_to(:reject)
  end

  it 'should provide #reject!' do
    @collection.should respond_to(:reject!)
  end

  it 'should provide #relationships' do
    @collection.should respond_to(:relationships)
  end

  it 'should provide #reload' do
    @collection.should respond_to(:reload)
  end

  it 'should provide #reverse' do
    @collection.should respond_to(:reverse)
  end

  it 'should provide #reverse!' do
    @collection.should respond_to(:reverse!)
  end

  it 'should provide #reverse_each' do
    @collection.should respond_to(:reverse_each)
  end

  it 'should provide #select' do
    @collection.should respond_to(:select)
  end

  it 'should provide #shift' do
    @collection.should respond_to(:shift)
  end

  it 'should provide #slice' do
    @collection.should respond_to(:slice)
  end

  it 'should provide #slice!' do
    @collection.should respond_to(:slice!)
  end

  it 'should provide #sort' do
    @collection.should respond_to(:sort)
  end

  it 'should provide #sort!' do
    @collection.should respond_to(:sort!)
  end

  it 'should provide #unshift' do
    @collection.should respond_to(:unshift)
  end

  it 'should provide #update!' do
    @collection.should respond_to(:update!)
  end

  it 'should provide #values_at' do
    @collection.should respond_to(:values_at)
  end
end
