set CONFIG=Debug
set DIR=%CONFIG%

%MERLIN_ROOT%\bin\Debug\ir rewrite.rb /cd:%MERLIN_ROOT%\bin\%DIR% @Assemblies.txt /config:%CONFIG% /key:%MERLIN_ROOT%\Support\MSSharedLibKey.snk /out:%MERLIN_ROOT%\bin\R%DIR%