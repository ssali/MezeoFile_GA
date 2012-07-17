@echo off
@echo Installing...
md "%Temp%\MezeoFile.2.0.66"
xcopy . "%Temp%\MezeoFile.2.0.66" /scrye
"%Temp%\MezeoFile.2.0.66\setup.exe"