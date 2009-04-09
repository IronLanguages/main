require 'rubygems'
require 'spec'

require File.dirname(__FILE__) + '/../lib/github'

class Module
  def metaclass
    class << self;self;end
  end
end

class Spec::NextInstanceProxy
  def initialize
    @deferred = []
  end

  def method_missing(sym, *args)
    proxy = Spec::NextInstanceProxy.new
    @deferred << [sym, args, proxy]
    proxy
  end

  def should_receive(*args)
    method_missing(:should_receive, *args)
  end
  alias stub! should_receive

  def invoke(obj)
    @deferred.each do |(sym, args, proxy)|
      result = obj.send(sym, *args)
      proxy.invoke(result)
    end
  end
end

class Class
  def next_instance
    meth = metaclass.instance_method(:new)
    proxy = Spec::NextInstanceProxy.new
    metaclass.send :define_method, :new do |*args|
      instance = meth.bind(self).call(*args)
      proxy.invoke(instance)
      metaclass.send :define_method, :new, meth
      instance
    end
    proxy
  end
end

module Spec::Example::ExampleGroupSubclassMethods
  def add_guard(klass, name, is_class = false)
    guarded = nil # define variable now for scoping
    target = (is_class ? klass.metaclass : klass)
    sep = (is_class ? "." : "#")
    target.class_eval do
      guarded = instance_method(name)
      define_method name do |*args|
        raise "Testing guards violated: Cannot call #{klass}#{sep}#{name}"
      end
    end
    @guards ||= []
    @guards << [klass, name, is_class, guarded]
  end

  def add_class_guard(klass, name)
    add_guard(klass, name, true)
  end

  def unguard(klass, name, is_class = false)
    row = @guards.find { |(k,n,i)| k == klass and n == name and i == is_class }
    raise "#{klass}#{is_class ? "." : "#"}#{name} is not guarded" if row.nil?
    (is_class ? klass.metaclass : klass).class_eval do
      define_method name, row.last
    end
    @guards.delete row
  end

  def class_unguard(klass, name)
    unguard(klass, name, true)
  end

  def unguard_all
    @guards ||= []
    @guards.each do |klass, name, is_class, guarded|
      (is_class ? klass.metaclass : klass).class_eval do
        define_method name, guarded
      end
    end
    @guards.clear
  end
end

# prevent the use of `` in tests
Spec::Runner.configure do |configuration|
  # load this here so it's covered by the `` guard
  configuration.prepend_before(:all) do
    module GitHub
      load 'helpers.rb'
      load 'commands.rb'
    end
  end

  configuration.prepend_after(:each) do
    GitHub.instance_variable_set :'@options', nil
    GitHub.instance_variable_set :'@debug', nil
  end

  configuration.prepend_before(:all) do
    self.class.send :include, Spec::Example::ExampleGroupSubclassMethods
  end

  configuration.prepend_before(:each) do
    add_guard Kernel, :`
    add_guard Kernel, :system
    add_guard Kernel, :fork
    add_guard Kernel, :exec
    add_class_guard Process, :fork
  end

  configuration.append_after(:each) do
    unguard_all
  end
end

# include this in any example group that defines @helper
module SetupMethods
  def setup_user_and_branch(user = :user, branch = :master)
    @helper.should_receive(:user_and_branch).any_number_of_times.and_return([user, branch])
  end

  def setup_url_for(remote = :origin, user = nil, project = :project)
    if user.nil?
      user = remote
      user = "user" if remote == :origin
    end
    @helper.should_receive(:url_for).any_number_of_times.with(remote).and_return("git://github.com/#{user}/#{project}.git")
  end
end
