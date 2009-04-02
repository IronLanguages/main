require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

if HAS_SQLITE3 || HAS_MYSQL || HAS_POSTGRES
  describe 'DataMapper::Is::List' do

    class User
      include DataMapper::Resource

      property :id, Serial
      property :name, String

      has n, :todos
    end

    class Todo
      include DataMapper::Resource

      property :id, Serial
      property :title, String

      belongs_to :user

      is :list, :scope => [:user_id]
    end

    before :each do
      User.auto_migrate!(:default)
      Todo.auto_migrate!(:default)

      u1 = User.create(:name => "Johnny")
      Todo.create(:user => u1, :title => "Write down what is needed in a list-plugin")
      Todo.create(:user => u1, :title => "Complete a temporary version of is-list")
      Todo.create(:user => u1, :title => "Squash bugs in nested-set")

      u2 = User.create(:name => "Freddy")
      Todo.create(:user => u2, :title => "Eat tasty cupcake")
      Todo.create(:user => u2, :title => "Procrastinate on paid work")
      Todo.create(:user => u2, :title => "Go to sleep")

    end

    describe 'automatic positioning' do
      it 'should get the shadow variable of the last position' do
        repository(:default) do |repos|
          Todo.get(3).position=8
          Todo.get(3).dirty?.should == true
          Todo.get(3).attribute_dirty?(:position).should == true
          Todo.get(3).original_values[:position].should == 3
          Todo.get(3).list_scope.should == Todo.get(3).original_list_scope
        end
      end

      it 'should insert items into the list automatically' do
        repository(:default) do |repos|
          Todo.get(3).position.should == 3
          Todo.get(6).position.should == 3
        end
      end

      it 'should rearrange items when setting position yourself' do
        repository(:default) do |repos|
          todo = Todo.get(2)
          todo.position = 1
          todo.save

          Todo.get(2).position.should == 1
          Todo.get(1).position.should == 2
        end
      end

      it 'should rearrange items when setting the position yourself multiple times' do
        repository(:default) do |repos|
          todo = Todo.get(2)
          todo.position = 3
          todo.save

          Todo.get(2).position.should == 3
          Todo.get(3).position.should == 2

          todo = Todo.get(2)
          todo.position = 2
          todo.save

          Todo.get(2).position.should == 2
          Todo.get(3).position.should == 3
        end
      end
    end

    describe 'movement' do
      it 'should rearrange items correctly when moving :higher' do
        repository(:default) do |repos|
          Todo.get(3).move :higher
          Todo.get(4).position.should == 1
          Todo.get(3).position.should == 2
          Todo.get(2).position.should == 3
        end
      end

      it 'should rearrange items correctly when moving :lower' do
        repository(:default) do |repos|
          Todo.get(2).position.should == 2
          Todo.get(3).position.should == 3
          Todo.get(2).move :lower
          Todo.get(2).position.should == 3
          Todo.get(3).position.should == 2

          Todo.get(4).position.should == 1
        end
      end

      it 'should rearrange items correctly when moving :highest or :lowest' do
        repository(:default) do |repos|

          # list 1
          Todo.get(1).position.should == 1
          Todo.get(1).move(:lowest)
          Todo.get(1).position.should == 3

          # list 2
          Todo.get(6).position.should == 3
          Todo.get(6).move(:highest)
          Todo.get(6).position.should == 1
          Todo.get(5).position.should == 3
        end
      end

      it 'should not rearrange when trying to move top-item up, or bottom item down' do
        repository(:default) do |repos|
          Todo.get(4).position.should == 1
          Todo.get(4).move(:higher).should == false
          Todo.get(4).position.should == 1

          Todo.get(6).position.should == 3
          Todo.get(6).move(:lower).should == false
        end
      end

      it 'should rearrange items correctly when moving :above or :below' do
        repository(:default) do |repos|
          Todo.get(4).position.should == 1
          Todo.get(6).position.should == 3
          Todo.get(4).move(:below => Todo.get(6))
          Todo.get(4).position.should == 3
          Todo.get(6).position.should == 2
        end
      end
    end

    describe 'scoping' do
      it 'should detach from old list if scope changed' do
        repository(:default) do |repos|
          item = Todo.get(4)
          item.position.should == 1
          item.user_id = 1
          item.save

          item.list_scope.should != item.original_list_scope
          item.position.should == 1
          Todo.get(1).position.should == 2
          Todo.get(5).position.should == 1

          item.user_id = 2
          item.position = 1
          item.save

          item.position.should == 1
          Todo.get(5).position.should == 2

        end
      end

      it 'should not allow you to move item into another scope' do
        repository(:default) do |repos|
          item = Todo.get(1)
          item.position.should == 1
          item.move(:below => Todo.get(5)).should == false
        end
      end

      it 'should detach from list when deleted' do
        repository(:default) do |repos|
          item = Todo.get(4)
          item.position.should == 1
          Todo.get(5).position.should == 2
          item.destroy

          Todo.get(5).position.should == 1

        end
      end
    end

    describe 'reparation' do
      it 'should fix them lists' do
        repository(:default) do |repos|
          # Need to do this with a batch update, as setting position = 20 wont
          # work (it will set it to bottom of list, not more)
          Todo.all(:position => 3).update!(:position => 20)

          item = Todo.get(6)
          item.position.should == 20
          item.repair_list
          item.position.should == 3
        end
      end
    end

  end
end
