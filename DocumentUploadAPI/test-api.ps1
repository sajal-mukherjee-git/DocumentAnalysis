# Test script for Document Upload API
# This PowerShell script tests the API endpoints

param(
    [string]$BaseUrl = "http://localhost:8080",
    [string]$TestFilePath = $null
)

Write-Host "Testing Document Upload API at: $BaseUrl" -ForegroundColor Green

# Test 1: Health Check
Write-Host "`n1. Testing Health Check..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get
    Write-Host "Health Check: " -NoNewline
    Write-Host "PASSED" -ForegroundColor Green
    Write-Host "Response: $($healthResponse | ConvertTo-Json -Depth 2)"
} catch {
    Write-Host "Health Check: " -NoNewline
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)"
}

# Test 2: API Documentation
Write-Host "`n2. Testing API Documentation..." -ForegroundColor Yellow
try {
    $docsResponse = Invoke-WebRequest -Uri "$BaseUrl/swagger" -Method Get
    if ($docsResponse.StatusCode -eq 200) {
        Write-Host "API Documentation: " -NoNewline
        Write-Host "PASSED" -ForegroundColor Green
    }
} catch {
    Write-Host "API Documentation: " -NoNewline
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)"
}

# Test 3: File Upload (if test file provided)
if ($TestFilePath -and (Test-Path $TestFilePath)) {
    Write-Host "`n3. Testing File Upload..." -ForegroundColor Yellow
    try {
        $form = @{
            file = Get-Item $TestFilePath
            description = "Test file upload from PowerShell script"
        }
        
        $uploadResponse = Invoke-RestMethod -Uri "$BaseUrl/api/documents/upload" -Method Post -Form $form
        Write-Host "File Upload: " -NoNewline
        Write-Host "PASSED" -ForegroundColor Green
        Write-Host "Upload Response:"
        Write-Host ($uploadResponse | ConvertTo-Json -Depth 2)
    } catch {
        Write-Host "File Upload: " -NoNewline
        Write-Host "FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)"
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response Body: $responseBody"
        }
    }
} else {
    Write-Host "`n3. Skipping File Upload Test (no test file provided)" -ForegroundColor Gray
    Write-Host "To test file upload, run: ./test-api.ps1 -TestFilePath 'path\to\your\file.pdf'"
}

# Test 4: Invalid File Upload (test validation)
Write-Host "`n4. Testing File Validation (empty file)..." -ForegroundColor Yellow
try {
    # Create a temporary empty file
    $tempFile = [System.IO.Path]::GetTempFileName()
    $form = @{
        file = Get-Item $tempFile
        description = "Test empty file for validation"
    }
    
    $validationResponse = Invoke-RestMethod -Uri "$BaseUrl/api/documents/upload" -Method Post -Form $form -ErrorAction Stop
    Write-Host "File Validation: " -NoNewline
    Write-Host "FAILED (should have rejected empty file)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "File Validation: " -NoNewline
        Write-Host "PASSED (correctly rejected empty file)" -ForegroundColor Green
    } else {
        Write-Host "File Validation: " -NoNewline
        Write-Host "FAILED (unexpected error)" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)"
    }
} finally {
    # Clean up temp file
    if (Test-Path $tempFile) {
        Remove-Item $tempFile -Force
    }
}

Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
Write-Host "API Base URL: $BaseUrl"
Write-Host "Test completed. Check results above." -ForegroundColor Green

# Example usage instructions
Write-Host "`n=== Usage Examples ===" -ForegroundColor Cyan
Write-Host "Health Check:"
Write-Host "  curl $BaseUrl/health"
Write-Host ""
Write-Host "File Upload:"
Write-Host "  curl -X POST $BaseUrl/api/documents/upload -F 'file=@example.pdf' -F 'description=Test upload'"
Write-Host ""
Write-Host "PowerShell Upload:"
Write-Host "  `$form = @{ file = Get-Item 'example.pdf'; description = 'Test' }"
Write-Host "  Invoke-RestMethod -Uri '$BaseUrl/api/documents/upload' -Method Post -Form `$form"
