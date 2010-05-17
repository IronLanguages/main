@echo off

echo Updated dlr.js
echo --------------
ruby %~dp0Scripts\gen_dlrjs.rb
echo Making dlr directory
echo --------------------
if not exist %~dp0dlr (
    mkdir %~dp0dlr
    mkdir %~dp0dlr\dlr
    echo DONE
) else (
    if not exist %~dp0dlr\dlr mkdir %~dp0dlr\dlr
    echo Already exists
)
