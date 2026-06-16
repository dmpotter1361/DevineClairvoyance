$ErrorActionPreference = "Stop"
$mod = Get-Content "G:\My Drive\VB\DC\DC\Module1.vb" -Raw

function Unescape($s) { return $s -replace '""', '"' }

$cards = [ordered]@{}

# --- getShortTerm: If e = "KEY" Then shortPhrase = "VALUE" ---
$shortRe = [regex]'(?s)If e = "((?:[^"]|"")*)" Then shortPhrase = "((?:[^"]|"")*)"'
foreach ($m in $shortRe.Matches($mod)) {
    $key = Unescape $m.Groups[1].Value
    $val = Unescape $m.Groups[2].Value
    if (-not $cards.Contains($key)) { $cards[$key] = [ordered]@{} }
    $cards[$key]["short"] = $val
}

# --- createDictionary: myDictionary.Add("KEY", "VALUE") (VALUE multiline) ---
# Split on the Add( marker so each chunk's terminating ") is unambiguous.
$body = $mod
$addRe = [regex]'(?s)myDictionary\.Add\("((?:[^"]|"")*?)",\s*"(.*?)"\)(?=\s*(?:myDictionary\.Add\(|Return\b|End Function|''))'
foreach ($m in $addRe.Matches($body)) {
    $key = Unescape $m.Groups[1].Value
    $val = Unescape $m.Groups[2].Value
    if (-not $cards.Contains($key)) { $cards[$key] = [ordered]@{} }
    $cards[$key]["long"] = $val.Trim()
}

$json = $cards | ConvertTo-Json -Depth 5
Set-Content -Path "C:\Users\micha\DevineClairvoyance\cards.json" -Value $json -Encoding UTF8

$withShort = ($cards.Values | Where-Object { $_.short }).Count
$withLong  = ($cards.Values | Where-Object { $_.long }).Count
Write-Output "Total keys: $($cards.Count)  | with short: $withShort | with long: $withLong"
Write-Output "Keys missing long:"
$cards.GetEnumerator() | Where-Object { -not $_.Value.long } | ForEach-Object { "  - $($_.Key)" }