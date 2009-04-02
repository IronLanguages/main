require File.expand_path(File.join(File.dirname(__FILE__), '..', 'spec_helper'))

if HAS_SQLITE3
  describe DataMapper::Associations do
    before :all do
      db1 = File.expand_path(File.join(File.dirname(__FILE__), "custom_db1_sqlite3.db"))
      db2 = File.expand_path(File.join(File.dirname(__FILE__), "custom_db2_sqlite3.db"))
      FileUtils.touch(db1)
      FileUtils.touch(db2)
      DataMapper.setup(:custom_db1, "sqlite3://#{db1}")
      DataMapper.setup(:custom_db2, "sqlite3://#{db2}")
      class ::CustomParent
        include DataMapper::Resource
        def self.default_repository_name
          :custom_db1
        end
        property :id, Serial
        property :name, String
        repository(:custom_db2) do
          has n, :custom_childs
        end
      end
      class ::CustomChild
        include DataMapper::Resource
        def self.default_repository_name
          :custom_db2
        end
        property :id, Serial
        property :name, String
        repository(:custom_db1) do
          belongs_to :custom_parent
        end
      end

    end
    before :each do
      [ CustomChild, CustomParent ].each { |m| m.auto_migrate! }

      parent = CustomParent.create(:name => "mother")
      child1 = parent.custom_childs.create(:name => "son")
      child2 = parent.custom_childs.create(:name => "daughter")

      @parent = CustomParent.first(:name => "mother")
      @child1 = CustomChild.first(:name => "son")
      @child2 = CustomChild.first(:name => "daughter")
    end
    it "should be able to handle has_many relationships to other repositories" do
      @parent.custom_childs.size.should == 2
      @parent.custom_childs.include?(@child1).should == true
      @parent.custom_childs.include?(@child2).should == true
      @parent.custom_childs.delete(@child1)
      @parent.custom_childs.save
      @parent.reload
      @parent.custom_childs.size.should == 1
      @parent.custom_childs.include?(@child2).should == true
    end
    it "should be able to handle belongs_to relationships to other repositories" do
      @child1.custom_parent.should == @parent
      @child2.custom_parent.should == @parent
      @child1.custom_parent = nil
      @child1.save
      @child1.reload
      @child1.custom_parent.should == nil
      @parent.reload
      @parent.custom_childs.size.should == 1
      @parent.custom_childs.include?(@child2).should == true
    end
  end
end

if ADAPTER
  repository(ADAPTER) do
    class ::Machine
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      has n, :areas
      has n, :fussy_areas, :class_name => 'Area', :rating.gte => 3, :type => 'particular'
    end

    class ::Area
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String
      property :rating, Integer
      property :type, String

      belongs_to :machine
    end

    class ::Pie
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      belongs_to :sky
    end

    class ::Sky
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      has 1, :pie
    end

    class ::Ultrahost
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      has n, :ultraslices, :order => [:id.desc]
    end

    class ::Ultraslice
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      belongs_to :ultrahost
    end

    class ::Node
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String

      has n, :children, :class_name => 'Node', :child_key => [ :parent_id ]
      belongs_to :parent, :class_name => 'Node', :child_key => [ :parent_id ]
    end

    class ::MadeUpThing
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :id, Serial
      property :name, String
      belongs_to :area
      belongs_to :machine
    end

    module ::Models
      class Project
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :title, String, :length => 255, :key => true
        property :summary, DataMapper::Types::Text

        has n, :tasks
        has 1, :goal
      end

      class Goal
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :title, String, :length => 255, :key => true
        property :summary, DataMapper::Types::Text

        belongs_to :project
      end

      class Task
        include DataMapper::Resource

        def self.default_repository_name
          ADAPTER
        end

        property :title, String, :length => 255, :key => true
        property :description, DataMapper::Types::Text

        belongs_to :project
      end
    end

    class ::Galaxy
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      property :name, String, :key => true, :length => 255
      property :size, Float,  :key => true, :precision => 15, :scale => 6
    end

    class ::Star
      include DataMapper::Resource

      def self.default_repository_name
        ADAPTER
      end

      belongs_to :galaxy
    end

  end

  describe DataMapper::Associations do
    describe 'namespaced associations' do
      before do
        Models::Project.auto_migrate!(ADAPTER)
        Models::Task.auto_migrate!(ADAPTER)
        Models::Goal.auto_migrate!(ADAPTER)
      end

      it 'should allow namespaced classes in parent and child for many <=> one' do
        m = Models::Project.new(:title => 'p1', :summary => 'sum1')
        m.tasks << Models::Task.new(:title => 't1', :description => 'desc 1')
        m.save

        t = Models::Task.first(:title => 't1')

        t.project.should_not be_nil
        t.project.title.should == 'p1'
        t.project.tasks.size.should == 1

        p = Models::Project.first(:title => 'p1')

        p.tasks.size.should == 1
        p.tasks[0].title.should == 't1'
      end

      it 'should allow namespaced classes in parent and child for one <=> one' do
        g = Models::Goal.new(:title => "g2", :summary => "desc 2")
        p = Models::Project.create(:title => "p2", :summary => "sum 2", :goal => g)

        pp = Models::Project.first(:title => 'p2')
        pp.goal.title.should == "g2"

        g = Models::Goal.first(:title => "g2")

        g.project.should_not be_nil
        g.project.title.should == 'p2'

        g.project.goal.should_not be_nil
      end
    end

    describe 'many to one associations' do
      before do
        Machine.auto_migrate!(ADAPTER)
        Area.auto_migrate!(ADAPTER)
        MadeUpThing.auto_migrate!(ADAPTER)

        machine1 = Machine.create(:name => 'machine1')
        machine2 = Machine.create(:name => 'machine2')
        area1   = Area.create(:name => 'area1', :machine => machine1)
        area2   = Area.create(:name => 'area2')
      end

      it '#belongs_to' do
        area = Area.new
        area.should respond_to(:machine)
        area.should respond_to(:machine=)
      end

      it 'should create the foreign key property immediately' do
        class ::Duck
          include DataMapper::Resource
          property :id, Serial
          belongs_to :sky
        end
        Duck.properties.slice(:sky_id).compact.should_not be_empty
        duck = Duck.new
        duck.should respond_to(:sky_id)
        duck.should respond_to(:sky_id=)
      end

      it 'should load without the parent'

      it 'should allow substituting the parent' do
        area1   = Area.first(:name => 'area1')
        machine2 = Machine.first(:name => 'machine2')

        area1.machine = machine2
        area1.save
        Area.first(:name => 'area1').machine.should == machine2
      end

      it 'should save both the object and parent if both are new' do
        area1 = Area.new(:name => 'area1')
        area1.machine = Machine.new(:name => 'machine1')
        area1.save
        area1.machine_id.should == area1.machine.id
      end

      it '#belongs_to with namespaced models' do
        repository(ADAPTER) do
          module ::FlightlessBirds
            class Ostrich
              include DataMapper::Resource
              property :id, Serial
              property :name, String
              belongs_to :sky # there's something sad about this :'(
            end
          end

          FlightlessBirds::Ostrich.properties(ADAPTER).slice(:sky_id).compact.should_not be_empty
        end
      end

      it 'should load the associated instance' do
        machine1 = Machine.first(:name => 'machine1')
        Area.first(:name => 'area1').machine.should == machine1
      end

      it 'should save the association key in the child' do
        machine2 = Machine.first(:name => 'machine2')

        Area.create(:name => 'area3', :machine => machine2)
        Area.first(:name => 'area3').machine.should == machine2
      end

      it 'should set the association key immediately' do
        machine = Machine.first(:name => 'machine1')
        Area.new(:machine => machine).machine_id.should == machine.id
      end

      it "should be able to set an association obtained from another association" do
        machine1 = Machine.first(:name => 'machine1')
        area1 = Area.first(:name => 'area1')
        area1.machine = machine1

        m = MadeUpThing.create(:machine => area1.machine, :name => "Weird")

        m.machine_id.should == machine1.id
      end

      it 'should save the parent upon saving of child' do
        e = Machine.new(:name => 'machine10')
        y = Area.create(:name => 'area10', :machine => e)

        y.machine.name.should == 'machine10'
        Machine.first(:name => 'machine10').should_not be_nil
      end

      it 'should set and retrieve associations on not yet saved objects' do
        e = Machine.create(:name => 'machine10')
        y = e.areas.build(:name => 'area10')

        y.machine.name.should == 'machine10'
      end

      it 'should convert NULL parent ids into nils' do
        Area.first(:name => 'area2').machine.should be_nil
      end

      it 'should save nil parents as NULL ids' do
        y1 = Area.create(:id => 20, :name => 'area20')
        y2 = Area.create(:id => 30, :name => 'area30', :machine => nil)

        y1.id.should == 20
        y1.machine.should be_nil
        y2.id.should == 30
        y2.machine.should be_nil
      end

      it 'should respect length on foreign keys' do
        property = Star.relationships[:galaxy].child_key[:galaxy_name]
        property.length.should == 255
      end

      it 'should respect precision and scale on foreign keys' do
        property = Star.relationships[:galaxy].child_key[:galaxy_size]
        property.precision.should == 15
        property.scale.should == 6
      end

      it 'should be reloaded when calling Resource#reload' do
        e = Machine.new(:name => 'machine40')
        y = Area.create(:name => 'area40', :machine => e)

        y.send(:machine_association).should_receive(:reload).once

        lambda { y.reload }.should_not raise_error
      end

      it "should have machine when created using machine_id" do
        m = Machine.create(:name => 'machineX')
        a = Area.new(:machine_id => m.id)
        a.machine.should == m
      end

      it "should not have a machine when orphaned" do
        a = Area.new(:machine_id => 42)
        a.machine.should be_nil
      end
    end

    describe 'one to one associations' do
      before do
        Sky.auto_migrate!(ADAPTER)
        Pie.auto_migrate!(ADAPTER)

        pie1 = Pie.create(:name => 'pie1')
        pie2 = Pie.create(:name => 'pie2')
        sky1 = Sky.create(:name => 'sky1', :pie => pie1)
      end

      it '#has 1' do
        s = Sky.new
        s.should respond_to(:pie)
        s.should respond_to(:pie=)
      end

      it 'should allow substituting the child' do
        sky1 = Sky.first(:name => 'sky1')
        pie1 = Pie.first(:name => 'pie1')
        pie2 = Pie.first(:name => 'pie2')

        sky1.pie.should == pie1
        pie2.sky.should be_nil

        sky1.pie = pie2
        sky1.save

        pie2.sky.should == sky1
        pie1.reload.sky.should be_nil
      end

      it 'should load the associated instance' do
        sky1 = Sky.first(:name => 'sky1')
        pie1 = Pie.first(:name => 'pie1')

        sky1.pie.should == pie1
      end

      it 'should save the association key in the child' do
        pie2 = Pie.first(:name => 'pie2')

        sky2 = Sky.create(:id => 2, :name => 'sky2', :pie => pie2)
        pie2.sky.should == sky2
      end

      it 'should save the children upon saving of parent' do
        p = Pie.new(:id => 10, :name => 'pie10')
        s = Sky.create(:id => 10, :name => 'sky10', :pie => p)

        p.sky.should == s

        Pie.first(:name => 'pie10').should_not be_nil
      end

      it 'should save nil parents as NULL ids' do
        p1 = Pie.create(:id => 20, :name => 'pie20')
        p2 = Pie.create(:id => 30, :name => 'pie30', :sky => nil)

        p1.id.should == 20
        p1.sky.should be_nil
        p2.id.should == 30
        p2.sky.should be_nil
      end

      it 'should be reloaded when calling Resource#reload' do
        pie = Pie.first(:name => 'pie1')
        pie.send(:sky_association).should_receive(:reload).once
        lambda { pie.reload }.should_not raise_error
      end
    end

    describe 'one to many associations' do
      before do
        Ultrahost.auto_migrate!(ADAPTER)
        Ultraslice.auto_migrate!(ADAPTER)
        Machine.auto_migrate!(ADAPTER)
        Area.auto_migrate!(ADAPTER)

        ultrahost1  = Ultrahost.create(:name => 'ultrahost1')
        ultrahost2  = Ultrahost.create(:name => 'ultrahost2')
        ultraslice1 = Ultraslice.create(:name => 'ultraslice1', :ultrahost => ultrahost1)
        ultraslice2 = Ultraslice.create(:name => 'ultraslice2', :ultrahost => ultrahost1)
        ultraslice3 = Ultraslice.create(:name => 'ultraslice3')
      end

      it '#has n' do
        h = Ultrahost.new
        h.should respond_to(:ultraslices)
      end

      it 'should allow removal of a child through a loaded association' do
        ultrahost1  = Ultrahost.first(:name => 'ultrahost1')
        ultraslice2 = ultrahost1.ultraslices.first

        ultrahost1.ultraslices.size.should == 2
        ultrahost1.ultraslices.delete(ultraslice2)
        ultrahost1.ultraslices.size.should == 1

        ultraslice2 = Ultraslice.first(:name => 'ultraslice2')
        ultraslice2.ultrahost.should_not be_nil

        ultrahost1.save

        ultraslice2.reload.ultrahost.should be_nil
      end

      it 'should use the IdentityMap correctly' do
        repository(ADAPTER) do
          ultrahost1 = Ultrahost.first(:name => 'ultrahost1')

          ultraslice =  ultrahost1.ultraslices.first
          ultraslice2 = ultrahost1.ultraslices(:order => [:id]).last # should be the same as 1
          ultraslice3 = Ultraslice.get(2) # should be the same as 1

          ultraslice.object_id.should == ultraslice2.object_id
          ultraslice.object_id.should == ultraslice3.object_id
        end
      end

      it '#<< should add exactly the parameters' do
        machine = Machine.new(:name => 'my machine')
        4.times do |i|
          machine.areas << Area.new(:name => "area nr #{i}")
        end
        machine.save
        machine.areas.size.should == 4
        4.times do |i|
          machine.areas.any? do |area|
            area.name == "area nr #{i}"
          end.should == true
        end
        machine = Machine.get!(machine.id)
        machine.areas.size.should == 4
        4.times do |i|
          machine.areas.any? do |area|
            area.name == "area nr #{i}"
          end.should == true
        end
      end

      it "#<< should add the correct number of elements if they are created" do
        machine = Machine.create(:name => 'my machine')
        4.times do |i|
          machine.areas << Area.create(:name => "area nr #{i}", :machine => machine)
        end
        machine.areas.size.should == 4
      end

      it "#build should add exactly one instance of the built record" do
        machine = Machine.create(:name => 'my machine')

        original_size = machine.areas.size
        machine.areas.build(:name => "an area", :machine => machine)

        machine.areas.size.should == original_size + 1
      end

      it '#<< should add default values for relationships that have conditions' do
        # it should add default values
        machine = Machine.new(:name => 'my machine')
        machine.fussy_areas << Area.new(:name => 'area 1', :rating => 4 )
        machine.save
        Area.first(:name => 'area 1').type.should == 'particular'
        # it should not add default values if the condition's property already has a value
        machine.fussy_areas << Area.new(:name => 'area 2', :rating => 4, :type => 'not particular')
        machine.save
        Area.first(:name => 'area 2').type.should == 'not particular'
        # it should ignore non :eql conditions
        machine.fussy_areas << Area.new(:name => 'area 3')
        machine.save
        Area.first(:name => 'area 3').rating.should == nil
      end

      it 'should load the associated instances, in the correct order' do
        ultrahost1 = Ultrahost.first(:name => 'ultrahost1')

        ultrahost1.ultraslices.should_not be_nil
        ultrahost1.ultraslices.size.should == 2
        ultrahost1.ultraslices.first.name.should == 'ultraslice2' # ordered by [:id.desc]
        ultrahost1.ultraslices.last.name.should == 'ultraslice1'

        ultraslice3 = Ultraslice.first(:name => 'ultraslice3')

        ultraslice3.ultrahost.should be_nil
      end

      it 'should add and save the associated instance' do
        ultrahost1 = Ultrahost.first(:name => 'ultrahost1')
        ultrahost1.ultraslices << Ultraslice.new(:id => 4, :name => 'ultraslice4')
        ultrahost1.save

        Ultraslice.first(:name => 'ultraslice4').ultrahost.should == ultrahost1
      end

      it 'should not save the associated instance if the parent is not saved' do
        h = Ultrahost.new(:id => 10, :name => 'ultrahost10')
        h.ultraslices << Ultraslice.new(:id => 10, :name => 'ultraslice10')

        Ultraslice.first(:name => 'ultraslice10').should be_nil
      end

      it 'should save the associated instance upon saving of parent' do
        h = Ultrahost.new(:id => 10, :name => 'ultrahost10')
        h.ultraslices << Ultraslice.new(:id => 10, :name => 'ultraslice10')
        h.save

        s = Ultraslice.first(:name => 'ultraslice10')

        s.should_not be_nil
        s.ultrahost.should == h
      end

      it 'should save the associated instances upon saving of parent when mass-assigned' do
        h = Ultrahost.create(:id => 10, :name => 'ultrahost10', :ultraslices => [ Ultraslice.new(:id => 10, :name => 'ultraslice10') ])

        s = Ultraslice.first(:name => 'ultraslice10')

        s.should_not be_nil
        s.ultrahost.should == h
      end

      it 'should have finder-functionality' do
        h = Ultrahost.first(:name => 'ultrahost1')

        h.ultraslices.should have(2).entries

        s = h.ultraslices.all(:name => 'ultraslice2')

        s.should have(1).entries
        s.first.id.should == 2

        h.ultraslices.first(:name => 'ultraslice2').should == s.first
      end

      it 'should be reloaded when calling Resource#reload' do
        ultrahost = Ultrahost.first(:name => 'ultrahost1')
        ultrahost.send(:ultraslices_association).should_receive(:reload).once
        lambda { ultrahost.reload }.should_not raise_error
      end
    end

    describe 'many-to-one and one-to-many associations combined' do
      before do
        Node.auto_migrate!(ADAPTER)

        Node.create(:name => 'r1')
        Node.create(:name => 'r2')
        Node.create(:name => 'r1c1',   :parent_id => 1)
        Node.create(:name => 'r1c2',   :parent_id => 1)
        Node.create(:name => 'r1c3',   :parent_id => 1)
        Node.create(:name => 'r1c1c1', :parent_id => 3)
      end

      it 'should properly set #parent' do
        r1 = Node.get 1
        r1.parent.should be_nil

        n3 = Node.get 3
        n3.parent.should == r1

        n6 = Node.get 6
        n6.parent.should == n3
      end

      it 'should properly set #children' do
        r1 = Node.get(1)
        off = r1.children
        off.size.should == 3
        off.include?(Node.get(3)).should be_true
        off.include?(Node.get(4)).should be_true
        off.include?(Node.get(5)).should be_true
      end

      it 'should allow to create root nodes' do
        r = Node.create(:name => 'newroot')
        r.parent.should be_nil
        r.children.size.should == 0
      end

      it 'should properly delete nodes' do
        r1 = Node.get 1

        r1.children.size.should == 3
        r1.children.delete(Node.get(4))
        r1.save
        Node.get(4).parent.should be_nil
        r1.children.size.should == 2
      end
    end

    describe 'through-associations' do
      before :all do
        repository(ADAPTER) do
          module ::Sweets
            class Shop
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              has n, :cakes                                 # has n
              has n, :recipes,     :through => :cakes       # has n => has 1
              has n, :ingredients, :through => :cakes       # has n => has 1 => has n
              has n, :creators,    :through => :cakes       # has n => has 1 => has 1
              has n, :ultraslices,      :through => :cakes       # has n => has n
              has n, :bites,       :through => :cakes       # has n => has n => has n
              has n, :shapes,      :through => :cakes       # has n => has n => has 1
              has n, :customers,   :through => :cakes       # has n => belongs_to (pending)
              has 1, :shop_owner                            # has 1
              has 1, :wife,        :through => :shop_owner  # has 1 => has 1
              has 1, :ring,        :through => :shop_owner  # has 1 => has 1 => has 1
              has n, :coats,       :through => :shop_owner  # has 1 => has 1 => has n
              has n, :children,    :through => :shop_owner  # has 1 => has n
              has n, :toys,        :through => :shop_owner  # has 1 => has n => has n
              has n, :boogers,     :through => :shop_owner  # has 1 => has n => has 1
            end

            class ShopOwner
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :shop
              has 1, :wife
              has n, :children
              has n, :toys,     :through => :children
              has n, :boogers,  :through => :children
              has n, :coats,    :through => :wife
              has 1, :ring,     :through => :wife
              has n, :schools,  :through => :children
            end

            class Wife
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :shop_owner
              has 1, :ring
              has n, :coats
            end

            class Coat
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :wife
            end

            class Ring
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :wife
            end

            class Child
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :shop_owner
              has n, :toys
              has 1, :booger
            end

            class Booger
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :child
            end

            class Toy
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :child
            end

            class Cake
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :shop
              belongs_to :customer
              has n, :ultraslices
              has n, :bites,       :through => :ultraslices
              has 1, :recipe
              has n, :ingredients, :through => :recipe
              has 1, :creator,     :through => :recipe
              has n, :shapes,      :through => :ultraslices
            end

            class Recipe
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :cake
              has n, :ingredients
              has 1, :creator
            end

            class Customer
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              has n, :cakes
            end

            class Creator
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :recipe
            end

            class Ingredient
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :recipe
            end

            class Ultraslice
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :size, Integer
              belongs_to :cake
              has n, :bites
              has 1, :shape
            end

            class Shape
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :ultraslice
            end

            class Bite
              include DataMapper::Resource
              def self.default_repository_name
                ADAPTER
              end
              property :id, Serial
              property :name, String
              belongs_to :ultraslice
            end

            DataMapper::Resource.descendants.each do |descendant|
              descendant.auto_migrate!(ADAPTER) if descendant.name =~ /^Sweets::/
            end

            betsys = Shop.new(:name => "Betsy's")
            betsys.save

            #
            # has n
            #

            german_chocolate = Cake.new(:name => 'German Chocolate')
            betsys.cakes << german_chocolate
            german_chocolate.save
            short_cake = Cake.new(:name => 'Short Cake')
            betsys.cakes << short_cake
            short_cake.save

            # has n => belongs_to

            old_customer = Customer.new(:name => 'John Johnsen')
            old_customer.cakes << german_chocolate
            old_customer.cakes << short_cake
            german_chocolate.save
            short_cake.save
            old_customer.save

            # has n => has 1

            schwarzwald = Recipe.new(:name => 'Schwarzwald Cake')
            schwarzwald.save
            german_chocolate.recipe = schwarzwald
            german_chocolate.save
            shortys_special = Recipe.new(:name => "Shorty's Special")
            shortys_special.save
            short_cake.recipe = shortys_special
            short_cake.save

            # has n => has 1 => has 1

            runar = Creator.new(:name => 'Runar')
            schwarzwald.creator = runar
            runar.save
            berit = Creator.new(:name => 'Berit')
            shortys_special.creator = berit
            berit.save

            # has n => has 1 => has n

            4.times do |i| schwarzwald.ingredients << Ingredient.new(:name => "Secret ingredient nr #{i}") end
            6.times do |i| shortys_special.ingredients << Ingredient.new(:name => "Well known ingredient nr #{i}") end

            # has n => has n

            10.times do |i| german_chocolate.ultraslices << Ultraslice.new(:size => i) end
            5.times do |i| short_cake.ultraslices << Ultraslice.new(:size => i) end
            german_chocolate.ultraslices.size.should == 10
            # has n => has n => has 1

            german_chocolate.ultraslices.each do |ultraslice|
              shape = Shape.new(:name => 'square')
              ultraslice.shape = shape
              shape.save
            end
            short_cake.ultraslices.each do |ultraslice|
              shape = Shape.new(:name => 'round')
              ultraslice.shape = shape
              shape.save
            end

            # has n => has n => has n
            german_chocolate.ultraslices.each do |ultraslice|
              6.times do |i|
                ultraslice.bites << Bite.new(:name => "Big bite nr #{i}")
              end
            end
            short_cake.ultraslices.each do |ultraslice|
              3.times do |i|
                ultraslice.bites << Bite.new(:name => "Small bite nr #{i}")
              end
            end

            #
            # has 1
            #

            betsy = ShopOwner.new(:name => 'Betsy')
            betsys.shop_owner = betsy
            betsys.save

            # has 1 => has 1

            barry = Wife.new(:name => 'Barry')
            betsy.wife = barry
            barry.save

            # has 1 => has 1 => has 1

            golden = Ring.new(:name => 'golden')
            barry.ring = golden
            golden.save

            # has 1 => has 1 => has n

            3.times do |i|
              barry.coats << Coat.new(:name => "Fancy coat nr #{i}")
            end
            barry.save

            # has 1 => has n

            5.times do |i|
              betsy.children << Child.new(:name => "Snotling nr #{i}")
            end
            betsy.save

            # has 1 => has n => has n

            betsy.children.each do |child|
              4.times do |i|
                child.toys << Toy.new(:name => "Cheap toy nr #{i}")
              end
              child.save
            end

            # has 1 => has n => has 1

            betsy.children.each do |child|
              booger = Booger.new(:name => 'Nasty booger')
              child.booger = booger
              child.save
            end
          end
        end
      end

      #
      # has n
      #

      it 'should return the right children for has n => has n relationships' do
        Sweets::Shop.first.ultraslices.size.should == 15
        10.times do |i|
          Sweets::Shop.first.ultraslices.select do |ultraslice|
            ultraslice.cake == Sweets::Cake.first(:name => 'German Chocolate') && ultraslice.size == i
          end
        end
      end

      it 'should return the right children for has n => has n => has 1' do
        Sweets::Shop.first.shapes.size.should == 15
        Sweets::Shop.first.shapes.select do |shape|
          shape.name == 'square'
        end.size.should == 10
        Sweets::Shop.first.shapes.select do |shape|
          shape.name == 'round'
        end.size.should == 5
      end

      it 'should return the right children for has n => has n => has n' do
        Sweets::Shop.first.bites.size.should == 75
        Sweets::Shop.first.bites.select do |bite|
          bite.ultraslice.cake == Sweets::Cake.first(:name => 'German Chocolate')
        end.size.should == 60
        Sweets::Shop.first.bites.select do |bite|
          bite.ultraslice.cake == Sweets::Cake.first(:name => 'Short Cake')
        end.size.should == 15
      end

      it 'should return the right children for has n => belongs_to relationships' do
        Sweets::Customer.first.cakes.size.should == 2
        customers = Sweets::Shop.first.customers.select do |customer|
          customer.name == 'John Johnsen'
        end
        customers.size.should == 1
      end

      it 'should return the right children for has n => has 1 relationships' do
        Sweets::Shop.first.recipes.size.should == 2
        Sweets::Shop.first.recipes.select do |recipe|
          recipe.name == 'Schwarzwald Cake'
        end.size.should == 1
        Sweets::Shop.first.recipes.select do |recipe|
          recipe.name == "Shorty's Special"
        end.size.should == 1
      end

      it 'should return the right children for has n => has 1 => has 1 relationships' do
        Sweets::Shop.first.creators.size.should == 2
        Sweets::Shop.first.creators.any? do |creator|
          creator.name == 'Runar'
        end.should == true
        Sweets::Shop.first.creators.any? do |creator|
          creator.name == 'Berit'
        end.should == true
      end

      it 'should return the right children for has n => has 1 => has n relationships' do
        Sweets::Shop.first.ingredients.size.should == 10
        4.times do |i|
          Sweets::Shop.first.ingredients.any? do |ingredient|
            ingredient.name == "Secret ingredient nr #{i}" && ingredient.recipe.cake == Sweets::Cake.first(:name => 'German Chocolate')
          end.should == true
        end
        6.times do |i|
          Sweets::Shop.first.ingredients.any? do |ingredient|
            ingredient.name == "Well known ingredient nr #{i}" && ingredient.recipe.cake == Sweets::Cake.first(:name => 'Short Cake')
          end.should == true
        end
      end

      #
      # has 1
      #

      it 'should return the right children for has 1 => has 1 relationships' do
        Sweets::Shop.first.wife.should == Sweets::Wife.first
      end

      it 'should return the right children for has 1 => has 1 => has 1 relationships' do
        Sweets::Shop.first.ring.should == Sweets::Ring.first
      end

      it 'should return the right children for has 1 => has 1 => has n relationships' do
        Sweets::Shop.first.coats.size.should == 3
        3.times do |i|
          Sweets::Shop.first.coats.any? do |coat|
            coat.name == "Fancy coat nr #{i}"
          end.should == true
        end
      end

      it 'should return the right children for has 1 => has n relationships' do
        Sweets::Shop.first.children.size.should == 5
        5.times do |i|
          Sweets::Shop.first.children.any? do |child|
            child.name == "Snotling nr #{i}"
          end.should == true
        end
      end

      it 'should return the right children for has 1 => has n => has 1 relationships' do
        Sweets::Shop.first.boogers.size.should == 5
        Sweets::Shop.first.boogers.inject(Set.new) do |sum, booger|
          sum << booger.child_id
        end.size.should == 5
      end

      it 'should return the right children for has 1 => has n => has n relationships' do
        Sweets::Shop.first.toys.size.should == 20
        5.times do |child_nr|
          4.times do |toy_nr|
            Sweets::Shop.first.toys.any? do |toy|
              toy.name == "Cheap toy nr #{toy_nr}" && toy.child = Sweets::Child.first(:name => "Snotling nr #{child_nr}")
            end.should == true
          end
        end
      end

      #
      # misc
      #

      it 'should join tables in the right order during has 1 => has n => has 1 queries' do
        child = Sweets::Shop.first.children(:name => 'Snotling nr 3').booger(:name.like => 'Nasty booger')
        child.should_not be_nil
        child.size.should eql(1)
        child.first.name.should eql("Nasty booger")
      end

      it 'should join tables in the right order for belongs_to relations' do
        wife = Sweets::Wife.first(Sweets::Wife.shop_owner.name => "Betsy", Sweets::Wife.shop_owner.shop.name => "Betsy's")
        wife.should_not be_nil
        wife.name.should eql("Barry")
      end

      it 'should raise exception if you try to change it' do
        lambda do
          Sweets::Shop.first.wife = Sweets::Wife.new(:name => 'Larry')
        end.should raise_error(DataMapper::Associations::ImmutableAssociationError)
      end

      it 'should be reloaded when calling Resource#reload' do
        betsys = Sweets::Shop.first(:name => "Betsy's")
        betsys.send(:customers_association).should_receive(:reload).once
        lambda { betsys.reload }.should_not raise_error
      end

    end

    if false # Many to many not yet implemented
    describe "many to many associations" do
      before(:all) do
        class ::RightItem
          include DataMapper::Resource

          def self.default_repository_name
            ADAPTER
          end

          property :id, Serial
          property :name, String

          has n..n, :left_items
        end

        class ::LeftItem
          include DataMapper::Resource

          def self.default_repository_name
            ADAPTER
          end

          property :id, Serial
          property :name, String

          has n..n, :right_items
        end

        RightItem.auto_migrate!
        LeftItem.auto_migrate!
      end

      def create_item_pair(number)
        @ri = RightItem.new(:name => "ri#{number}")
        @li = LeftItem.new(:name => "li#{number}")
      end

      it "should add to the association from the left" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0000"
          @ri.save; @li.save
          @ri.should_not be_new_record
          @li.should_not be_new_record

          @li.right_items << @ri
          @li.right_items.should include(@ri)
          @li.reload
          @ri.reload
          @li.right_items.should include(@ri)
        end
      end

      it "should add to the association from the right" do
        create_item_pair "0010"
        @ri.save; @li.save
        @ri.should_not be_new_record
        @li.should_not be_new_record

        @ri.left_items << @li
        @ri.left_items.should include(@li)
        @li.reload
        @ri.reload
        @ri.left_items.should include(@li)
      end

      it "should load the associated collection from the either side" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0020"
          @ri.save; @li.save
          @ri.left_items << @li
          @ri.reload; @li.reload

          @ri.left_items.should include(@li)
          @li.right_items.should include(@ri)
        end
      end

      it "should load the associated collection from the right" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0030"
          @ri.save; @li.save
          @li.right_items << @li
          @ri.reload; @li.reload

          @ri.left_items.should include(@li)
          @li.right_items.should include(@ri)
        end
      end

      it "should save the left side of the association if new record" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0040"
          @ri.save
          @li.should be_new_record
          @ri.left_items << @li
          @li.should_not be_new_record
        end
      end

      it "should save the right side of the association if new record" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0050"
          @li.save
          @ri.should be_new_record
          @li.right_items << @ri
          @ri.should_not be_new_record
        end
      end

      it "should save both side of the association if new record" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0060"
          @li.should be_new_record
          @ri.should be_new_record
          @ri.left_items << @li
          @ri.should_not be_new_record
          @li.should_not be_new_record
        end
      end

      it "should remove an item from the left collection without destroying the item" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0070"
          @li.save; @ri.save
          @ri.left_items << @li
          @ri.reload; @li.reload
          @ri.left_items.should include(@li)
          @ri.left_items.delete(@li)
          @ri.left_items.should_not include(@li)
          @li.reload
          LeftItem.get(@li.id).should_not be_nil
        end
      end

      it "should remove an item from the right collection without destroying the item" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0080"
          @li.save; @ri.save
          @li.right_items << @ri
          @li.reload; @ri.reload
          @li.right_items.should include(@ri)
          @li.right_items.delete(@ri)
          @li.right_items.should_not include(@ri)
          @ri.reload
          RightItem.get(@ri.id).should_not be_nil
        end
      end

      it "should remove the item from the collection when an item is deleted" do
        pending "Waiting on Many To Many to be implemented" do
          create_item_pair "0090"
          @li.save; @ri.save
          @ri.left_items << @li
          @ri.reload; @li.reload
          @ri.left_items.should include(@li)
          @li.destroy
          @ri.reload
          @ri.left_items.should_not include(@li)
        end
      end
    end
  end
  end
end
