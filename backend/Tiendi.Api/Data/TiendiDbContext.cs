using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Tiendi.Api.Models;

namespace Tiendi.Api.Data;

public partial class TiendiDbContext : DbContext
{
    public TiendiDbContext(DbContextOptions<TiendiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVentas { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<MovimientosInventario> MovimientosInventarios { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedores { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria);

            entity.HasIndex(e => e.Nombre, "UQ_Categorias_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Categorias_Activo");
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Categorias_FechaCreacion");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.HasIndex(e => new { e.Nombre, e.Apellido }, "IX_Clientes_Nombre");

            entity.HasIndex(e => e.Identificacion, "UX_Clientes_Identificacion")
                .IsUnique()
                .HasFilter("([Identificacion] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Clientes_Activo");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Clientes_FechaCreacion");
            entity.Property(e => e.Identificacion).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra);

            entity.HasIndex(e => e.Fecha, "IX_Compras_Fecha");

            entity.HasIndex(e => e.IdProveedor, "IX_Compras_IdProveedor");

            entity.HasIndex(e => e.IdUsuario, "IX_Compras_IdUsuario");

            entity.HasIndex(e => e.NumeroCompra, "UQ_Compras_NumeroCompra").IsUnique();

            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETADA", "DF_Compras_Estado");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Compras_Fecha");
            entity.Property(e => e.NumeroCompra).HasMaxLength(30);
            entity.Property(e => e.Observacion).HasMaxLength(250);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Proveedores");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Usuarios");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra);

            entity.HasIndex(e => e.IdCompra, "IX_DetalleCompras_IdCompra");

            entity.HasIndex(e => e.IdProducto, "IX_DetalleCompras_IdProducto");

            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompras_Compras");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompras_Productos");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta);

            entity.HasIndex(e => e.IdProducto, "IX_DetalleVentas_IdProducto");

            entity.HasIndex(e => e.IdVenta, "IX_DetalleVentas_IdVenta");

            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentas_Productos");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentas_Ventas");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.IdGasto);

            entity.HasIndex(e => e.Fecha, "IX_Gastos_Fecha");

            entity.HasIndex(e => e.IdUsuario, "IX_Gastos_IdUsuario");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Gastos_Activo");
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Gastos_Fecha");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_Usuarios");
        });

        modelBuilder.Entity<MovimientosInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("MovimientosInventario");

            entity.HasIndex(e => e.Fecha, "IX_MovimientosInventario_Fecha");

            entity.HasIndex(e => e.IdCompra, "IX_MovimientosInventario_IdCompra");

            entity.HasIndex(e => e.IdProducto, "IX_MovimientosInventario_IdProducto");

            entity.HasIndex(e => e.IdUsuario, "IX_MovimientosInventario_IdUsuario");

            entity.HasIndex(e => e.IdVenta, "IX_MovimientosInventario_IdVenta");

            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_MovimientosInventario_Fecha");
            entity.Property(e => e.Observacion).HasMaxLength(250);
            entity.Property(e => e.StockAnterior).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StockNuevo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoMovimiento).HasMaxLength(30);

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK_MovimientosInventario_Compras");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientosInventario_Productos");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientosInventario_Usuarios");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.MovimientosInventarios)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_MovimientosInventario_Ventas");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago);

            entity.HasIndex(e => e.IdVenta, "IX_Pagos_IdVenta");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Pagos_Fecha");
            entity.Property(e => e.MetodoPago).HasMaxLength(30);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Ventas");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.HasIndex(e => e.IdCategoria, "IX_Productos_IdCategoria");

            entity.HasIndex(e => e.Nombre, "IX_Productos_Nombre");

            entity.HasIndex(e => e.Codigo, "UX_Productos_Codigo")
                .IsUnique()
                .HasFilter("([Codigo] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Productos_Activo");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Costo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.ImagenUrl).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Productos_FechaCreacion");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Stock).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StockMinimo).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK_Productos_Categorias");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor);

            entity.HasIndex(e => e.Nombre, "IX_Proveedores_Nombre");

            entity.HasIndex(e => e.Identificacion, "UX_Proveedores_Identificacion")
                .IsUnique()
                .HasFilter("([Identificacion] IS NOT NULL)");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Proveedores_Activo");
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Proveedores_FechaCreacion");
            entity.Property(e => e.Identificacion).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.HasIndex(e => e.Email, "UQ_Usuarios_Email").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuarios_Activo");
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Usuarios_FechaCreacion");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Rol)
                .HasMaxLength(30)
                .HasDefaultValue("ADMIN", "DF_Usuarios_Rol");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta);

            entity.HasIndex(e => e.Fecha, "IX_Ventas_Fecha");

            entity.HasIndex(e => e.IdCliente, "IX_Ventas_IdCliente");

            entity.HasIndex(e => e.IdUsuario, "IX_Ventas_IdUsuario");

            entity.HasIndex(e => e.NumeroVenta, "UQ_Ventas_NumeroVenta").IsUnique();

            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETADA", "DF_Ventas_Estado");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Ventas_Fecha");
            entity.Property(e => e.NumeroVenta).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Ventas_Clientes");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
