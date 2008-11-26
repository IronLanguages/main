p ARGV.object_id == $*.object_id

ARGV = []          # warning

p ARGV.object_id == $*.object_id

$foo = [1,2,3]
alias $* $foo 

p ARGV     # ignores alias


p '-' * 50

p DATA.class
p DATA

p RUBY_PLATFORM
p RUBY_RELEASE_DATE
p RUBY_VERSION

p PLATFORM
p RELEASE_DATE
p VERSION

p TOPLEVEL_BINDING

SCRIPT_LINES__ = {}
require "Require.1.rb"
p SCRIPT_LINES__

__END__
foo
bar