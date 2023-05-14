using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace bms_web_api.Data
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {

        }
        // DbSet
        public DbSet<UserData> Users { get; set; }
        public DbSet<BookData> Books { get; set; }
        public DbSet<AuthorData> Authors { get; set; }
        public DbSet<PublisherData> Publishers { get; set; }
        public DbSet<CategoryData> Categories { get; set; }
        public DbSet<CustomerData> Customers { get; set; }
        public DbSet<OrderData> Orders { get; set; }
        public DbSet<OrderItemData> OrderItems { get; set; }
        public DbSet<InventoryReceiptData> InventoryReceiptDatas { get; set; }
        public DbSet<InventoryExportData> InventoryExportDatas { get; set; }
        // Fluent Api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserData>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.userId);
                entity.HasIndex(user => user.username).IsUnique(); //duy nhất
                entity.Property(user => user.name).IsRequired().HasColumnType("nvarchar(50)");
                entity.Property(user => user.username).IsRequired().HasColumnType("varchar(20)");
                entity.Property(user => user.user_email).IsRequired().HasColumnType("varchar(50)");
                // Mã hóa chuỗi băm 64 ký tự
                entity.Property(user => user.password).IsRequired().HasColumnType("varchar(64)");
                entity.Property(user => user.role).IsRequired().HasColumnType("nvarchar(35)");

            });
            modelBuilder.Entity<BookData>(entity =>
            {
                // Tên bảng
                entity.ToTable("Books");
                // Khóa chính
                entity.HasKey(book => book.Id);
                entity.HasIndex(book => book.ISBN).IsUnique(); // mã duy nhất
                entity.Property(book => book.ISBN).IsRequired().HasMaxLength(13);
                entity.Property(book => book.book_title).IsRequired().HasColumnType("nvarchar(100)"); ;

                entity.Property(book => book.book_des).HasColumnType("nvarchar(max)");

                entity.Property(book => book.book_image).IsRequired().HasColumnType("varbinary(max)");
                entity.Property(book => book.user_book).IsRequired().HasColumnType("nvarchar(30)");
                entity.Property(book => book.public_date).IsRequired().HasColumnType("date");
                // Số lượng tồn kho không âm
                entity.Property(i => i.book_quantity).HasDefaultValue(0);
                // Kiểu DateTime hiện tại local
                entity.Property(book => book.update_date).HasDefaultValueSql("getdate()");
                // Một nhà xuất bản có thể xuất bản nhiều sách
                entity.HasOne(d => d.Publisher)
                .WithMany(p => p.BookDatas)
                .HasForeignKey(d => d.publisher_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Books_Publishers"); // tên khóa ngoại
                // Một thể loại có nhiều sách
                entity.HasOne(d => d.Category)
                .WithMany(p => p.BookDatas)
                .HasForeignKey(d => d.category_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Books_Categories");
                // Một tác giả có thể được viết nhiều quyển sách
                entity.HasOne(d => d.Author)
                .WithMany(p => p.BookDatas)
                .HasForeignKey(d => d.author_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Books_Authors");
            });
            modelBuilder.Entity<InventoryReceiptData>(entity =>
            {
                // Tên bảng
                entity.ToTable("InventoryReceipts");
                // Khóa chính
                entity.HasKey(i => i.irc_id);
                entity.Property(i => i.irc_id).IsRequired().HasColumnType("varchar(7)");
                // Số lượng tồn kho không âm
                entity.Property(i => i.book_quantity).HasDefaultValue(0);
                // Kiểu DateTime hiện tại local
                entity.Property(i => i.input_date).HasDefaultValueSql("getdate()");
                entity.HasOne(p => p.Book)
                .WithMany(b => b.InventoryReceipts)
                .HasForeignKey(p => p.book_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IRCs_Books");
            });
            modelBuilder.Entity<InventoryExportData>(entity =>
            {
                // Tên bảng
                entity.ToTable("InventoryExports");
                // Khóa chính
                entity.HasKey(i => i.iep_id);
                entity.Property(i => i.iep_id).IsRequired().HasColumnType("varchar(7)");
                // Kiểu DateTime hiện tại local
                entity.Property(i => i.export_date).HasDefaultValueSql("getdate()");
            });
            modelBuilder.Entity<AuthorData>(entity =>
            {
                entity.ToTable("Authors");
                entity.HasKey(author => author.Id);
                entity.Property(author => author.author_name).IsRequired().HasColumnType("nvarchar(100)");
            });

          
            modelBuilder.Entity<PublisherData>(entity =>
            {
                entity.ToTable("Publishers");
                entity.HasKey(publisher => publisher.Id);
                entity.Property(publisher => publisher.publisher_name).HasColumnType("nvarchar(120)");
                entity.Property(publisher => publisher.publisher_address).HasColumnType("nvarchar(120)");
                entity.Property(publisher => publisher.publisher_phone).HasColumnType("nvarchar(10)");

            });
            modelBuilder.Entity<CategoryData>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(cate => cate.Id);
                entity.Property(cate => cate.category_name).IsRequired().HasColumnType("nvarchar(50)");
            });
            modelBuilder.Entity<CustomerData>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(cus => cus.Id);
                entity.Property(cus => cus.customer_name).IsRequired().HasColumnType("nvarchar(100)");
                entity.Property(cus => cus.customer_email).IsRequired().HasColumnType("varchar(50)");
                entity.Property(cus => cus.customer_address).IsRequired().HasColumnType("nvarchar(120)");
                entity.Property(cus => cus.customer_phone).IsRequired().HasColumnType("nvarchar(10)");
            });
            modelBuilder.Entity<OrderData>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(order => order.order_id);
                entity.Property(order => order.order_id).IsRequired().HasColumnType("varchar(7)");
                // Thành tiền không âm
                entity.Property(order => order.total_price).HasDefaultValue(0);
                // Kiểu DateTime hiện tại local
                entity.Property(order => order.order_date).HasDefaultValueSql("getdate()");
                entity.Property(order => order.receive_date).HasDefaultValueSql("getdate()");
                entity.Property(order => order.payment).IsRequired().HasColumnType("nvarchar(30)");
                entity.Property(order => order.status).IsRequired().HasColumnType("nvarchar(30)");

                // một khách hàng sẽ có nhiều đơn hàng
                entity.HasOne(d => d.Customer)
              .WithMany(p => p.Orders)
              .HasForeignKey(d => d.customer_id)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Orders_Customers"); // tên khóa ngoại

            });
            modelBuilder.Entity<OrderItemData>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(orderItem => orderItem.Id);
                entity.Property(orderItem => orderItem.quantity).IsRequired().HasColumnType("int").HasDefaultValue(1);
                // một đơn hàng sẽ có nhiều sản phẩm/ order items
                entity.HasOne(d => d.Order)
              .WithMany(p => p.OrderItems)
              .HasForeignKey(d => d.order_id)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_OrderItems_Orders"); // tên khóa ngoại
                // một danh sách order chứa nhiều quyển sách
                entity.HasOne(d => d.Book)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.book_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_OrderItems_Books"); // tên khóa ngoại
            });

        }
    }
}
