# Pruebas del inventario

## Ejecución

```powershell
dotnet test .\Nexora.Tests\Nexora.Tests.csproj
```

También se pueden ejecutarlar desde Visual Studio Test Explorer después de compilar la solución.

## Cobertura

Las pruebas de `Controllers/InventarioControllerTests.cs` usan una base de datos EF Core InMemory aislada por prueba:

- `Create_Post_GuardaProductoYRedirigeAlInventario`: comprueba que la acción Create guarda el SKU, nombre, precio y stock del producto, y redirige a Index.
- `Edit_Post_ActualizaProductoYRedirigeAlInventario`: comprueba que la acción Edit actualiza nombre, precio y stock, y redirige a Index.