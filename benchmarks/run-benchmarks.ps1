param(
    [int]$Iterations = 5,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$cliProject = Join-Path $root "src/MsfLang.Cli/MsfLang.Cli.csproj"
$programs = @(
    "factorial20.msf",
    "sum_recursive_1000.msf",
    "arith_loop_500.msf",
    "fib20.msf"
)

Write-Host "Building CLI ($Configuration)..."
dotnet build $cliProject -c $Configuration --nologo | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

$results = @()
foreach ($name in $programs) {
    $path = Join-Path $root "benchmarks/programs/$name"
    $times = @()
    $output = $null
    $exitCode = $null
    for ($i = 0; $i -lt $Iterations; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $output = & dotnet run --project $cliProject -c $Configuration --no-build -- run $path 2>&1
        $exitCode = $LASTEXITCODE
        $sw.Stop()
        $times += [math]::Round($sw.Elapsed.TotalMilliseconds, 3)
        if ($exitCode -ne 0) { throw "Benchmark $name failed: $output" }
    }
    $avg = [math]::Round((($times | Measure-Object -Average).Average), 3)
    $min = [math]::Round((($times | Measure-Object -Minimum).Minimum), 3)
    $max = [math]::Round((($times | Measure-Object -Maximum).Maximum), 3)
    $results += [pscustomobject]@{
        Program = $name
        Iterations = $Iterations
        AvgMs = $avg
        MinMs = $min
        MaxMs = $max
        Output = (($output | Out-String).Trim())
        SamplesMs = ($times -join ", ")
    }
}

$results | Format-Table -AutoSize
$stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
$md = @()
$md += "# Benchmark Run"
$md += ""
$md += "- Date: $stamp"
$md += "- Machine: $env:COMPUTERNAME"
$md += "- OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription)"
$md += "- .NET: $(dotnet --version)"
$md += "- Configuration: $Configuration"
$md += "- Iterations per program: $Iterations"
$md += "- Command: dotnet run --project src/MsfLang.Cli -c $Configuration --no-build -- run benchmarks/programs/<program>"
$md += ""
$md += "| Program | Output | Avg (ms) | Min (ms) | Max (ms) | Samples (ms) |"
$md += "|---|---:|---:|---:|---:|---|"
foreach ($r in $results) {
    $md += "| $($r.Program) | $($r.Output) | $($r.AvgMs) | $($r.MinMs) | $($r.MaxMs) | $($r.SamplesMs) |"
}
$outFile = Join-Path $root "benchmarks/results/latest.md"
$md -join "`n" | Set-Content -Path $outFile -Encoding UTF8
Write-Host "Wrote $outFile"
