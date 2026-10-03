@echo off
setlocal
set "XADREZ_PROJECT_DIR=%~dp0..\Game"

rem Require a real Unity project before creating folders.
for %%D in (Assets Packages ProjectSettings) do (
    if not exist "%XADREZ_PROJECT_DIR%\%%D\" (
        echo ERRO: projeto Unity nao encontrado em "%XADREZ_PROJECT_DIR%".
        echo Crie primeiro o projeto Game no Unity Hub e execute novamente.
        exit /b 1
    )
)

for %%D in (
    "Scenes"
    "Prefabs"
    "Data"
    "Scripts\Board"
    "Scripts\Pieces"
    "Scripts\Combat"
    "Scripts\Evolutions"
    "Scripts\Economy"
    "Scripts\Turns"
    "Scripts\UI"
    "Scripts\Presentation"
    "Sprites\Pieces\Pawn"
    "Sprites\Pieces\Horse"
    "Sprites\Pieces\King"
    "Sprites\Pieces\Rook"
    "Sprites\Pieces\Bishop"
    "Sprites\Pieces\Queen"
    "Sprites\Board"
    "Sprites\UI"
) do (
    call :CreateFolder "%%~D"
    if errorlevel 1 goto Failed
)

echo Estrutura criada em Game\Assets\_Game.
echo Pastas existentes preservadas. Scripts, sprites e cenas serao adicionados depois.
echo Abra a Unity para importar as pastas e gerar seus arquivos .meta.
exit /b 0

:CreateFolder
set "XADREZ_TARGET_DIR=%XADREZ_PROJECT_DIR%\Assets\_Game\%~1"
if not exist "%XADREZ_TARGET_DIR%\" mkdir "%XADREZ_TARGET_DIR%"
if not exist "%XADREZ_TARGET_DIR%\" exit /b 1
rem Git does not track empty directories. Do not overwrite existing placeholders.
if not exist "%XADREZ_TARGET_DIR%\.gitkeep" (
    type nul > "%XADREZ_TARGET_DIR%\.gitkeep"
    if errorlevel 1 exit /b 1
)
exit /b 0

:Failed
echo ERRO: nao foi possivel criar uma das pastas. Verifique a mensagem acima.
exit /b 1
