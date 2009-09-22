@echo off

echo Updated dlr.js
echo --------------
ruby %~dp0Scripts\gen_dlrjs.rb
echo Making dlr directory
echo --------------------
if not exist %~dp0dlr (
    mkdir %~dp0dlr
    echo DONE
) else (
    echo Already exists
)
