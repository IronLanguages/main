@echo off

call %~dp0build.bat

pushd %~dp0
ruby %~dp0..\Scripts\run_tests.rb
popd
