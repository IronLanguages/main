require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))

describe DataMapper::Associations::OneToMany do

  load_models_for_metaphor :vehicles

  before do
    @class = Class.new do
      def self.name
        'User'
      end

      include DataMapper::Resource

      property :user_id, Integer, :key => true
    end
  end

  it 'should provide #has' do
    @class.should respond_to(:has)
  end

  describe '#has' do
    it 'should return a Relationship' do
      @class.has(@class.n, :orders).should be_kind_of(DataMapper::Associations::Relationship)
    end

    describe 'relationship' do
      before do
        @relationship = mock('relationship')
        DataMapper::Associations::Relationship.stub!(:new).and_return(@relationship)
      end

      it 'should receive the name' do
        DataMapper::Associations::Relationship.should_receive(:new) do |name,_,_,_,_|
          name.should == :orders
        end
        @class.has(@class.n, :orders)
      end

      it 'should receive the repository name' do
        DataMapper::Associations::Relationship.should_receive(:new) do |_,repository_name,_,_,_|
          repository_name.should == :mock
        end
        repository(:mock) do
          @class.has(@class.n, :orders)
        end
      end

      it 'should receive the child model name when passed in as class_name' do
        DataMapper::Associations::Relationship.should_receive(:new) do |_,_,child_model_name,_,_|
          child_model_name.should == 'Company::Order'
        end
        @class.has(@class.n, :orders, :class_name => 'Company::Order')
      end

      it 'should receive the child model name when class_name not passed in' do
        DataMapper::Associations::Relationship.should_receive(:new) do |_,_,child_model_name,_,_|
          child_model_name.should == 'Order'
        end
        @class.has(@class.n, :orders)
      end

      it 'should receive the parent model name' do
        DataMapper::Associations::Relationship.should_receive(:new) do |_,_,_,parent_model_name,_|
          parent_model_name.should == @class
        end
        @class.has(@class.n, :orders)
      end

      it 'should receive the parent model name' do
        options = { :min => 0, :max => 100 }
        DataMapper::Associations::Relationship.should_receive(:new) do |_,_,_,parent_model_name,_|
          options.object_id.should == options.object_id
        end
        @class.has(@class.n, :orders, options)
      end
    end

    it 'should add an accessor for the proxy' do
      @class.new.should_not respond_to(:orders)
      @class.has(@class.n, :orders)
      @class.new.should respond_to(:orders)
    end

    describe 'proxy accessor' do
      before :all do
        class ::User
          include DataMapper::Resource
        end

        class ::Order
          include DataMapper::Resource
        end
      end

      it 'should return a OneToMany::Proxy' do
        @class.has(@class.n, :orders)
        @class.new.orders.should be_kind_of(DataMapper::Associations::OneToMany::Proxy)
      end
    end
  end

  it 'should work with classes inside modules'
end

describe DataMapper::Associations::OneToMany::Proxy do
  before do
    @parent       = mock('parent', :new_record? => true, :kind_of? => true)
    @resource     = mock('resource', :null_object => true)
    @collection   = []
    @parent_key   = mock('parent key', :get => [])
    @repository   = mock('repository', :save => nil, :kind_of? => true)
    @relationship = mock('relationship', :get_children => @collection, :query => {}, :kind_of? => true, :child_key => [], :parent_key => @parent_key)
    @association  = DataMapper::Associations::OneToMany::Proxy.new(@relationship, @parent)
  end

  describe 'a method that relates the resource', :shared => true do
    it 'should add the resource to the collection' do
      @association.should_not include(@resource)
      do_add.should == return_value
      @association.should include(@resource)
    end

    it 'should not automatically save that the resource was added to the association' do
      @relationship.should_not_receive(:attach_parent)
      do_add.should == return_value
    end

    it 'should persist the addition after saving the association' do
      @relationship.should_receive(:with_repository).with(@resource).and_yield(@repository)
      do_add.should == return_value
      @relationship.should_receive(:attach_parent).with(@resource, @parent)
      @association.save
    end
  end

  describe 'a method that orphans the resource', :shared => true do
    before do
      @association << @resource
    end

    it 'should remove the resource from the collection' do
      @association.should include(@resource)
      do_remove.should == return_value
      @association.should_not include(@resource)
    end

    it 'should not automatically save that the resource was removed from the association' do
      @relationship.should_not_receive(:attach_parent)
      do_remove.should == return_value
    end

    it 'should persist the removal after saving the association' do
      @relationship.should_receive(:with_repository).with(@resource).and_yield(@repository)
      do_remove.should == return_value
      @relationship.should_receive(:attach_parent).with(@resource, nil)
      @association.save
    end
  end

  it 'should provide #<<' do
    @association.should respond_to(:<<)
  end

  describe '#<<' do
    def do_add
      @association << @resource
    end

    def return_value
      @association
    end

    it_should_behave_like 'a method that relates the resource'
  end

  it 'should provide #push' do
    @association.should respond_to(:push)
  end

  describe '#push' do
    def do_add
      @association.push(@resource)
    end

    def return_value
      @association
    end

    it_should_behave_like 'a method that relates the resource'
  end

  it 'should provide #unshift' do
    @association.should respond_to(:unshift)
  end

  describe '#unshift' do
    def do_add
      @association.unshift(@resource)
    end

    def return_value
      @association
    end

    it_should_behave_like 'a method that relates the resource'
  end

  it 'should provide #replace' do
    @association.should respond_to(:replace)
  end

  describe '#replace' do
    before do
      @children = [
        mock('child 1', :save => true),
        mock('child 2', :save => true),
      ]
      @collection << @resource
      @collection.stub!(:loaded?).and_return(true)
      @relationship.stub!(:attach_parent)
    end

    def do_replace
      @association.replace(@children)
    end

    def return_value
      @association
    end

    it 'should remove the resource from the collection' do
      @association.should include(@resource)
      do_replace.should == return_value
      @association.should_not include(@resource)
    end

    it 'should not automatically save that the resource was removed from the association' do
      @relationship.should_not_receive(:attach_parent)
      do_replace.should == return_value
    end

    it 'should persist the removal after saving the association' do
      do_replace.should == return_value
      @relationship.should_receive(:with_repository).exactly(3).times.and_yield(@repository)
      @relationship.should_receive(:attach_parent).with(@resource, nil)
      @association.save
    end

    it 'should not automatically save that the children were added to the association' do
      @relationship.should_not_receive(:attach_parent)
      do_replace.should == return_value
    end

    it 'should persist the addition after saving the association' do
      do_replace.should == return_value
      @relationship.should_receive(:with_repository).exactly(3).times.and_yield(@repository)
      @relationship.should_receive(:attach_parent).with(@children[0], @parent)
      @relationship.should_receive(:attach_parent).with(@children[1], @parent)
      @association.save
    end
  end

  it 'should provide #pop' do
    @association.should respond_to(:pop)
  end

  describe '#pop' do
    def do_remove
      @association.pop
    end

    def return_value
      @resource
    end

    it_should_behave_like 'a method that orphans the resource'
  end

  it 'should provide #shift' do
    @association.should respond_to(:shift)
  end

  describe '#shift' do
    def do_remove
      @association.shift
    end

    def return_value
      @resource
    end

    it_should_behave_like 'a method that orphans the resource'
  end

  it 'should provide #delete' do
    @association.should respond_to(:delete)
  end

  describe '#delete' do
    def do_remove
      @association.delete(@resource)
    end

    def return_value
      @resource
    end

    it_should_behave_like 'a method that orphans the resource'
  end

  it 'should provide #delete_at' do
    @association.should respond_to(:delete_at)
  end

  describe '#delete_at' do
    def do_remove
      @association.delete_at(0)
    end

    def return_value
      @resource
    end

    it_should_behave_like 'a method that orphans the resource'
  end

  it 'should provide #clear' do
    @association.should respond_to(:clear)
  end

  describe '#clear' do
    def do_remove
      @association.clear
    end

    def return_value
      @association
    end

    it_should_behave_like 'a method that orphans the resource'

    it 'should empty the collection' do
      @association << mock('other resource', :new_record? => false)
      @association.should have(2).entries
      do_remove
      @association.should be_empty
    end
  end

  it 'should provide #reload' do
    @association.should respond_to(:reload)
  end

  describe '#reload' do
    before do
      @children = [ mock('child 1', :save => true), mock('child 2', :save => true) ]
      @relationship.stub!(:get_children).and_return(@children)
    end

    it 'should set the @children ivar to nil' do
      @association.__send__(:children).should == @children # Sanity check.

      # We can't test the value of the @children instance variable since
      # #instance_variable_get will be run on @children (thanks to
      # Proxy#method_missing). Instead, test that Relationship#get_children is
      # run -- if @children wasn't set to nil, this expectation should fail.
      @relationship.should_receive(:get_children).once.and_return(@children)
      @association.reload

      # Trigger #get_children on the relationship.
      @association.__send__(:children).should == @children
    end

    it 'should return self' do
      @association.reload.should be_kind_of(DataMapper::Associations::OneToMany::Proxy)
      @association.reload.object_id.should == @association.object_id
    end
  end

  describe 'when deleting the parent' do
    it 'should delete all the children without calling destroy if relationship :dependent is :delete_all'

    it 'should destroy all the children if relationship :dependent is :destroy'

    it 'should set the parent key for each child to nil if relationship :dependent is :nullify'

    it 'should restrict the parent from being deleted if a child remains if relationship :dependent is restrict'

    it 'should be restrict by default if relationship :dependent is not specified'
  end
end
