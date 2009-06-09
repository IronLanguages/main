# encoding: utf-8

require 'pathname'
require 'iconv'
require Pathname(__FILE__).dirname.parent.expand_path + 'spec_helper'

describe DataMapper::Types::Slug do

  before(:all) do
    class ::SlugTest
      include DataMapper::Resource

      property :id, Serial
      property :name, Slug

    end
    SlugTest.auto_migrate!
  end

  it "should create the permalink" do
    repository(:default) do
      SlugTest.create(:name => 'New DataMapper Type')
    end

    SlugTest.first.name.should == "new-datamapper-type"
  end

  it "should find by a slug" do
    repository(:default) do
      SlugTest.create(:name => "This Should Be a Slug")
    end
    slug = "this-should-be-a-slug"

    slugged = SlugTest.first(:name => slug)
    slugged.should_not be_nil
    slugged.name.should == slug
  end

  [
    ["Iñtërnâtiônàlizætiøn",      "internationalizaetion" ],
    ["Hello World",               "hello-world"],
    ["This is Dan's Blog",        "this-is-dans-blog"],
    ["This is My Site, and Blog", "this-is-my-site-and-blog"]
  ].each do |name, slug|

      it "should sluggify #{name}" do
        repository(:default) do
          SlugTest.create(:name => name)
        end
        SlugTest.first(:name => slug).should_not be_nil
      end
    end



end
