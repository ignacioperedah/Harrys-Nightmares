# Nombre del archivo de salida
$outputFile = "codigo_unificado.txt"

# Limpiar o crear el archivo de salida (vaciamos el anterior si existe)
$null > $outputFile

# Recorrer cada argumento pasado (pueden ser nombres o "*.cs")
foreach ($argumento in $args) {
    # Resolvemos el camino (esto separa el *.cs en archivos individuales)
    $archivos = Get-ChildItem $argumento -ErrorAction SilentlyContinue

    foreach ($file in $archivos) {
        $nombre = $file.Name
        
        # Escribir el encabezado en el archivo
        Add-Content $outputFile "`n===================================================="
        Add-Content $outputFile "ARCHIVO: $nombre"
        Add-Content $outputFile "====================================================`n"
        
        # Copiar el contenido de este archivo específico
        Get-Content $file.FullName | Add-Content $outputFile
        
        Write-Host "Agregado: $nombre" -ForegroundColor Green
    }
}

Write-Host "`nListo! 'Harry' ya tiene su código unificado en $outputFile" -ForegroundColor Cyan