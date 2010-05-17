set CONFIG=Debug
set DIR=%CONFIG%

%DLR_ROOT%\bin\Debug\ir rewrite.rb /cd:%DLR_ROOT%\bin\%DIR% @Assemblies.txt /config:%CONFIG% /key:%DLR_ROOT%\Internal\MSSharedLibKey.snk /out:%DLR_ROOT%\bin\R%DIR%
