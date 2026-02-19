# POS-TICKET - Sistema de Punto de Venta Nativo

Sistema de Punto de Venta (POS) diseñado para Windows, optimizado para pantallas táctiles y hardware de impresión térmica de 80mm.

## 🚀 Características Principales
- **Interfaz Nativa**: Desarrollado en C# con WPF (.NET 8), garantizando fluidez y compatibilidad total con periféricos en Windows.
- **Optimizado para Táctil**: Botones grandes, colores suaves para evitar fatiga visual y gestos intuitivos.
- **Base de Datos Robusta**: Integración con SQL Server para almacenamiento local de ventas, productos y auditorías.
- **Gestión de Sesión Dinámica**:
  - Un clic para agregar 1 unidad.
  - Doble clic para ingresar cantidad personalizada.
- **Fácil Administración**:
  - Actualización de precios en tiempo real.
  - Creación de nuevos productos con selección de colores.
- **Auditoría y Cierre**:
  - Registro de retiros de caja numerados.
  - Cierre de caja con exportación automática a Excel (Formato Profesional).
  - Almacenamiento histórico de cierres en la base de datos.
- **Impresión Térmica**: Configurado para ticketeadoras de 80mm vía Bluetooth/USB (Protocolo nativo de Windows).

## 🛠️ Requisitos Técnicos
- **Sistema Operativo**: Windows 10 o Windows 11.
- **Base de Datos**: SQL Server (LocalDB o instancia completa).
- **Framework**: .NET 8.0 SDK o Runtime.
- **Periféricos**: Impresora térmica de 80mm instalada como impresora predeterminada.

## 📦 Instalación y Configuración
1. **Configurar Número de Caja**:
   - Localiza el archivo `config.ini` en la carpeta raíz.
   - Edita `CajaNumber=1` con el número asignado a ese terminal.
2. **Instalar Dependencias**:
   - Asegúrate de tener SQL Server instalado. Al iniciar el programa por primera vez, se creará automáticamente la base de datos `POS-TICKET`.
3. **Ejecución**:
   - Ejecuta `SimplePOS.exe` o compila el código fuente usando:
     ```bash
     dotnet run
     ```

## 📂 Estructura del Código
- `Models/`: Modelos de datos y contexto de base de datos (EF Core).
- `ViewModels/`: Lógica de negocio e interactividad (Patrón MVVM).
- `Views/`: Interfaces gráficas en XAML.
- `config.ini`: Configuración local del terminal.

## ⚖️ Licencia
Este proyecto es de uso libre bajo la licencia MIT.
