require File.expand_path(File.join(File.dirname(__FILE__), '..', '..', 'spec_helper'))

describe DataMapper::Associations::ManyToOne do

  load_models_for_metaphor :vehicles

  it 'should allow a declaration' do
    lambda do
      class ::Vehicle
        belongs_to :manufacturer
      end
    end.should_not raise_error
  end
end

describe DataMapper::Associations::ManyToOne::Proxy do

  load_models_for_metaphor :vehicles

  before do
    @child        = mock('child', :kind_of? => true)
    @parent       = mock('parent', :nil? => false, :new_record? => false)
    @relationship = mock('relationship', :kind_of? => true, :get_parent => @parent, :attach_parent => nil)
    @association  = DataMapper::Associations::ManyToOne::Proxy.new(@relationship, @child)

    @association.replace(@parent)
  end

  it 'should provide #replace' do
    @association.should respond_to(:replace)
  end

  describe '#class' do
    it 'should be forwarded to parent' do
      @parent.should_receive(:class).and_return(Manufacturer)
      @association.class.should == Manufacturer
    end
  end

  describe '#replace' do
    before do
      @other = mock('other parent')
    end

    before do
      @relationship.should_receive(:attach_parent).with(@child, @other)
    end

    it 'should remove the resource from the collection' do
      @association.should == @parent
      @association.replace(@other)
      @association.should == @other
    end

    it 'should not automatically save that the resource was removed from the association' do
      @other.should_not_receive(:save)
      @association.replace(@other)
    end

    it 'should return the association' do
      @association.replace(@other).object_id.should == @association.object_id
    end
  end

  it 'should provide #save' do
    @association.should respond_to(:replace)
  end

  describe '#save' do
    describe 'when the parent is nil' do
      before do
        @parent.stub!(:nil?).and_return(true)
      end

      it 'should not save the parent' do
        @association.save
      end

      it 'should return false' do
        @association.save.should == false
      end
    end

    describe 'when the parent is not a new record' do
      before do
        @parent.should_receive(:new_record?).with(no_args).and_return(false)
      end

      it 'should not save the parent' do
        @parent.should_not_receive(:save)
        @association.save
      end

      it 'should return true' do
        @association.save.should == true
      end
    end

    describe 'when the parent is a new record' do
      before do
        @parent.should_receive(:new_record?).with(no_args).and_return(true)
      end

      it 'should save the parent' do
        @relationship.should_receive(:with_repository).and_yield
        @parent.should_receive(:save).with(no_args)
        @association.save
      end

      it 'should return the result of the save' do
        child_key = mock("child_key")
        child_key.should_receive(:set).and_return(true)
        parent_key = mock("parent_key")
        parent_key.should_receive(:get).and_return(1)
        @relationship.should_receive(:with_repository).and_yield
        @relationship.should_receive(:child_key).and_return(child_key)
        @relationship.should_receive(:parent_key).and_return(parent_key)
        save_results = mock('save results')
        @parent.should_receive(:save).with(no_args).and_return(save_results)
        @association.save.object_id.should == save_results.object_id
      end
    end
  end

  it 'should provide #reload' do
    @association.should respond_to(:reload)
  end

  describe '#reload' do
    before(:each) do
      @mock_parent = mock('#reload test parent')
      @association.replace(@mock_parent)
    end

    it 'should set the @parent ivar to nil' do
      @association.__send__(:parent).should == @mock_parent # Sanity check.

      # We can't test the value of the instance variable since
      # #instance_variable_get will be run on the @parent (thanks to
      # Proxy#method_missing). Instead, test that Relationship#get_parent is
      # run -- if @parent wasn't set to nil, this expectation should fail.
      @relationship.should_receive(:get_parent).once.and_return(@mock_parent)
      @association.reload

      # Trigger #get_parent on the relationship.
      @association.__send__(:parent)
    end

    it 'should not change the foreign key in the child' do
      @relationship.should_not_receive(:attach_parent)
      @association.reload
    end

    it 'should return self' do
      @association.reload.should be_kind_of(DataMapper::Associations::ManyToOne::Proxy)
      @association.reload.object_id.should == @association.object_id
    end
  end
end
