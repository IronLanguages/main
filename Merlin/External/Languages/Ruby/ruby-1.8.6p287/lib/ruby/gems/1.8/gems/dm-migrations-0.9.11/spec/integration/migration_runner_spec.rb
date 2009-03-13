require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

ADAPTERS.each do |adapter|
  describe "Using Adapter #{adapter}, " do
    describe 'empty migration runner' do
      it "should return an empty array if no migrations have been defined" do
        migrations.should be_kind_of(Array)
        migrations.should have(0).item
      end
    end
    describe 'migration runnner' do
      # set up some 'global' setup and teardown tasks
      before(:each) do
        migration( 1, :create_people_table) { }
      end

      after(:each) do
        migrations.clear
      end

      describe '#migration' do

        it 'should create a new migration object, and add it to the list of migrations' do
          migrations.should be_kind_of(Array)
          migrations.should have(1).item
          migrations.first.name.should == "create_people_table"
        end

        it 'should allow multiple migrations to be added' do
          migration( 2, :add_dob_to_people) { }
          migration( 2, :add_favorite_pet_to_people) { }
          migration( 3, :add_something_else_to_people) { }
          migrations.should have(4).items
        end

        it 'should raise an error on adding with a duplicated name' do
          lambda { migration( 1, :create_people_table) { } }.should raise_error(RuntimeError, /Migration name conflict/)
        end

      end

      describe '#migrate_up! and #migrate_down!' do
        before(:each) do
          migration( 2, :add_dob_to_people) { }
          migration( 2, :add_favorite_pet_to_people) { }
          migration( 3, :add_something_else_to_people) { }
        end

        it 'calling migrate_up! should migrate up all the migrations' do
          # add our expectation that migrate_up should be called
          migrations.each do |m|
            m.should_receive(:perform_up)
          end
          migrate_up!
        end

        it 'calling migrate_up! with an arguement should only migrate to that level' do
          migrations.each do |m|
            if m.position <= 2
              m.should_receive(:perform_up)
            else
              m.should_not_receive(:perform_up)
            end
          end
          migrate_up!(2)
        end

        it 'calling migrate_down! should migrate down all the migrations' do
          # add our expectation that migrate_up should be called
          migrations.each do |m|
            m.should_receive(:perform_down)
          end
          migrate_down!
        end

      end
    end
  end
end
