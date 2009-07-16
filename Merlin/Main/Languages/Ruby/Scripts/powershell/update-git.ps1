param(
  [string]
  $exp = "rubysync",
  [string]
  $logfile = "update-git.log"
)

$env:logfile = $logfile
Push-Location #intentionally called without args, the following expression should set the new location
Invoke-Expression $exp

#TODO: Utilize or re-create commonps.ps1
if ($env:MERLIN_ROOT -eq $null) {
  Write-Error "`$exp is expected to set $env:MERLIN_ROOT and Set-Location to within the repository"
  exit-pushd
  exit
}

write-command "git pull"
write-command "tf get /overwrite"
write-command "rake git:commit"
write-command "git push"

pop-location

