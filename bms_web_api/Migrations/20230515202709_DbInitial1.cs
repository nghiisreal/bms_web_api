using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bms_web_api.Migrations
{
    public partial class DbInitial1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    author_name = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    customer_phone = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    customer_address = table.Column<string>(type: "nvarchar(120)", nullable: false),
                    customer_email = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    publisher_name = table.Column<string>(type: "nvarchar(120)", nullable: false),
                    publisher_address = table.Column<string>(type: "nvarchar(120)", nullable: false),
                    publisher_phone = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "varchar(20)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    user_email = table.Column<string>(type: "varchar(50)", nullable: false),
                    password = table.Column<string>(type: "varchar(64)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(35)", nullable: false),
                    emailConfirmation_Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isEmail_Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    verify_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    resetToken_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    login_Token = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "varchar(7)", nullable: false),
                    total_price = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    payment = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    receive_date = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getdate()"),
                    customer_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISBN = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    book_title = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    book_price = table.Column<double>(type: "float", nullable: false),
                    book_quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    num_pages = table.Column<int>(type: "int", nullable: false),
                    book_des = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    book_image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    user_book = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    public_date = table.Column<DateTime>(type: "date", nullable: false),
                    update_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    publisher_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    author_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Authors",
                        column: x => x.author_id,
                        principalTable: "Authors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Categories",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Publishers",
                        column: x => x.publisher_id,
                        principalTable: "Publishers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryExports",
                columns: table => new
                {
                    iep_id = table.Column<string>(type: "varchar(7)", nullable: false),
                    export_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    orderId = table.Column<string>(type: "varchar(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryExports", x => x.iep_id);
                    table.ForeignKey(
                        name: "FK_InventoryExports_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceipts",
                columns: table => new
                {
                    irc_id = table.Column<string>(type: "varchar(7)", nullable: false),
                    input_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    book_quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    book_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceipts", x => x.irc_id);
                    table.ForeignKey(
                        name: "FK_IRCs_Books",
                        column: x => x.book_id,
                        principalTable: "Books",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<string>(type: "varchar(7)", nullable: false),
                    book_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    book_price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Books",
                        column: x => x.book_id,
                        principalTable: "Books",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "order_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_author_id",
                table: "Books",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_Books_category_id",
                table: "Books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_publisher_id",
                table: "Books",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryExports_orderId",
                table: "InventoryExports",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipts_book_id",
                table: "InventoryReceipts",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_book_id",
                table: "OrderItems",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_order_id",
                table: "OrderItems",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_customer_id",
                table: "Orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_username",
                table: "Users",
                column: "username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryExports");

            migrationBuilder.DropTable(
                name: "InventoryReceipts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
