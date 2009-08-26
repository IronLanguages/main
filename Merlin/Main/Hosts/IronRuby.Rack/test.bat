@echo off

echo Running tests:

set GEM_PATH=C:\Ruby\lib\ruby\gems\1.8
set GEM_HOME=C:\Ruby\lib\ruby\gems\1.8
rmdir /S /Q %~dp0perf
mkdir %~dp0perf
pushd %~dp0perf

echo IronRuby Rack ...
%~dp0..\..\bin\release\ir.exe %~dp0test.rb rack > ironruby-rack.txt

echo IronRuby Sinatra ...
%~dp0..\..\bin\release\ir.exe %~dp0test.rb sinatra > ironruby-sinatra.txt

echo IronRuby Rails ...
%~dp0..\..\bin\release\ir.exe %~dp0test.rb rails > ironruby-rails.txt

echo MRI Rack ...
ruby %~dp0test.rb rack > ruby-rack.txt

echo MRI Sinatra ...
ruby %~dp0test.rb sinatra > ruby-sinatra.txt

echo MRI Rails ...
ruby %~dp0test.rb rails > ruby-rails.txt

echo DONE. See the "perf" directory for results.
popd
