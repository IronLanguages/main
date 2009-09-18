@echo off
echo Updated dlr.js
echo --------------
ruby %~dp0Scripts\gen_dlrjs.rb
pushd %~dp0
ruby %~dp0..\Scripts\run_tests.rb
popd
