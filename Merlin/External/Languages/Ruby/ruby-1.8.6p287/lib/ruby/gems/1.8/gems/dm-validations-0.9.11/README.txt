This is a DataMapper plugin that provides validations for DataMapper model
classes.

== Setup
DataMapper validation capabilities are automatically available when you
'require dm-validations' into your application.

More specifically, DataMapper::Validate is automatically included into
DataMapper::Resource when you require dm-validations.

== Specifying Model Validations
There are two primary ways to implement validations for your models:

1) Placing validation methods with properties as params in your class
   definitions like:
   - validates_length :name
   - validates_length [:name, :description]

2) Using auto-validations, please see DataMapper::Validate::AutoValidate

An example class with validations declared:

  require 'dm-validations'

  class Account
    include DataMapper::Resource

    property :name, String
    validates_length :name
  end

See all of the DataMapper::Validate module's XYZValidator(s) to learn about the
complete collections of validators available to you.

== Validating
DataMapper validations, when included, alter the default save/create/update
process for a model.  Unless you specify a context the resource must be
valid in the :default context before saving.

You may manually validate a resource using the valid? method, which will
return true if the resource is valid, and false if it is invalid.

In addition to the valid? method, there is also an all_valid? method that
recursively walks both the current object and its associated objects and returns
its true/false result for the entire walk.

== Working with Validation Errors
If your validators find errors in your model, they will populate the
DataMapper::Validate::ValidationErrors object that is available through each of
your models via calls to your model's errors method.

For example:

  my_account = Account.new(:name => "Jose")
  if my_account.save
    # my_account is valid and has been saved
  else
    my_account.errors.each do |e|
      puts e
    end
  end

See DataMapper::Validate::ValidationErrors for all you can do with your model's
errors method.

== Contextual Validations

DataMapper Validations also provide a means of grouping your validations into
contexts. This enables you to run different sets of validations under ...
different contexts.

TO BE ADDED... For now, see
