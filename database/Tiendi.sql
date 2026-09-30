USE master;
GO

IF DB_ID('Tiendi_DB') IS NULL
BEGIN
    CREATE DATABASE Tiendi_DB;
END
GO

USE Tiendi_DB;
GO

CREATE TABLE Usuarios
(
    IdUsuario INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Rol NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Usuarios_Rol DEFAULT 'ADMIN',
    Activo BIT NOT NULL
        CONSTRAINT DF_Usuarios_Activo DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT CK_Usuarios_Rol
        CHECK (Rol IN ('ADMIN', 'VENDEDOR'))
);
GO

CREATE TABLE Categorias
(
    IdCategoria INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    Activo BIT NOT NULL
        CONSTRAINT DF_Categorias_Activo DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Categorias_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Categorias PRIMARY KEY (IdCategoria),
    CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE Productos
(
    IdProducto INT IDENTITY(1,1) NOT NULL,
    IdCategoria INT NULL,
    Codigo NVARCHAR(50) NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    ImagenUrl NVARCHAR(500) NULL,
    Costo DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Productos_Costo DEFAULT 0,
    PrecioVenta DECIMAL(18,2) NOT NULL,
    Stock DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Productos_Stock DEFAULT 0,
    StockMinimo DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Productos_StockMinimo DEFAULT 0,
    Activo BIT NOT NULL
        CONSTRAINT DF_Productos_Activo DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Productos_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Productos PRIMARY KEY (IdProducto),

    CONSTRAINT FK_Productos_Categorias
        FOREIGN KEY (IdCategoria)
        REFERENCES Categorias(IdCategoria),

    CONSTRAINT CK_Productos_Costo
        CHECK (Costo >= 0),

    CONSTRAINT CK_Productos_PrecioVenta
        CHECK (PrecioVenta >= 0),

    CONSTRAINT CK_Productos_Stock
        CHECK (Stock >= 0),

    CONSTRAINT CK_Productos_StockMinimo
        CHECK (StockMinimo >= 0)
);
GO

CREATE UNIQUE INDEX UX_Productos_Codigo
ON Productos(Codigo)
WHERE Codigo IS NOT NULL;
GO

CREATE INDEX IX_Productos_Nombre
ON Productos(Nombre);
GO

CREATE INDEX IX_Productos_IdCategoria
ON Productos(IdCategoria);
GO

CREATE TABLE Clientes
(
    IdCliente INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NULL,
    Identificacion NVARCHAR(20) NULL,
    Telefono NVARCHAR(20) NULL,
    Email NVARCHAR(150) NULL,
    Direccion NVARCHAR(250) NULL,
    Activo BIT NOT NULL
        CONSTRAINT DF_Clientes_Activo DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Clientes_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Clientes PRIMARY KEY (IdCliente)
);
GO

CREATE UNIQUE INDEX UX_Clientes_Identificacion
ON Clientes(Identificacion)
WHERE Identificacion IS NOT NULL;
GO

CREATE INDEX IX_Clientes_Nombre
ON Clientes(Nombre, Apellido);
GO

CREATE TABLE Proveedores
(
    IdProveedor INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Identificacion NVARCHAR(20) NULL,
    Telefono NVARCHAR(20) NULL,
    Email NVARCHAR(150) NULL,
    Direccion NVARCHAR(250) NULL,
    Activo BIT NOT NULL
        CONSTRAINT DF_Proveedores_Activo DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Proveedores_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Proveedores PRIMARY KEY (IdProveedor)
);
GO

CREATE UNIQUE INDEX UX_Proveedores_Identificacion
ON Proveedores(Identificacion)
WHERE Identificacion IS NOT NULL;
GO

CREATE INDEX IX_Proveedores_Nombre
ON Proveedores(Nombre);
GO

CREATE TABLE Ventas
(
    IdVenta BIGINT IDENTITY(1,1) NOT NULL,
    IdCliente INT NULL,
    IdUsuario INT NOT NULL,
    NumeroVenta NVARCHAR(30) NOT NULL,
    Fecha DATETIME2 NOT NULL
        CONSTRAINT DF_Ventas_Fecha DEFAULT SYSDATETIME(),
    Subtotal DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Ventas_Descuento DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Ventas_Estado DEFAULT 'COMPLETADA',

    CONSTRAINT PK_Ventas PRIMARY KEY (IdVenta),
    CONSTRAINT UQ_Ventas_NumeroVenta UNIQUE (NumeroVenta),

    CONSTRAINT FK_Ventas_Clientes
        FOREIGN KEY (IdCliente)
        REFERENCES Clientes(IdCliente),

    CONSTRAINT FK_Ventas_Usuarios
        FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios(IdUsuario),

    CONSTRAINT CK_Ventas_Subtotal
        CHECK (Subtotal >= 0),

    CONSTRAINT CK_Ventas_Descuento
        CHECK (Descuento >= 0),

    CONSTRAINT CK_Ventas_Total
        CHECK (Total >= 0),

    CONSTRAINT CK_Ventas_DescuentoSubtotal
        CHECK (Descuento <= Subtotal),

    CONSTRAINT CK_Ventas_Estado
        CHECK (Estado IN ('COMPLETADA', 'ANULADA'))
);
GO

CREATE INDEX IX_Ventas_Fecha
ON Ventas(Fecha);
GO

CREATE INDEX IX_Ventas_IdCliente
ON Ventas(IdCliente);
GO

CREATE INDEX IX_Ventas_IdUsuario
ON Ventas(IdUsuario);
GO

CREATE TABLE DetalleVentas
(
    IdDetalleVenta BIGINT IDENTITY(1,1) NOT NULL,
    IdVenta BIGINT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_DetalleVentas_Descuento DEFAULT 0,
    Subtotal DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_DetalleVentas PRIMARY KEY (IdDetalleVenta),

    CONSTRAINT FK_DetalleVentas_Ventas
        FOREIGN KEY (IdVenta)
        REFERENCES Ventas(IdVenta),

    CONSTRAINT FK_DetalleVentas_Productos
        FOREIGN KEY (IdProducto)
        REFERENCES Productos(IdProducto),

    CONSTRAINT CK_DetalleVentas_Cantidad
        CHECK (Cantidad > 0),

    CONSTRAINT CK_DetalleVentas_Precio
        CHECK (PrecioUnitario >= 0),

    CONSTRAINT CK_DetalleVentas_Costo
        CHECK (CostoUnitario >= 0),

    CONSTRAINT CK_DetalleVentas_Descuento
        CHECK (Descuento >= 0),

    CONSTRAINT CK_DetalleVentas_Subtotal
        CHECK (Subtotal >= 0)
);
GO

CREATE INDEX IX_DetalleVentas_IdVenta
ON DetalleVentas(IdVenta);
GO

CREATE INDEX IX_DetalleVentas_IdProducto
ON DetalleVentas(IdProducto);
GO

CREATE TABLE Pagos
(
    IdPago BIGINT IDENTITY(1,1) NOT NULL,
    IdVenta BIGINT NOT NULL,
    MetodoPago NVARCHAR(30) NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Fecha DATETIME2 NOT NULL
        CONSTRAINT DF_Pagos_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Pagos PRIMARY KEY (IdPago),

    CONSTRAINT FK_Pagos_Ventas
        FOREIGN KEY (IdVenta)
        REFERENCES Ventas(IdVenta),

    CONSTRAINT CK_Pagos_Metodo
        CHECK
        (
            MetodoPago IN
            (
                'EFECTIVO',
                'TRANSFERENCIA',
                'TARJETA'
            )
        ),

    CONSTRAINT CK_Pagos_Monto
        CHECK (Monto > 0)
);
GO

CREATE INDEX IX_Pagos_IdVenta
ON Pagos(IdVenta);
GO

CREATE TABLE Gastos
(
    IdGasto BIGINT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NOT NULL,
    Descripcion NVARCHAR(250) NOT NULL,
    Categoria NVARCHAR(100) NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Fecha DATETIME2 NOT NULL
        CONSTRAINT DF_Gastos_Fecha DEFAULT SYSDATETIME(),
    Activo BIT NOT NULL
        CONSTRAINT DF_Gastos_Activo DEFAULT 1,

    CONSTRAINT PK_Gastos PRIMARY KEY (IdGasto),

    CONSTRAINT FK_Gastos_Usuarios
        FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios(IdUsuario),

    CONSTRAINT CK_Gastos_Monto
        CHECK (Monto > 0)
);
GO

CREATE INDEX IX_Gastos_Fecha
ON Gastos(Fecha);
GO

CREATE INDEX IX_Gastos_IdUsuario
ON Gastos(IdUsuario);
GO

CREATE TABLE Compras
(
    IdCompra BIGINT IDENTITY(1,1) NOT NULL,
    IdProveedor INT NOT NULL,
    IdUsuario INT NOT NULL,
    NumeroCompra NVARCHAR(30) NOT NULL,
    Fecha DATETIME2 NOT NULL
        CONSTRAINT DF_Compras_Fecha DEFAULT SYSDATETIME(),
    Subtotal DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_Compras_Descuento DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Compras_Estado DEFAULT 'COMPLETADA',
    Observacion NVARCHAR(250) NULL,

    CONSTRAINT PK_Compras PRIMARY KEY (IdCompra),
    CONSTRAINT UQ_Compras_NumeroCompra UNIQUE (NumeroCompra),

    CONSTRAINT FK_Compras_Proveedores
        FOREIGN KEY (IdProveedor)
        REFERENCES Proveedores(IdProveedor),

    CONSTRAINT FK_Compras_Usuarios
        FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios(IdUsuario),

    CONSTRAINT CK_Compras_Subtotal
        CHECK (Subtotal >= 0),

    CONSTRAINT CK_Compras_Descuento
        CHECK (Descuento >= 0),

    CONSTRAINT CK_Compras_Total
        CHECK (Total >= 0),

    CONSTRAINT CK_Compras_DescuentoSubtotal
        CHECK (Descuento <= Subtotal),

    CONSTRAINT CK_Compras_Estado
        CHECK (Estado IN ('COMPLETADA', 'ANULADA'))
);
GO

CREATE INDEX IX_Compras_IdProveedor
ON Compras(IdProveedor);
GO

CREATE INDEX IX_Compras_IdUsuario
ON Compras(IdUsuario);
GO

CREATE INDEX IX_Compras_Fecha
ON Compras(Fecha);
GO

CREATE TABLE DetalleCompras
(
    IdDetalleCompra BIGINT IDENTITY(1,1) NOT NULL,
    IdCompra BIGINT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_DetalleCompras_Descuento DEFAULT 0,
    Subtotal DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_DetalleCompras PRIMARY KEY (IdDetalleCompra),

    CONSTRAINT FK_DetalleCompras_Compras
        FOREIGN KEY (IdCompra)
        REFERENCES Compras(IdCompra),

    CONSTRAINT FK_DetalleCompras_Productos
        FOREIGN KEY (IdProducto)
        REFERENCES Productos(IdProducto),

    CONSTRAINT CK_DetalleCompras_Cantidad
        CHECK (Cantidad > 0),

    CONSTRAINT CK_DetalleCompras_Costo
        CHECK (CostoUnitario >= 0),

    CONSTRAINT CK_DetalleCompras_Descuento
        CHECK (Descuento >= 0),

    CONSTRAINT CK_DetalleCompras_Subtotal
        CHECK (Subtotal >= 0)
);
GO

CREATE INDEX IX_DetalleCompras_IdCompra
ON DetalleCompras(IdCompra);
GO

CREATE INDEX IX_DetalleCompras_IdProducto
ON DetalleCompras(IdProducto);
GO

CREATE TABLE MovimientosInventario
(
    IdMovimiento BIGINT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL,
    IdUsuario INT NOT NULL,
    TipoMovimiento NVARCHAR(30) NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    StockAnterior DECIMAL(18,2) NOT NULL,
    StockNuevo DECIMAL(18,2) NOT NULL,
    IdVenta BIGINT NULL,
    IdCompra BIGINT NULL,
    Observacion NVARCHAR(250) NULL,
    Fecha DATETIME2 NOT NULL
        CONSTRAINT DF_MovimientosInventario_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_MovimientosInventario
        PRIMARY KEY (IdMovimiento),

    CONSTRAINT FK_MovimientosInventario_Productos
        FOREIGN KEY (IdProducto)
        REFERENCES Productos(IdProducto),

    CONSTRAINT FK_MovimientosInventario_Usuarios
        FOREIGN KEY (IdUsuario)
        REFERENCES Usuarios(IdUsuario),

    CONSTRAINT FK_MovimientosInventario_Ventas
        FOREIGN KEY (IdVenta)
        REFERENCES Ventas(IdVenta),

    CONSTRAINT FK_MovimientosInventario_Compras
        FOREIGN KEY (IdCompra)
        REFERENCES Compras(IdCompra),

    CONSTRAINT CK_MovimientosInventario_Tipo
        CHECK
        (
            TipoMovimiento IN
            (
                'COMPRA',
                'VENTA',
                'AJUSTE_ENTRADA',
                'AJUSTE_SALIDA'
            )
        ),

    CONSTRAINT CK_MovimientosInventario_Cantidad
        CHECK (Cantidad > 0),

    CONSTRAINT CK_MovimientosInventario_StockAnterior
        CHECK (StockAnterior >= 0),

    CONSTRAINT CK_MovimientosInventario_StockNuevo
        CHECK (StockNuevo >= 0),

    CONSTRAINT CK_MovimientosInventario_Referencia
        CHECK
        (
            NOT
            (
                IdVenta IS NOT NULL
                AND IdCompra IS NOT NULL
            )
        )
);
GO

CREATE INDEX IX_MovimientosInventario_IdProducto
ON MovimientosInventario(IdProducto);
GO

CREATE INDEX IX_MovimientosInventario_IdUsuario
ON MovimientosInventario(IdUsuario);
GO

CREATE INDEX IX_MovimientosInventario_Fecha
ON MovimientosInventario(Fecha);
GO

CREATE INDEX IX_MovimientosInventario_IdVenta
ON MovimientosInventario(IdVenta);
GO

CREATE INDEX IX_MovimientosInventario_IdCompra
ON MovimientosInventario(IdCompra);
GO

INSERT INTO Categorias
(
    Nombre,
    Descripcion
)
VALUES
('Bebidas', 'Bebidas y productos líquidos'),
('Snacks', 'Galletas, papas, chocolates y productos similares'),
('Limpieza', 'Productos para limpieza'),
('Otros', 'Productos sin una categoría específica');
GO

SELECT
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO