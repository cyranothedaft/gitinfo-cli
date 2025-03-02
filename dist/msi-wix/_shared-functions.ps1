function log1([System.ConsoleColor]$color, $obj) { 
    Write-Host -ForegroundColor $color $obj
}

function log2([System.ConsoleColor]$color1, $obj1, [System.ConsoleColor]$color2, $obj2) { 
    Write-Host -ForegroundColor $color1 -NoNewline $obj1
    Write-Host -ForegroundColor $color2            $obj2
}

function debug([string]$label, $value) { log2 DarkGray "# ${label}: " DarkGreen  $value }
function info ([string]$label, $value) { log2 DarkBlue "${label}: "   DarkYellow $value }
function prog ([string]$label        ) { log1 Cyan     "-- ${label}" }
