param(
    [string]$name = "rubysync",
    [string]$server = "vstfdevdiv",
    [switch]$get
    )

$dir = "C:\vsl_ip26\rubysync"

if (-not (Test-Path $dir)) { 
    New-Item -Path $dir -ItemType directory | Out-Null
}

$dir = Resolve-Path $dir

Push-Location $dir
"Creating workspace $name..." | out-string
tf workspace /new /noprompt "/template:rubysync;jdeville" "/s:$server" $name

if ($LASTEXITCODE -ne 0) {
  Write-Error "Creating the workspace failed"
  exit-pushd
  exit
}
Push-Location $env:TEMP
git clone git@github.com:ironruby/ironruby.git
Set-Location ironruby
Copy-Item -Recurse .git $dir
Set-Location ..
Remove-Item -Recurse -Force ironruby
Pop-Location
git reset --hard
if ($get) {
  tf get /overwrite
}
Pop-Location

