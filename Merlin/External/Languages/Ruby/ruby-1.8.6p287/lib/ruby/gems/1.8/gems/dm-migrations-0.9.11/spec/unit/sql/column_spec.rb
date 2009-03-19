require 'pathname'
require Pathname(__FILE__).dirname + '../../spec_helper'

require Pathname(__FILE__).dirname + '../../../lib/sql/column'

describe SQL::Column do
  before do
    @column = SQL::Column.new
  end

  %w{name type not_null default_value primary_key unique}.each do |meth|
    it "should have a ##{meth} attribute" do
      @column.should respond_to(meth.intern)
    end
  end

end
