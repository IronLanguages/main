require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

ADAPTERS.each do |adapter|
  describe 'DataMapper::Constraints' do
    before do
      DataMapper::Repository.adapters[:default] =  DataMapper::Repository.adapters[adapter]

      class ::Stable
        include DataMapper::Resource
        include DataMapper::Constraints

        property :id,       Serial
        property :location, String
        property :size,     Integer

        has n, :cows
      end

      class ::Farmer
        include DataMapper::Resource
        include DataMapper::Constraints

        property :first_name, String, :key => true
        property :last_name,  String, :key => true

        has n, :cows
      end

      class ::Cow
        include DataMapper::Resource
        include DataMapper::Constraints

        property :id,    Serial
        property :name,  String
        property :breed, String

        belongs_to :stable
        belongs_to :farmer
      end

      #Used to test a belongs_to association with no has() association
      #on the other end
      class ::Pig
        include DataMapper::Resource
        include DataMapper::Constraints

        property :id,   Serial
        property :name, String

        belongs_to :farmer
      end

      #Used to test M:M :through => Resource relationships
      class ::Chicken
        include DataMapper::Resource
        include DataMapper::Constraints

        property :id,   Serial
        property :name, String

        has n, :tags, :through => Resource
      end

      class ::Tag
        include DataMapper::Resource
        include DataMapper::Constraints

        property :id,     Serial
        property :phrase, String

        has n, :chickens, :through => Resource
      end

      DataMapper.auto_migrate!
    end # before

    it "is included when DataMapper::Constraints is loaded" do
      Cow.new.should be_kind_of(DataMapper::Constraints)
    end

    it "should be able to create related objects with a foreign key constraint" do
      @s  = Stable.create(:location => "Hometown")
      @c1 = Cow.create(:name => "Bea", :stable => @s)
    end

    it "should be able to create related objects with a composite foreign key constraint" do
      @f  = Farmer.create(:first_name => "John", :last_name => "Doe")
      @c1 = Cow.create(:name => "Bea", :farmer => @f)
    end

    it "should not be able to create related objects with a failing foreign key constraint" do
      s = Stable.create
      lambda { @c1 = Cow.create(:name => "Bea", :stable_id => s.id + 1) }.should raise_error
    end

    describe "belongs_to without matching has association" do
      before do
        @f1 = Farmer.create(:first_name => "John", :last_name => "Doe")
        @f2 = Farmer.create(:first_name => "Some", :last_name => "Body")
        @p = Pig.create(:name => "Bea", :farmer => @f2)
      end
      it "should destroy the parent if there are no children in the association" do
        @f1.destroy.should == true
      end

      it "the child should be destroyable" do
        @p.destroy.should == true
      end

    end

    describe "constraint options" do

      describe "when no constraint options are given" do

        it "should destroy the parent if there are no children in the association" do
          @f1 = Farmer.create(:first_name => "John", :last_name => "Doe")
          @f2 = Farmer.create(:first_name => "Some", :last_name => "Body")
          @c1 = Cow.create(:name => "Bea", :farmer => @f2)
          @f1.destroy.should == true
        end

        it "should not destroy the parent if there are children in the association" do
          @f = Farmer.create(:first_name => "John", :last_name => "Doe")
          @c1 = Cow.create(:name => "Bea", :farmer => @f)
          @f.destroy.should == false
        end

      end

      describe "when :constraint => :protect is given" do
        before do
          class ::Farmer
            has n, :cows, :constraint => :protect
            has 1, :pig, :constraint => :protect
          end
          class ::Pig
            belongs_to :farmer
          end
          class ::Cow
            belongs_to :farmer
          end
          class ::Chicken
            has n, :tags, :through => Resource, :constraint => :protect
          end
          class ::Tag
            has n, :chickens, :through => Resource, :constraint => :protect
          end
        end

        describe "one-to-one associations" do
          before do
            @f1 = Farmer.create(:first_name => "Mary", :last_name => "Smith")
            @p1 = Pig.create(:name => "Morton",:farmer => @f1)
          end

          it "should not destroy the parent if there are children in the association" do
            @f1.destroy.should == false
          end

          it "the child should be destroyable" do
            @p1.destroy.should == true
          end
        end

        describe "one-to-many associations" do
          before do
            @f1 = Farmer.create(:first_name => "John", :last_name => "Doe")
            @f2 = Farmer.create(:first_name => "Some", :last_name => "Body")
            @c1 = Cow.create(:name => "Bea", :farmer => @f2)
          end

          it "should destroy the parent if there are no children in the association" do
            @f1.destroy.should == true
          end

          it "should not destroy the parent if there are children in the association" do
            @f2.destroy.should == false
          end

          it "the child should be destroyable" do
            @c1.destroy.should == true
          end
        end

        describe "many-to-many associations" do
          before do
            @t1   = Tag.create(:phrase => "silly chicken")
            @t2   = Tag.create(:phrase => "serious chicken")
            @chk1 = Chicken.create(:name =>"Frank the Chicken", :tags => [@t2])
          end

          it "should destroy the parent if there are no children in the association" do
            @t1.destroy.should == true
          end

          it "should not destroy the parent if there are children in the association" do
            @t2.destroy.should == false
          end

          it "the child should be destroyable" do
            @chk1.tags.clear
            @chk1.save.should == true
            @chk1.tags.should be_empty
          end
        end

      end # when constraint protect is given

      describe "when :constraint => :destroy! is given" do
        before do
          class ::Farmer
            has n, :cows, :constraint => :destroy!
          end
          class ::Cow
            belongs_to :farmer
          end
          class ::Chicken
            has n, :tags, :through => Resource, :constraint => :destroy!
          end
          class ::Tag
            has n, :chickens, :through => Resource, :constraint => :destroy!
          end

          DataMapper.auto_migrate!
        end

        describe "one-to-many associations" do
          before(:each) do
            @f = Farmer.create(:first_name => "John", :last_name => "Doe")
            @c1 = Cow.create(:name => "Bea", :farmer => @f)
            @c2 = Cow.create(:name => "Riksa", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
          end

          it "should destroy the children" do
            @f.destroy
            @f.cows.all? { |c| c.should be_new_record }
          end

          it "the child should be destroyable" do
            @c1.destroy.should == true
          end

        end

        describe "many-to-many associations" do
          before do
            @t1   = Tag.create(:phrase => "floozy")
            @t2   = Tag.create(:phrase => "dirty")
            @chk1 = Chicken.create(:name => "Nancy Chicken", :tags => [@t1, @t2])
          end

          it "should destroy! the parent and the children, too" do
            @chk1.destroy.should == true
            @chk1.should be_new_record

            # @t1 & @t2 should still exist, the chicken_tags should have been deleted
            ChickenTag.all.should be_empty
            @t1.should_not be_new_record
            @t2.should_not be_new_record
          end

          it "the child should be destroyable" do
            @chk1.destroy.should == true
          end
        end

      end # when :constraint => :destroy! is given

      describe "when :constraint => :destroy is given" do
        before do
          class ::Farmer
            has n, :cows, :constraint => :destroy
            has 1, :pig, :constraint => :destroy
          end
          class ::Cow
            belongs_to :farmer
          end
          class ::Chicken
            has n, :tags, :through => Resource, :constraint => :destroy
          end
          class ::Tag
            has n, :chickens, :through => Resource, :constraint => :destroy
          end

          DataMapper.auto_migrate!
        end

        describe "one-to-one associations" do
          before do
            @f = Farmer.create(:first_name => "Ted", :last_name => "Cornhusker")
            @p = Pig.create(:name => "BaconBits", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
          end

          it "should destroy the children" do
            pig = @f.pig
            @f.destroy
            pig.should be_new_record
          end

          it "the child should be destroyable" do
            @p.destroy.should == true
          end
        end

        describe "one-to-many associations" do
          before(:each) do
            @f = Farmer.create(:first_name => "John", :last_name => "Doe")
            @c1 = Cow.create(:name => "Bea", :farmer => @f)
            @c2 = Cow.create(:name => "Riksa", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
          end

          it "should destroy the children" do
            @f.destroy
            @f.cows.all? { |c| c.should be_new_record }
            @f.should be_new_record
          end

          it "the child should be destroyable" do
            @c1.destroy.should == true
          end

        end

        describe "many-to-many associations" do
          before do
            @t1   = Tag.create :phrase => "floozy"
            @t2   = Tag.create :phrase => "dirty"
            @chk1 = Chicken.create :name => "Nancy Chicken", :tags => [@t1,@t2]
          end

          it "should destroy the parent and the children, too" do
            @chk1.destroy.should == true
            @chk1.should be_new_record

            #@t1 & @t2 should still exist, the chicken_tags should have been deleted
            ChickenTag.all.should be_empty
            @t1.should_not be_new_record
            @t2.should_not be_new_record
          end

          it "the child should be destroyable" do
            @chk1.destroy.should == true
          end
        end

      end # when :constraint => :destroy is given

      describe "when :constraint => :set_nil is given" do
        before do
          class ::Farmer
            has n, :cows, :constraint => :set_nil
            has 1, :pig, :constraint => :set_nil
          end
          class ::Cow
            belongs_to :farmer
          end
          # NOTE: M:M Relationships are not supported,
          # see "when checking constraint types" tests at bottom
          DataMapper.auto_migrate!
        end

        describe "one-to-one associations" do
          before do
            @f = Farmer.create(:first_name => "Mr", :last_name => "Hands")
            @p = Pig.create(:name => "Greasy", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
          end

          it "should set the child's foreign_key id to nil" do
            pig = @f.pig
            @f.destroy.should == true
            pig.farmer.should be_nil
          end

          it "the child should be destroyable" do
            @p.destroy.should == true
          end

        end

        describe "one-to-many associations" do
          before(:each) do
            @f = Farmer.create(:first_name => "John", :last_name => "Doe")
            @c1 = Cow.create(:name => "Bea", :farmer => @f)
            @c2 = Cow.create(:name => "Riksa", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
          end

          it "should set the foreign_key ids of children to nil" do
            @f.destroy
            @f.cows.all? { |c| c.farmer.should be_nil }
          end

          it "the children should be destroyable" do
            @c1.destroy.should == true
            @c2.destroy.should == true
          end

        end

      end # describe "when :constraint => :set_nil is given" do

      describe "when :constraint => :skip is given" do
        before do
          class ::Farmer
            has n, :cows, :constraint => :skip
            has 1, :pig, :constraint => :skip
          end
          class ::Cow
            belongs_to :farmer
          end
          class ::Chicken
            has n, :tags, :through => Resource, :constraint => :skip
          end
          class ::Tag
            has n, :chickens, :through => Resource, :constraint => :skip
          end
          DataMapper.auto_migrate!
        end

        describe "one-to-one associations" do
          before do
            @f = Farmer.create(:first_name => "William", :last_name => "Shepard")
            @p  = Pig.create(:name => "Jiggles The Pig", :farmer => @f)
          end

          it "should let the parent be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
            # @p.farmer.should be_new_record
          end

          it "should let the children become orphan records" do
            @f.destroy
            @p.farmer.should be_new_record
          end

          it "the child should be destroyable" do
            @p.destroy.should == true
          end

        end

        describe "one-to-many associations" do
          before do
            @f = Farmer.create(:first_name => "John", :last_name => "Doe")
            @c1 = Cow.create(:name => "Bea", :farmer => @f)
            @c2 = Cow.create(:name => "Riksa", :farmer => @f)
          end

          it "should let the parent to be destroyed" do
            @f.destroy.should == true
            @f.should be_new_record
          end

          it "should let the children become orphan records" do
            @f.destroy
            @c1.farmer.should be_new_record
            @c2.farmer.should be_new_record
          end

          it "the children should be destroyable" do
            @c1.destroy.should == true
            @c2.destroy.should == true
          end

        end

        describe "many-to-many associations" do
          before do
            @t = Tag.create(:phrase => "Richard Pryor's Chicken")
            @chk = Chicken.create(:name => "Delicious", :tags => [@t])
          end

          it "the children should be destroyable" do
            @chk.destroy.should == true
          end
        end

      end # describe "when :constraint => :skip is given"

      describe "when checking constraint types" do

        #M:M relationships results in a join table composed of a two part primary key
        # setting a portion of the primary key is not possible for two reasons:
        # 1. the columns are defined as :nullable => false
        # 2. there could be duplicate rows if more than one of either of the types
        #   was deleted while being associated to the same type on the other side of the relationshp
        #   Given
        #   Turkey(Name: Ted, ID: 1) =>
        #       Tags[Tag(Phrase: awesome, ID: 1), Tag(Phrase: fat, ID: 2)]
        #   Turkey(Name: Steve, ID: 2) =>
        #       Tags[Tag(Phrase: awesome, ID: 1), Tag(Phrase: flamboyant, ID: 3)]
        #
        #   Table turkeys_tags would look like (turkey_id, tag_id)
        #     (1, 1)
        #     (1, 2)
        #     (2, 1)
        #     (2, 3)
        #
        #   If both turkeys were deleted and pk was set null
        #     (null, 1)
        #     (null, 2)
        #     (null, 1) #at this time there would be a duplicate row error
        #     (null, 3)
        #
        #   I would suggest setting :constraint to :skip in this scenario which will leave
        #     you with orphaned rows.
        it "should raise an error if :set_nil is given for a M:M relationship" do
          lambda{
            class ::Chicken
              has n, :tags, :through => Resource, :constraint => :set_nil
            end
            class ::Tag
              has n, :chickens, :through => Resource, :constraint => :set_nil
            end
          }.should raise_error(ArgumentError)
        end

        # Resource#destroy! is not suppored in dm-core
        it "should raise an error if :destroy! is given for a 1:1 relationship" do
          lambda do
            class ::Farmer
              has 1, :pig, :constraint => :destroy!
            end
          end.should raise_error(ArgumentError)
        end

        it "should raise an error if an unknown type is given" do
          lambda do
            class ::Farmer
              has n, :cows, :constraint => :chocolate
            end
          end.should raise_error(ArgumentError)
        end

      end

    end # describe 'constraint options'

  end # DataMapper::Constraints
end # ADAPTERS.each
