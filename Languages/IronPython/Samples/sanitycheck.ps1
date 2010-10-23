#This script just runs a few sanity checks on the 
#samples to ensure everything basically runs. You 
#should note that the 7th and 8th demo of the Winforms
#example are commented out as they require VS compilation.
#Also, the IronCube sample is commented out as this isn't 
#shipping yet.

#NOTES:
# - you must run this script from the "Samples" directory
# - if you're having problems running this script, try 
#   running "Set-ExecutionPolicy RemoteSigned" from a PS 
#   session. By default, PS Beta 1 does not allow any PS scripts
#   to be executed.
# - of course you must fulfill the prereqs of all the samples

param($IPY_DIR)

$MAIN_DIR = $PWD
set-alias IPY_CMD $IPY_DIR\ipy.exe

dir | Select-Object FullName
sleep 15


echo "----------------------------------------"
cd $MAIN_DIR\DynamicWebServiceHelpers
dir | Select-Object FullName
sleep 5

copy -force $IPY_DIR\*.dll .
sleep 5
&"$env:WINDIR\Microsoft.NET\Framework\v3.5\MSBuild.exe" sources\DynamicWebServiceHelpers.csproj
sleep 5

IPY_CMD bing.py
sleep 5
IPY_CMD flickr.py
sleep 5
IPY_CMD injectors.py
sleep 5
IPY_CMD mathservice.py
sleep 5
IPY_CMD rss.py
sleep 5
IPY_CMD stocks.py
sleep 5
IPY_CMD weather.py
sleep 5

echo "----------------------------------------"
cd $MAIN_DIR\ClrType
dir | Select-Object FullName
sleep 5

IPY_CMD sample.py
sleep 5

echo "----------------------------------------"
cd $MAIN_DIR\CommentChecker
dir | Select-Object FullName
sleep 5

IPY_CMD main.py misspelled.py

echo "----------------------------------------"
cd $MAIN_DIR\Direct3D
dir | Select-Object FullName
sleep 5

IPY_CMD checkpoints\checkpoint1.py
IPY_CMD checkpoints\checkpoint2.py
IPY_CMD checkpoints\checkpoint3.py
IPY_CMD checkpoints\checkpoint4.py
IPY_CMD checkpoints\checkpoint5.py
IPY_CMD checkpoints\checkpoint6.py

IPY_CMD demo1.py
IPY_CMD demo2.py
IPY_CMD demo3.py
IPY_CMD demo4.py
IPY_CMD GravityDemo.py
IPY_CMD MeshDemo.py
IPY_CMD tutorial.py

echo "----------------------------------------"
cd $MAIN_DIR\DiskUse
dir | Select-Object FullName
sleep 5

IPY_CMD app.py

echo "----------------------------------------"
cd $MAIN_DIR\FMsynth
dir | Select-Object FullName
sleep 5

IPY_CMD fmsynth.py

echo "----------------------------------------"
cd $MAIN_DIR\IPPowerShell
dir | Select-Object FullName
sleep 5

IPY_CMD minsysreq.py
sleep 5
IPY_CMD minsysreq_ps.py
sleep 5
IPY_CMD powershell.py
sleep 5

echo "----------------------------------------"
cd $MAIN_DIR\IronTunes
dir | Select-Object FullName
sleep 5

tlbimp $env:SystemRoot\System32\quartz.dll
sleep 5
IPY_CMD IronTunes.py

echo "----------------------------------------"
cd $MAIN_DIR\Puzzle
dir | Select-Object FullName
sleep 5

IPY_CMD -D puzzle.py

echo "----------------------------------------"
cd $MAIN_DIR\WinFormsMapPoint
dir | Select-Object FullName
sleep 5

cd WinForms

IPY_CMD formV1.py
IPY_CMD formV2.py
IPY_CMD formV3.py
IPY_CMD formv4.py
IPY_CMD formV5.py
IPY_CMD formv6.py
IPY_CMD formv7.py
IPY_CMD formv8.py

echo "----------------------------------------"
cd $MAIN_DIR