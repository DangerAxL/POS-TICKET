# POS-TICKET - Sistema de Punto de Venta Nativo para Carnavales

Sistema de Punto de Venta (POS) profesional diseñado específicamente para eventos masivos, optimizado para máxima velocidad de despacho, uso con pantallas táctiles y hardware de impresión térmica Bluetooth/USB.

## 🚀 Guía de Funcionalidades Detallada

### 1. Panel de Ventas Dinámico
- **Botones de Producto**: Los productos se muestran con colores suaves y nombres grandes para una identificación visual rápida.
- **Acción Un Clic**: Al tocar un producto, se añade automáticamente **1 unidad** al ticket, ideal para momentos de alta demanda.
- **Acción Doble Clic (Multiplicador)**: Al hacer doble clic en un producto, se abre un teclado numérico táctil para ingresar una cantidad específica (ej. 10 cervezas), evitando clics repetitivos.

### 2. Gestión de Pagos Inteligente
- **Selección Obligatoria**: Antes de confirmar cada venta, el sistema solicita elegir el método de pago (**Efectivo** o **Mercado Pago/Transferencia**).
- **Discriminación de Saldo**: El sistema diferencia internamente qué dinero entró por vía digital y cuál por billetes físicos para facilitar el arqueo.

### 3. Control de Caja y Retiros (Seguridad)
- **Cambio Inicial**: Permite ingresar el monto con el que se arranca el turno/jornada directamente desde la pantalla principal.
- **COMP RETIROS DE CAJA**: Funcionalidad para registrar extracciones de efectivo durante el turno. Cada retiro:
  * Genera un comprobante impreso individual.
  * Se numera automáticamente (Nro 1, 2, 3...).
  * Se resta del saldo final de caja pero **no** afecta el total de ventas brutas (auditoría limpia).

### 4. Administración de Inventario en Tiempo Real
- **Actualizar Precios**: Permite modificar el valor de cualquier producto sin cerrar la aplicación.
- **Nuevo Producto**: Permite dar de alta productos que no estaban en la lista original, eligiendo su nombre, precio y color del botón. Estos cambios se guardan permanentemente en la base de datos SQL Server.

### 5. Cierre de Caja y Arqueo (Doble Respaldo)
Al presionar "Cerrar Caja", el sistema ejecuta tres acciones críticas:

#### A. Impresión de Ticket Térmico de Cierre
Formato idéntico al solicitado en Xenix Sol:
- Listado de productos con cantidades y subtotales.
- Desglose de **Cambio Inicial**.
- Listado detallado de **cada retiro de caja** efectuado.
- Cálculo de **Saldo Total** (Ventas + Inicio - Retiros).
- Diferenciación de **Efectivo en Caja** vs **Comprobantes Digitales**.

#### B. Reporte Digital en Excel
- Genera automáticamente un archivo `.xlsx` en el escritorio con la fecha y hora.
- Incluye el encabezado oficial amarillo con el número de caja.
- Organiza los datos para auditoría contable.

#### C. Auditoría en Base de Datos
- Guarda de forma inmutable la fecha, hora y el total de ventas en SQL Server para consultas históricas.

### 6. Configuración de Terminales
- **Número de Caja Dinámico**: A través del archivo `config.ini`, se puede asignar un número único a cada dispositivo (Caja 1, Caja 2, etc.), permitiendo que el sistema funcione localmente en múltiples terminales de forma sincronizada pero identificable.

## 🛠️ Especificaciones Técnicas
- **Lenguaje**: C# / .NET 8 / WPF (Nativo Windows).
- **Base de Datos**: Microsoft SQL Server (Base: `POS-TICKET`).
- **Impresión**: Driver genérico de texto o gráfico para 80mm.
- **Configuración**: Local, sin necesidad de Internet para operar.

---
*Desarrollado para la Gestión de Ventas POS*
