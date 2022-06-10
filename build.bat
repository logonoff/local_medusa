rmdir bin /s /q
dotnet publish -o bin/medusae/local_medusa /p:DebugType=embedded
copy bin\medusae\local_medusa\local_medusa.dll bin
