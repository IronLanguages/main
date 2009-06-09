dir = Pathname(__FILE__).dirname.expand_path / 'adapters'

require dir / 'abstract_adapter'
require dir / 'in_memory_adapter'

# NOTE: this is a temporary work-around to the load error problems,
# and is better fixed in dm-core/next.  The main reason the fix is
# not applied in dm-core/master is because the change is non-trivial.

%w[ data_objects sqlite3 mysql postgres ].each do |gem|
  begin
    require dir / "#{gem}_adapter"
  rescue LoadError, Gem::Exception
    # ignore it
  end
end
