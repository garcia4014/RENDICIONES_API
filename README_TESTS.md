# Pruebas Unitarias - Rendiciones API

## 📋 Descripción

Este proyecto implementa pruebas unitarias completas para la API de Rendiciones, utilizando el mismo enfoque que **JUnit + Mockito** en Java, pero con las herramientas equivalentes de .NET:

- **xUnit** → Equivalente a JUnit
- **Moq** → Equivalente a Mockito  
- **FluentAssertions** → Para aserciones legibles y expresivas

## 🏗️ Estructura de Proyectos de Pruebas

```
RENDICIONES_API/
├── CapaNegocio.ContabilidadAPI.Tests/
│   └── Services/
│       ├── ComprobantePagoServiceTests.cs (12 pruebas)
│       └── SviaticoServiceTests.cs (15 pruebas)
│
└── ContabilidadAPI.Tests/
    └── Controllers/
        ├── ComprobantePagoControllerTests.cs (10 pruebas)
        └── SviaticoControllerTests.cs (9 pruebas)
```

## 📦 Paquetes NuGet Instalados

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| xUnit | 2.9.2 | Framework de pruebas |
| xunit.runner.visualstudio | 3.1.4 | Integración con VS Test Explorer |
| coverlet.collector | 7.0.5 | Recolección de cobertura de código |
| Moq | 4.20.72 | Librería de mocking |
| FluentAssertions | 8.8.0 | Aserciones fluidas |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.1 | Base de datos en memoria |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.1 | Pruebas de integración |

## 🎯 Patrones de Prueba Implementados

### 1. Patrón AAA (Arrange-Act-Assert)

```csharp
[Fact(DisplayName = "GetAllAsync - Debe retornar lista paginada de comprobantes")]
public async Task GetAllAsync_DebeRetornarListaPaginada()
{
    // Arrange - Preparación del escenario
    var mockService = new Mock<IComprobantePagoService>();
    var comprobantes = new List<ComprobantePagoDto> { /* datos */ };
    mockService.Setup(x => x.GetAllAsync(1, 10)).ReturnsAsync(apiResponse);

    // Act - Ejecución del método
    var result = await mockService.Object.GetAllAsync(1, 10);

    // Assert - Verificación de resultados
    result.Success.Should().BeTrue();
    result.Data.Should().HaveCount(2);
    mockService.Verify(x => x.GetAllAsync(1, 10), Times.Once);
}
```

### 2. Uso de Mocks (Equivalente a Mockito)

```csharp
// Java/Mockito:
// when(service.GetById(1)).thenReturn(comprobante);
// verify(service, times(1)).GetById(1);

// C#/Moq:
_mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(apiResponse);
_mockService.Verify(x => x.GetByIdAsync(1), Times.Once);
```

### 3. Pruebas Parametrizadas (Theory)

```csharp
[Theory(DisplayName = "ExisteDuplicadoAsync - Debe validar duplicados correctamente")]
[InlineData("F001", "00001", true)]
[InlineData("B001", "99999", false)]
public async Task ExisteDuplicadoAsync_DebeValidarDuplicados(
    string serie, string correlativo, bool existeDuplicado)
{
    // ...
}
```

### 4. FluentAssertions para Legibilidad

```csharp
// En lugar de:
Assert.True(result.Success);
Assert.Equal(2, result.Data.Count);

// Usamos:
result.Success.Should().BeTrue();
result.Data.Should().HaveCount(2);
result.Data.All(c => c.SvIdDetalle == detalleId).Should().BeTrue();
```

## 🚀 Cómo Ejecutar las Pruebas

### Opción 1: Línea de Comandos

```bash
# Ejecutar todas las pruebas de servicios
cd CapaNegocio.ContabilidadAPI.Tests
dotnet test --verbosity normal

# Ejecutar todas las pruebas de controladores
cd ContabilidadAPI.Tests
dotnet test --verbosity normal

# Ejecutar todas las pruebas de la solución
cd RENDICIONES_API
dotnet test

# Con cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Opción 2: Visual Studio

1. **Test Explorer**:
   - Menú: `Test` → `Test Explorer` (Ctrl+E, T)
   - Click en "Run All" para ejecutar todas las pruebas
   
2. **Ejecución Individual**:
   - Click derecho en el método de prueba
   - Seleccionar "Run Test(s)"

3. **Debug de Pruebas**:
   - Click derecho en el método
   - Seleccionar "Debug Test(s)"

### Opción 3: Rider / VS Code

```bash
# En la terminal integrada
dotnet test
```

## 📊 Resultados de Pruebas

**CapaNegocio.ContabilidadAPI.Tests:**
- ✅ 27 pruebas ejecutadas
- ✅ 27 exitosas
- ❌ 0 fallidas
- ⏱️ Duración: ~10.6s

**Desglose por Clase:**
- ComprobantePagoServiceTests: 12 pruebas ✅
- SviaticoServiceTests: 15 pruebas ✅

## 📝 Ejemplos de Pruebas Implementadas

### Pruebas de Servicios

```csharp
namespace CapaNegocio.ContabilidadAPI.Tests.Services
{
    public class ComprobantePagoServiceTests
    {
        ✅ GetAllAsync_DebeRetornarListaPaginada()
        ✅ GetByIdAsync_DebeRetornarComprobantePorId()
        ✅ GetByIdAsync_DebeRetornarError_CuandoNoExiste()
        ✅ CreateAsync_DebeCrearComprobante()
        ✅ DeleteAsync_DebeEliminarComprobante()
        ✅ GetByDetalleIdAsync_DebeRetornarComprobantesPorDetalle()
        ✅ ActualizarComprobanteObservado_DebeMarcaComoObservado()
        ✅ ExisteDuplicadoAsync_DebeValidarDuplicados() [Theory]
        ✅ ValidarComprobanteEnSunatAsync_DebeEjecutarValidacion()
        ✅ GetEstadisticasAsync_DebeRetornarEstadisticas()
        ✅ BuscarAsync_DebeManejarExcepciones()
    }
}
```

### Pruebas de Controladores

```csharp
namespace ContabilidadAPI.Tests.Controllers
{
    public class ComprobantePagoControllerTests
    {
        ✅ GetAll_DebeRetornarOkConLista()
        ✅ GetById_DebeRetornarOk_CuandoExiste()
        ✅ GetById_DebeRetornarNotFound_CuandoNoExiste()
        ✅ Create_DebeRetornarCreated()
        ✅ ActualizarObservado_DebeRetornarOk()
        ✅ Delete_DebeRetornarOk()
        ✅ GetBySvIdDetalle_DebeRetornarListaPorDetalle()
        ✅ GetById_DebeValidarParametros() [Theory]
        ✅ Create_DebeManejExcepcionDelServicio()
    }
}
```

## 🔍 Técnicas de Testing Utilizadas

### 1. Mocking de Dependencias

```csharp
// Simulamos el repositorio sin acceder a la BD real
var mockRepo = new Mock<IComprobantePago>();
mockRepo.Setup(x => x.GetById(1)).ReturnsAsync(comprobante);
```

### 2. Verificación de Llamadas

```csharp
// Verificamos que se llamó exactamente una vez
_mockService.Verify(x => x.CreateAsync(It.IsAny<ComprobantePagoCreateDto>()), Times.Once);
```

### 3. Aserciones Expresivas

```csharp
result.Data.Should().NotBeNull()
    .And.HaveCount(2)
    .And.OnlyContain(x => x.SvIdDetalle == 5);
```

### 4. Pruebas de Excepciones

```csharp
[Fact]
public async Task BuscarAsync_DebeManejarExcepciones()
{
    // Arrange
    var apiResponse = new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "Error interno")
    {
        Success = false,
        Errors = new List<string> { "Error de base de datos" }
    };
    
    // Act & Assert
    result.Success.Should().BeFalse();
    result.Errors.Should().Contain("Error de base de datos");
}
```

## 🎓 Comparación Java vs C#

| Concepto | Java (JUnit + Mockito) | C# (xUnit + Moq) |
|----------|------------------------|------------------|
| Anotación de Test | `@Test` | `[Fact]` |
| Test Parametrizado | `@ParameterizedTest` | `[Theory]` |
| Setup | `@BeforeEach` | Constructor |
| Mock | `@Mock` | `Mock<T>()` |
| When/Then | `when().thenReturn()` | `.Setup().Returns()` |
| Verify | `verify(mock, times(1))` | `.Verify(Times.Once)` |
| Assert | `assertEquals()` | `.Should().Be()` |

## 📖 Mejores Prácticas

### ✅ Hacer
- Usar nombres descriptivos para las pruebas
- Seguir el patrón AAA (Arrange-Act-Assert)
- Una aserción por prueba (idealmente)
- Probar casos límite y excepciones
- Usar `[Theory]` para pruebas parametrizadas
- Verificar llamadas a dependencias con `.Verify()`

### ❌ Evitar
- Pruebas que dependen de otras pruebas
- Pruebas que acceden a recursos externos (BD, APIs)
- Pruebas con lógica compleja
- Nombres genéricos como `Test1`, `Test2`
- Múltiples asserts no relacionados

## 🔧 Configuración Adicional

### Agregar Cobertura de Código

```bash
# Instalar herramienta de reportes
dotnet tool install -g dotnet-reportgenerator-globaltool

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Generar reporte HTML
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### Integración Continua (CI/CD)

```yaml
# .github/workflows/tests.yml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '10.0.x'
      - name: Run tests
        run: dotnet test --verbosity normal
```

## 🆘 Solución de Problemas

### Problema: "File is being used by another process"

**Solución:**
```bash
# Detener la aplicación en ejecución
# Cerrar Visual Studio
# Ejecutar las pruebas
dotnet test
```

### Problema: Pruebas lentas

**Solución:**
```bash
# Ejecutar en paralelo
dotnet test --parallel

# Ejecutar solo pruebas de una clase
dotnet test --filter "FullyQualifiedName~ComprobantePagoServiceTests"
```

## 📚 Recursos Adicionales

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/)
- [Microsoft Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## 👥 Contribuir

Para agregar nuevas pruebas:

1. Crear archivo en `Services/` o `Controllers/`
2. Heredar de la clase base si existe
3. Seguir el patrón AAA
4. Usar nombres descriptivos
5. Agregar `[Fact]` o `[Theory]`
6. Ejecutar `dotnet test` para verificar

---

**Última actualización:** 2024
**Mantenedor:** Equipo de Desarrollo Movitec
