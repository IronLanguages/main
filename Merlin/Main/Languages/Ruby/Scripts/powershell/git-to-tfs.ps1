param(
[string] $remote,
[switch] $skip_pull,
[switch] $skip_tfs,
[switch] $skip_msg
)

if (!$skip_pull) {
  git pull $remote master
  if($lastexitcode -ne 0) {
    exit(1)
  }
}
$pager = $env:PAGER
set PAGER=cat
$_, $begin, $end = (git log -1 | select-string "Merge:").tostring().split(' ')
$log = invoke-expression "git log --name-status ^$begin..$end" | select-string "^(M|A|D|\s)\s"

$message = $log | select-string "^\s+"
$files = $log | select-string "^(M|A|D)"
if(!$skip_tfs) {
  cd "$env:MERLIN_ROOT\..\.."
  $files | %{
    $mode_and_file = $_.ToString().Split()
    $mode = $mode_and_file | select-object -first 1
    $file = $mode_and_file | select-object -last 1
    switch($mode) {
    A { "add $file" | out-string; tf add $file; break }
    D { "delete $file" | out-string; tf delete $file; break }
    M { "edit $file" | out-string; tf edit $file; break }
    }
  }
}
if(!$skip_msg){
  set-content -path "$env:HOME\Desktop\commit-$remote.txt" $message
}
set PAGER=$pager

