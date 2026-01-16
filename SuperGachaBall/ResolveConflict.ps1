
$file = "Assets/Scenes/Test World.unity"
$backup = "Assets/Scenes/Test World.unity.bak"
Copy-Item $file $backup

$lines = Get-Content $file
$newLines = @()
$skip = $false

foreach ($line in $lines) {
    if ($line.StartsWith("<<<<<<<")) {
        # Start of conflict, we want to keep what follows (HEAD)
        $skip = $false
        continue
    }
    elseif ($line.StartsWith("=======")) {
        # Middle of conflict, start skipping the other side
        $skip = $true
        continue
    }
    elseif ($line.StartsWith(">>>>>>>")) {
        # End of conflict, stop skipping
        $skip = $false
        continue
    }

    if (-not $skip) {
        $newLines += $line
    }
}

$newLines | Set-Content $file
Write-Host "Conflict markers removed. Backup saved to $backup"
