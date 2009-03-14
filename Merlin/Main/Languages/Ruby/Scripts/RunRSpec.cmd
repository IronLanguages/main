@echo off
setlocal

if "%1"=="-?" (
    echo Example usages:
    echo     RunRSpec array\collect_spec.rb
    echo     RunRSpec ..Library\complex\abs_spec.rb
    echo     RunRSpec -ruby -e "returns an instance of Array" array\allocate_spec.rb
    echo You can also use:
    echo     rake why_regression array collect
    goto END:
)

if "%1"=="-ruby" (
    set RUBY_CMD=%RUBY18_EXE%
    REM set RUBY_EXE=%RUBY18_EXE%
    REM set MSPEC_RUNNER=1
    shift
    goto RUN_RUBY_CMD
)

if "%1"=="-ruby19" (
    set RUBY_CMD=%RUBY19_EXE%
    set RUBY_EXE=%RUBY19_EXE%
    REM set MSPEC_RUNNER=1
    shift
    goto RUN_RUBY_CMD
)

set RUBY_CMD=%MERLIN_ROOT%\bin\Debug\ir.exe
set RUBY_CMD_OPTS=-X:Interpret
set EXCL_TAGS=--excl-tag fails --excl-tag critical

:RUN_RUBY_CMD

if not exist %RUBY_CMD% (
    echo RunRSpec could not find %RUBY_CMD%
    goto END:
)

if "%1" == "-e" (
    set EXAMPLE_STR=--example %2
    shift
    shift
)

pushd %MERLIN_ROOT%\..\External\Languages\IronRuby\mspec

@echo on

%RUBY_CMD% %RUBY_CMD_OPTS% mspec\bin\mspec-run -fd --verbose %EXAMPLE_STR% %EXCL_TAGS% --config default.mspec rubyspec/core/%1

@popd

:END
