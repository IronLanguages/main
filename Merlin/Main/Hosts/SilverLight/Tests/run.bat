@echo off
echo Updated dlr.js
echo --------------
ruby %~dp0Scripts\gen_dlrjs.rb

echo Making dlr.xap folder
echo -----------------
if not exist %~dp0dlr (mkdir %~dp0dlr)

pushd %~dp0
ruby %~dp0..\Scripts\run_tests.rb
popd
