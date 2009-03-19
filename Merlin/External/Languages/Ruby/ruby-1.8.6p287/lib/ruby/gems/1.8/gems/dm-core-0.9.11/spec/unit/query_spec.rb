require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

GOOD_OPTIONS = [
  [ :reload,   false     ],
  [ :reload,   true      ],
  [ :offset,   0         ],
  [ :offset,   1         ],
  [ :limit,    1         ],
  [ :limit,    2         ],
  [ :order,    [ DataMapper::Query::Direction.new(Article.properties[:created_at], :desc) ] ],
  [ :fields,   Article.properties.defaults.to_a ], # TODO: fill in allowed default value
  #[ :links,    [ :stub ] ], # TODO: fill in allowed default value
  [ :includes, [ :stub ] ], # TODO: fill in allowed default value
]

BAD_OPTIONS = {
  :reload     => 'true',
  :offset     => -1,
  :limit      => 0,
#  :order      => [],  # TODO: spec conditions where :order may be empty
#  :fields     => [],  # TODO: spec conditions where :fields may be empty
  :links      => [],
  :includes   => [],
  :conditions => [],
}

# flatten GOOD_OPTIONS into a Hash to remove default values, since
# default value, when defined, is always listed first in GOOD_OPTIONS
UPDATED_OPTIONS = GOOD_OPTIONS.inject({}) do |options,(attribute,value)|
  options.update attribute => value
end

UPDATED_OPTIONS.merge!({ :fields => [ :id, :author ]})

describe DataMapper::Query do
  before do
    @adapter    = mock('adapter')
    @repository = mock('repository', :kind_of? => true, :name => 'mock', :adapter => @adapter)

    @query = DataMapper::Query.new(@repository, Article)
  end

  it 'should provide .new' do
    DataMapper::Query.should respond_to(:new)
  end

  describe '.new' do
    describe 'should set the attribute' do
      it '#model with model' do
        query = DataMapper::Query.new(@repository, Article)
        query.model.should == Article
      end

      GOOD_OPTIONS.each do |(attribute,value)|
        it "##{attribute} with options[:#{attribute}] if it is #{value.inspect}" do
          query = DataMapper::Query.new(@repository, Article, attribute => value)
          query.send(attribute == :reload ? :reload? : attribute).should == value
        end
      end

      describe ' #conditions with options[:conditions]' do
        it 'when they are a Hash' do
          query = DataMapper::Query.new(@repository, Article, :conditions => { :author => 'dkubb' })
          query.conditions.should == [ [ :eql, Article.properties[:author], 'dkubb' ] ]
        end

        it 'when they have a one element Array' do
          query = DataMapper::Query.new(@repository, Article, :conditions => [ 'name = "dkubb"' ])
          query.conditions.should == [ [ :raw, 'name = "dkubb"' ] ]
          query.bind_values.should == []
        end

        it 'when they have a two or more element Array' do
          bind_values = %w[ dkubb ]
          query = DataMapper::Query.new(@repository, Article, :conditions => [ 'name = ?', *bind_values ])
          query.conditions.should == [ [ :raw, 'name = ?', bind_values ] ]
          query.bind_values.should == bind_values

          bind_values = [ 'dkubb', 32 ]
          query = DataMapper::Query.new(@repository, Article, :conditions => [ 'name = ? OR age = ?', *bind_values ])
          query.conditions.should == [ [ :raw, 'name = ? OR age = ?', bind_values ] ]
          query.bind_values.should == bind_values

          bind_values = [ %w[ dkubb ssmoot ] ]
          query = DataMapper::Query.new(@repository, Article, :conditions => [ 'name IN ?', *bind_values ])
          query.conditions.should == [ [ :raw, 'name IN ?', bind_values ] ]
          query.bind_values.should == bind_values
        end

        it 'when they have another DM:Query as the value of sub-select' do
          class Acl
            include DataMapper::Resource
            property :id, Integer
            property :resource_id, Integer
          end

          acl_query = DataMapper::Query.new(@repository, Acl, :fields=>[:resource_id]) #this would normally have conditions
          query = DataMapper::Query.new(@repository, Article, :id.in => acl_query)
          query.conditions.each do |operator, property, value|
            operator.should == :in
            property.name.should == :id
            value.should == acl_query
          end
        end
      end

      describe ' #conditions with unknown options' do
        it 'when a Symbol object is a key' do
          query = DataMapper::Query.new(@repository, Article, :author => 'dkubb')
          query.conditions.should == [ [ :eql, Article.properties[:author], 'dkubb' ] ]
        end

        it 'when a Query::Operator object is a key' do
          query = DataMapper::Query.new(@repository, Article, :author.like => /\Ad(?:an\.)kubb\z/)
          query.conditions.should == [ [ :like, Article.properties[:author], /\Ad(?:an\.)kubb\z/ ] ]
        end
      end

      it '#order with model.default_order if none provided' do
        query = DataMapper::Query.new(@repository, Article)
        query.order.should == [ DataMapper::Query::Direction.new(Article.properties[:id], :asc) ]
      end
    end

    describe 'should raise an ArgumentError' do
      it 'when repository is nil' do
        lambda {
          DataMapper::Query.new(nil, NormalClass)
        }.should raise_error(ArgumentError)
      end

      it 'when model is nil' do
        lambda {
          DataMapper::Query.new(@repository, nil)
        }.should raise_error(ArgumentError)
      end

      it 'when model is a Class that does not include DataMapper::Resource' do
        lambda {
          DataMapper::Query.new(@repository, NormalClass)
        }.should raise_error(ArgumentError)
      end

      it 'when options is not a Hash' do
        lambda {
          DataMapper::Query.new(@repository, Article, nil)
        }.should raise_error(ArgumentError)
      end

      BAD_OPTIONS.each do |attribute,value|
        it "when options[:#{attribute}] is nil" do
          lambda {
            DataMapper::Query.new(@repository, Article, attribute => nil)
          }.should raise_error(ArgumentError)
        end

        it "when options[:#{attribute}] is #{value.kind_of?(Array) && value.empty? ? 'an empty Array' : value.inspect}" do
          lambda {
            DataMapper::Query.new(@repository, Article, attribute => value)
          }.should raise_error(ArgumentError)
        end
      end

      it 'when unknown options use something that is not a Query::Operator, Symbol or String is a key' do
        lambda {
          DataMapper::Query.new(@repository, Article, nil => nil)
        }.should raise_error(ArgumentError)
      end
    end

    describe 'should normalize' do
      it '#fields' do
        DataMapper::Query.new(@repository, Article, :fields => [:id]).fields.should == Article.properties.slice(:id)
      end
    end

    describe 'should translate custom types' do
      before(:each) do
        class Acl
          include DataMapper::Resource
          property :id, Integer
          property :is_custom_type, DM::Boolean
        end
      end
      it "should call Boolean#dump for :is_custom_type options" do
        DM::Boolean.should_receive(:dump).with(:false, Acl.properties[:is_custom_type])
        DataMapper::Query.new(@repository, Acl, :is_custom_type => :false)
      end
    end
  end

  it 'should provide #update' do
    @query.should respond_to(:update)
  end

  describe '#update' do
    before do
      @query = DataMapper::Query.new(@repository, Article, UPDATED_OPTIONS)
    end

    it 'should instantiate a DataMapper::Query object from other when it is a Hash' do
      other = { :reload => :true }

      @query.should_receive(:class).with(no_args).exactly(3).times.ordered.and_return(DataMapper::Query)
      DataMapper::Query.should_receive(:new).with(@repository, @query.model, other).ordered.and_return(@query)

      @query.update(other)
    end

    it 'should raise an ArgumentError if other query model is different' do
      lambda {
        other = DataMapper::Query.new(@repository, Comment)
        @query.update(other)
      }.should raise_error(ArgumentError)
    end

    it 'should return self' do
      other = DataMapper::Query.new(@repository, Article)
      @query.update(other).should == @query
    end

    describe 'should overwrite the attribute' do
      it '#reload? with other reload?' do
        other = DataMapper::Query.new(@repository, Article, :reload => true)
        @query.update(other).reload?.should == true
      end

      it '#offset with other offset when it is not equal to 0' do
        other = DataMapper::Query.new(@repository, Article, :offset => 1)
        @query.update(other).offset.should == 1
      end

      it '#limit with other limit when it is not nil' do
        other = DataMapper::Query.new(@repository, Article, :limit => 1)
        @query.update(other).limit.should == 1
      end

      it '#the operator if condition is the same and operater is changed (:not / :eql)' do
        # especially needed for collection#update where you might do something like:
        # all(:name.not => "John").update(:name => "John")
        pending do
          other = DataMapper::Query.new(@repository, Article, :author.not => "dkubb")
          @query.update(other).conditions.should == [ [ :not, Article.properties[:author], 'dkubb' ] ]
          @query.update(:author => "dkubb").conditions.should == [ [ :eql, Article.properties[:author], 'dkubb' ] ]
        end
      end

      [ :eql, :like ].each do |operator|
        it "#conditions with other conditions when updating the '#{operator}' clause to a different value than in self" do
          # set the initial conditions
          @query.update(:author.send(operator) => 'ssmoot')

          # update the conditions, and overwrite with the new value
          other = DataMapper::Query.new(@repository, Article, :author.send(operator) => 'dkubb')
          @query.update(other).conditions.should == [ [ operator, Article.properties[:author], 'dkubb' ] ]
        end
      end

      [ :gt, :gte ].each do |operator|
        it "#conditions with other conditions when updating the '#{operator}' clause to a value less than in self" do
          # set the initial conditions
          @query.update(:created_at.send(operator) => Time.at(1))

          # update the conditions, and overwrite with the new value is less
          other = DataMapper::Query.new(@repository, Article, :created_at.send(operator) => Time.at(0))
          @query.update(other).conditions.should == [ [ operator, Article.properties[:created_at], Time.at(0) ] ]
        end
      end

      [ :lt, :lte ].each do |operator|
        it "#conditions with other conditions when updating the '#{operator}' clause to a value greater than in self" do
          # set the initial conditions
          @query.update(:created_at.send(operator) => Time.at(0))

          # update the conditions, and overwrite with the new value is more
          other = DataMapper::Query.new(@repository, Article, :created_at.send(operator) => Time.at(1))
          @query.update(other).conditions.should == [ [ operator, Article.properties[:created_at], Time.at(1) ] ]
        end
      end

      it "#order with other order unique values" do
        order = [
          DataMapper::Query::Direction.new(Article.properties[:created_at], :desc),
          DataMapper::Query::Direction.new(Article.properties[:author],     :desc),
          DataMapper::Query::Direction.new(Article.properties[:title],      :desc),
        ]

        other = DataMapper::Query.new(@repository, Article, :order => order)
        @query.update(other).order.should == order
      end

      # dkubb: I am not sure i understand the intent here. link now needs to be
      #       a DM::Assoc::Relationship or the name (Symbol or String) of an
      #       association on the Resource -- thx guyvdb
      #
      # NOTE: I have commented out :links in the GOOD_OPTIONS above
      #
      [ :links, :includes ].each do |attribute|
        it "##{attribute} with other #{attribute} unique values" do
          pending 'DataMapper::Query::Path not ready'
          other = DataMapper::Query.new(@repository, Article, attribute => [ :stub, :other, :new ])
          @query.update(other).send(attribute).should == [ :stub, :other, :new ]
        end
      end

      it "#fields with other fields unique values" do
        other = DataMapper::Query.new(@repository, Article, :fields => [ :blog_id ])
        @query.update(other).fields.should == Article.properties.slice(:blog_id)
      end

      it '#conditions with other conditions when they are unique' do
        # set the initial conditions
        @query.update(:title => 'On DataMapper')

        # update the conditions, but merge the conditions together
        other = DataMapper::Query.new(@repository, Article, :author => 'dkubb')
        @query.update(other).conditions.should == [ [ :eql, Article.properties[:title], 'On DataMapper' ], [ :eql, Article.properties[:author], 'dkubb' ] ]
      end

      [ :not, :in ].each do |operator|
        it "#conditions with other conditions when updating the '#{operator}' clause" do
          # set the initial conditions
          @query.update(:created_at.send(operator) => [ Time.at(0) ])

          # update the conditions, and overwrite with the new value is more
          other = DataMapper::Query.new(@repository, Article, :created_at.send(operator) => [ Time.at(1) ])
          @query.update(other).conditions.should == [ [ operator, Article.properties[:created_at], [ Time.at(0), Time.at(1) ] ] ]
        end
      end

      it '#conditions with other conditions when they have a one element condition' do
        # set the initial conditions
        @query.update(:title => 'On DataMapper')

        # update the conditions, but merge the conditions together
        other = DataMapper::Query.new(@repository, Article, :conditions => [ 'author = "dkubb"' ])
        @query.update(other).conditions.should == [ [ :eql, Article.properties[:title], 'On DataMapper' ], [ :raw, 'author = "dkubb"' ] ]
      end

      it '#conditions with other conditions when they have a two or more element condition' do
        # set the initial conditions
        @query.update(:title => 'On DataMapper')

        # update the conditions, but merge the conditions together
        other = DataMapper::Query.new(@repository, Article, :conditions => [ 'author = ?', 'dkubb' ])
        @query.update(other).conditions.should == [ [ :eql, Article.properties[:title], 'On DataMapper' ], [ :raw, 'author = ?', [ 'dkubb' ] ] ]
      end
    end

    describe 'should not update the attribute' do
      it '#offset when other offset is equal to 0' do
        other = DataMapper::Query.new(@repository, Article, :offset => 0)
        other.offset.should == 0
        @query.update(other).offset.should == 1
      end

      it '#limit when other limit is nil' do
        other = DataMapper::Query.new(@repository, Article)
        other.limit.should be_nil
        @query.update(other).offset.should == 1
      end

      [ :gt, :gte ].each do |operator|
        it "#conditions with other conditions when they have a '#{operator}' clause with a value greater than in self" do
          # set the initial conditions
          @query.update(:created_at.send(operator) => Time.at(0))

          # do not overwrite with the new value if it is more
          other = DataMapper::Query.new(@repository, Article, :created_at.send(operator) => Time.at(1))
          @query.update(other).conditions.should == [ [ operator, Article.properties[:created_at], Time.at(0) ] ]
        end
      end

      [ :lt, :lte ].each do |operator|
        it "#conditions with other conditions when they have a '#{operator}' clause with a value less than in self" do
          # set the initial conditions
          @query.update(:created_at.send(operator) => Time.at(1))

          # do not overwrite with the new value if it is less
          other = DataMapper::Query.new(@repository, Article, :created_at.send(operator) => Time.at(0))
          @query.update(other).conditions.should == [ [ operator, Article.properties[:created_at], Time.at(1) ] ]
        end
      end
    end
  end

  it 'should provide #merge' do
    @query.should respond_to(:merge)
  end

  describe '#merge' do
    it 'should pass arguments as-is to duplicate object\'s #update method' do
      dupe_query = @query.dup
      @query.should_receive(:dup).with(no_args).ordered.and_return(dupe_query)
      dupe_query.should_receive(:update).with(:author => 'dkubb').ordered
      @query.merge(:author => 'dkubb')
    end

    it 'should return the duplicate object' do
      dupe_query = @query.merge(:author => 'dkubb')
      @query.object_id.should_not == dupe_query.object_id
      @query.merge(:author => 'dkubb').should == dupe_query
    end
  end

  it 'should provide #==' do
    @query.should respond_to(:==)
  end

  describe '#==' do
    describe 'should be equal' do
      it 'when other is same object' do
        @query.update(:author => 'dkubb').should == @query
      end

      it 'when other has the same attributes' do
        other = DataMapper::Query.new(@repository, Article)
        @query.object_id.should_not == other.object_id
        @query.should == other
      end

      it 'when other has the same conditions sorted differently' do
        @query.update(:author => 'dkubb')
        @query.update(:title  => 'On DataMapper')

        other = DataMapper::Query.new(@repository, Article, :title => 'On DataMapper')
        other.update(:author => 'dkubb')

        # query conditions are in different order
        @query.conditions.should == [ [ :eql, Article.properties[:author], 'dkubb'         ], [ :eql, Article.properties[:title],  'On DataMapper' ] ]
        other.conditions.should  == [ [ :eql, Article.properties[:title],  'On DataMapper' ], [ :eql, Article.properties[:author], 'dkubb'         ] ]

        @query.should == other
      end
    end

    describe 'should be different' do
      it 'when other model is different than self.model' do
        @query.should_not == DataMapper::Query.new(@repository, Comment)
      end

      UPDATED_OPTIONS.each do |attribute,value|
        it "when other #{attribute} is different than self.#{attribute}" do
          @query.should_not == DataMapper::Query.new(@repository, Article, attribute => value)
        end
      end

      it 'when other conditions are different than self.conditions' do
        @query.should_not == DataMapper::Query.new(@repository, Article, :author => 'dkubb')
      end
    end
  end

  it 'should provide #reverse' do
    @query.should respond_to(:reverse)
  end

  describe '#reverse' do
    it 'should create a duplicate query and delegate to #reverse!' do
      copy = @query.dup
      copy.should_receive(:reverse!).with(no_args).and_return(@query)
      @query.should_receive(:dup).with(no_args).and_return(copy)

      @query.reverse.should == @query
    end
  end

  it 'should provide #reverse!' do
    @query.should respond_to(:reverse!)
  end

  describe '#reverse!' do
    it 'should update the query with the reverse order' do
      normal_order  = Article.key.map { |p| DataMapper::Query::Direction.new(p, :asc) }
      reverse_order = Article.key.map { |p| DataMapper::Query::Direction.new(p, :desc) }

      normal_order.should_not be_empty
      reverse_order.should_not be_empty

      @query.order.should == normal_order
      @query.should_receive(:update).with(:order => reverse_order)
      @query.reverse!.object_id.should == @query.object_id
    end
  end

  describe 'inheritance properties' do
    before(:each) do
      class Parent
        include DataMapper::Resource
        property :id, Serial
        property :type, Discriminator
      end
      @query = DataMapper::Query.new(@repository, Parent)
      @other_query = DataMapper::Query.new(@repository, Article)
    end

    it 'should provide #inheritance_property' do
      @query.should respond_to(:inheritance_property)
    end

    describe '#inheritance_property' do
      it 'should return a Property object if there is a Discriminator field' do
        @query.inheritance_property.should be_kind_of(DataMapper::Property)
        @query.inheritance_property.name.should == :type
        @query.inheritance_property.type.should == DM::Discriminator
      end

      it 'should return nil if there is no Discriminator field' do
        @other_query.inheritance_property.should be_nil
      end
    end

    it 'should provide #inheritance_property_index' do
      @query.should respond_to(:inheritance_property_index)
    end

    describe '#inheritance_property_index' do
      it 'should return integer index if there is a Discriminator field' do
        @query.inheritance_property_index.should be_kind_of(Integer)
        @query.inheritance_property_index.should == 1
      end

      it 'should return nil if there is no Discriminator field'
    end
  end
end

describe DataMapper::Query::Operator do
  before do
    @operator = :thing.gte
  end

  it 'should provide #==' do
    @operator.should respond_to(:==)
  end

  describe '#==' do
    describe 'should be equal' do
      it 'when other is same object' do
        @operator.should == @operator
      end

      it 'when other has the same target and operator' do
        other = :thing.gte
        @operator.target.should == other.target
        @operator.operator.should == other.operator
        @operator.should == other
      end
    end

    describe 'should be different' do
      it 'when other class is not a descendant of self.class' do
        other = :thing
        other.class.should_not be_kind_of(@operator.class)
        @operator.should_not == other
      end

      it 'when other has a different target' do
        other = :other.gte
        @operator.target.should_not == other.target
        @operator.should_not == other
      end

      it 'when other has a different operator' do
        other = :thing.gt
        @operator.operator.should_not == other.operator
        @operator.should_not == other
      end
    end
  end
end
