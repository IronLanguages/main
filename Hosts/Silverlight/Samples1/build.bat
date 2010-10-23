@echo off

echo Creating dlr directory
if not exist %~dp0dlr mkdir %~dp0dlr
if not exist %~dp0dlr\dlr mkdir %~dp0dlr\dlr

echo Generating dlr.js
ruby %~dp0..\Scripts\generate_dlrjs.rb > %~dp0generate_dlrjs.log
del %~dp0generate_dlrjs.log
copy %~dp0..\Scripts\dlr.js %~dp0 > %~dp0copylog.log
del %~dp0copylog.log
