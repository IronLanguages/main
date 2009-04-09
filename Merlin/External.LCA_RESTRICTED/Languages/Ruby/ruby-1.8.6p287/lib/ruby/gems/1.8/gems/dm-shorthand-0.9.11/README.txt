= dm-shorthand

DataMapper plugin providing shortcut usage of models in multiple repositories.

When using this plugin, every time you define a new model M, a method with the same
name as the model is defined inside the module where you defined M.

== Example

Let's say you have repositories :default, :one and :two.

  class MyModel
    include DataMapper::Resource

    property :a, String
    property :b, String
  end

This will allow you to operate on those repositories like this:

  # create a new instance of MyModel in repository :one
  MyModel(:one).create(:a => "a's value!", :b => "b's value!")

  # fetch the MyModel instance with id == 1 from repository :two
  MyModel(:two)[1]

  # instantiate a new MyModel instance with its default repository
  # set to :default
  m = MyModel.new
