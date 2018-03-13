@echo off

rmdir Client\Assets /s /q
rmdir Server\Assets /s /q
mklink /d Client\Assets ..\Assets
mklink /d Server\Assets ..\Assets
