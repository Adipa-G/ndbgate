for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
for /d /r . %%d in (packages) do @if exist "%%d" (
	cd %%d
	for /d /r . %%e in (*) do @if exist "%%e" rd /s/q "%%e"
)