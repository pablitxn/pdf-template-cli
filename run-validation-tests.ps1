# PDF Template CLI - E2E Validation Test Runner
# This script runs the E2E validation tests using fixtures and LLM validation

Write-Host "🧪 PDF Template CLI - E2E Validation Test Runner" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if API key is configured
$appsettings = Get-Content "src\Adapters\CLI\appsettings.json" -Raw
if ($appsettings -match "YOUR_OPENAI_API_KEY") {
    Write-Host "⚠️  WARNING: OpenAI API key not configured!" -ForegroundColor Yellow
    Write-Host "Please update src\Adapters\CLI\appsettings.json with your API key" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        exit 1
    }
}

# Clean previous test outputs
Write-Host "🧹 Cleaning previous test outputs..." -ForegroundColor Gray
if (Test-Path "tests\E2E\test-outputs") {
    Remove-Item -Path "tests\E2E\test-outputs" -Recurse -Force
}
if (Test-Path "tests\E2E\validation-summary.json") {
    Remove-Item -Path "tests\E2E\validation-summary.json" -Force
}

# Build the E2E test project
Write-Host "🔨 Building E2E test project..." -ForegroundColor Gray
dotnet build tests\E2E\E2E.csproj -c Release --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Run the validation tests
Write-Host ""
Write-Host "🚀 Running validation tests with LLM..." -ForegroundColor Green
Write-Host "This will process all fixtures and validate outputs using AI" -ForegroundColor Gray
Write-Host ""

dotnet run --project tests\E2E\E2E.csproj -c Release --no-build

# Check if validation summary was created
if (Test-Path "tests\E2E\validation-summary.json") {
    Write-Host ""
    Write-Host "✅ Validation tests completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Summary:" -ForegroundColor Cyan
    
    # Parse and display summary
    $summary = Get-Content "tests\E2E\validation-summary.json" | ConvertFrom-Json
    Write-Host "Total Tests: $($summary.summary.totalTests)"
    Write-Host "Passed: $($summary.summary.passed) ✅" -ForegroundColor Green
    Write-Host "Failed: $($summary.summary.failed) ❌" -ForegroundColor Red
    Write-Host "Average Confidence: $([math]::Round($summary.summary.averageConfidence * 100, 2))%"
    
    Write-Host ""
    Write-Host "📁 Generated outputs: tests\E2E\test-outputs\" -ForegroundColor Gray
    Write-Host "📄 Full report: tests\E2E\validation-summary.json" -ForegroundColor Gray
} else {
    Write-Host "❌ Validation tests failed or did not complete" -ForegroundColor Red
    exit 1
}