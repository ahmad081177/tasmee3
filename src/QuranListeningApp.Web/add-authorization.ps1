# Security Fix Script - Add Authorization Attributes
# This script adds [Authorize] attributes to all pages missing them

Write-Host "Adding authorization attributes to protect pages..." -ForegroundColor Cyan

# Admin pages - require Admin role
$adminPages = @(
    'Components\Pages\Admin\AdminStudentReport.razor',
    'Components\Pages\Admin\EditSession.razor',
    'Components\Pages\Admin\Reports.razor',
    'Components\Pages\Admin\AddSession.razor',
    'Components\Pages\Admin\AddUser.razor',
    'Components\Pages\Admin\EditUser.razor'
)

foreach ($page in $adminPages) {
    $filePath = "D:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web\$page"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if ($content -notmatch '@attribute.*Authorize') {
            Write-Host "Adding Admin authorization to: $page" -ForegroundColor Yellow
            $content = $content -replace '(@rendermode InteractiveServer)', "`$1`n@attribute [Microsoft.AspNetCore.Authorization.Authorize(Roles = `"Admin`")]"
            Set-Content -Path $filePath -Value $content -Encoding UTF8 -NoNewline
        }
    }
}

# Teacher pages - require Teacher role
$teacherPages = @(
    'Components\Pages\Teacher\EditSession.razor',
    'Components\Pages\Teacher\TeacherStudentReport.razor',
    'Components\Pages\Teacher\AddSession.razor'
)

foreach ($page in $teacherPages) {
    $filePath = "D:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web\$page"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if ($content -notmatch '@attribute.*Authorize') {
            Write-Host "Adding Teacher authorization to: $page" -ForegroundColor Yellow
            $content = $content -replace '(@rendermode InteractiveServer)', "`$1`n@attribute [Microsoft.AspNetCore.Authorization.Authorize(Roles = `"Teacher`")]"
            Set-Content -Path $filePath -Value $content -Encoding UTF8 -NoNewline
        }
    }
}

# Student pages - require Student role
$studentPages = @(
    'Components\Pages\Student\Dashboard.razor'
)

foreach ($page in $studentPages) {
    $filePath = "D:\ws\personal\mine\tasmee3\src\QuranListeningApp.Web\$page"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if ($content -notmatch '@attribute.*Authorize') {
            Write-Host "Adding Student authorization to: $page" -ForegroundColor Yellow
            $content = $content -replace '(@rendermode InteractiveServer)', "`$1`n@attribute [Microsoft.AspNetCore.Authorization.Authorize(Roles = `"Student`")]"
            Set-Content -Path $filePath -Value $content -Encoding UTF8 -NoNewline
        }
    }
}

Write-Host "`nAuthorization attributes added successfully!" -ForegroundColor Green
Write-Host "Building project to verify changes..." -ForegroundColor Cyan
