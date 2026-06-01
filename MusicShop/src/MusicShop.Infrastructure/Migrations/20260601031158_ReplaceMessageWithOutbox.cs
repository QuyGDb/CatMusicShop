using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMessageWithOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistGenres_Artists_ArtistId",
                table: "ArtistGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistGenres_Genres_GenreId",
                table: "ArtistGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CuratedCollectionItems_CuratedCollections_CollectionId",
                table: "CuratedCollectionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CuratedCollectionItems_Products_ProductId",
                table: "CuratedCollectionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseGenres_Genres_GenreId",
                table: "ReleaseGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseGenres_Releases_ReleaseId",
                table: "ReleaseGenres");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReleaseGenres",
                table: "ReleaseGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CuratedCollectionItems",
                table: "CuratedCollectionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carts",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistGenres",
                table: "ArtistGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminActivityLogs",
                table: "AdminActivityLogs");

            migrationBuilder.RenameTable(
                name: "ReleaseGenres",
                newName: "ReleaseGenre");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "CuratedCollectionItems",
                newName: "CuratedCollectionItem");

            migrationBuilder.RenameTable(
                name: "Carts",
                newName: "Cart");

            migrationBuilder.RenameTable(
                name: "CartItems",
                newName: "CartItem");

            migrationBuilder.RenameTable(
                name: "ArtistGenres",
                newName: "ArtistGenre");

            migrationBuilder.RenameTable(
                name: "AdminActivityLogs",
                newName: "AdminActivityLog");

            migrationBuilder.RenameIndex(
                name: "IX_ReleaseGenres_GenreId",
                table: "ReleaseGenre",
                newName: "IX_ReleaseGenre_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_OrderId",
                table: "Payment",
                newName: "IX_Payment_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItem",
                newName: "IX_OrderItem_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CuratedCollectionItems_ProductId",
                table: "CuratedCollectionItem",
                newName: "IX_CuratedCollectionItem_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CuratedCollectionItems_CollectionId",
                table: "CuratedCollectionItem",
                newName: "IX_CuratedCollectionItem_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_UserId",
                table: "Cart",
                newName: "IX_Cart_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItem",
                newName: "IX_CartItem_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_CartId",
                table: "CartItem",
                newName: "IX_CartItem_CartId");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistGenres_GenreId",
                table: "ArtistGenre",
                newName: "IX_ArtistGenre_GenreId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReleaseGenre",
                table: "ReleaseGenre",
                columns: new[] { "ReleaseId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CuratedCollectionItem",
                table: "CuratedCollectionItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cart",
                table: "Cart",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistGenre",
                table: "ArtistGenre",
                columns: new[] { "ArtistId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminActivityLog",
                table: "AdminActivityLog",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedAt",
                table: "OutboxMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status",
                table: "OutboxMessages",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistGenre_Artists_ArtistId",
                table: "ArtistGenre",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistGenre_Genres_GenreId",
                table: "ArtistGenre",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Cart_CartId",
                table: "CartItem",
                column: "CartId",
                principalTable: "Cart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CuratedCollectionItem_CuratedCollections_CollectionId",
                table: "CuratedCollectionItem",
                column: "CollectionId",
                principalTable: "CuratedCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CuratedCollectionItem_Products_ProductId",
                table: "CuratedCollectionItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Order_OrderId",
                table: "Payment",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseGenre_Genres_GenreId",
                table: "ReleaseGenre",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseGenre_Releases_ReleaseId",
                table: "ReleaseGenre",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistGenre_Artists_ArtistId",
                table: "ArtistGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistGenre_Genres_GenreId",
                table: "ArtistGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Cart_CartId",
                table: "CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CuratedCollectionItem_CuratedCollections_CollectionId",
                table: "CuratedCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_CuratedCollectionItem_Products_ProductId",
                table: "CuratedCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Order_OrderId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseGenre_Genres_GenreId",
                table: "ReleaseGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseGenre_Releases_ReleaseId",
                table: "ReleaseGenre");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReleaseGenre",
                table: "ReleaseGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CuratedCollectionItem",
                table: "CuratedCollectionItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cart",
                table: "Cart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistGenre",
                table: "ArtistGenre");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminActivityLog",
                table: "AdminActivityLog");

            migrationBuilder.RenameTable(
                name: "ReleaseGenre",
                newName: "ReleaseGenres");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "CuratedCollectionItem",
                newName: "CuratedCollectionItems");

            migrationBuilder.RenameTable(
                name: "CartItem",
                newName: "CartItems");

            migrationBuilder.RenameTable(
                name: "Cart",
                newName: "Carts");

            migrationBuilder.RenameTable(
                name: "ArtistGenre",
                newName: "ArtistGenres");

            migrationBuilder.RenameTable(
                name: "AdminActivityLog",
                newName: "AdminActivityLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ReleaseGenre_GenreId",
                table: "ReleaseGenres",
                newName: "IX_ReleaseGenres_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_OrderId",
                table: "Payments",
                newName: "IX_Payments_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItems",
                newName: "IX_OrderItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_CuratedCollectionItem_ProductId",
                table: "CuratedCollectionItems",
                newName: "IX_CuratedCollectionItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CuratedCollectionItem_CollectionId",
                table: "CuratedCollectionItems",
                newName: "IX_CuratedCollectionItems_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItem_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItem_CartId",
                table: "CartItems",
                newName: "IX_CartItems_CartId");

            migrationBuilder.RenameIndex(
                name: "IX_Cart_UserId",
                table: "Carts",
                newName: "IX_Carts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistGenre_GenreId",
                table: "ArtistGenres",
                newName: "IX_ArtistGenres_GenreId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReleaseGenres",
                table: "ReleaseGenres",
                columns: new[] { "ReleaseId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CuratedCollectionItems",
                table: "CuratedCollectionItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carts",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistGenres",
                table: "ArtistGenres",
                columns: new[] { "ArtistId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminActivityLogs",
                table: "AdminActivityLogs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LockId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MessageId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReplacedByTokenHash = table.Column<string>(type: "text", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Direction_ProcessedAt",
                table: "Messages",
                columns: new[] { "Direction", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IdempotencyKey",
                table: "Messages",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"Direction\" = 'Outbox' AND \"IdempotencyKey\" IS NOT NULL AND \"ProcessedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageId",
                table: "Messages",
                column: "MessageId",
                unique: true,
                filter: "\"Direction\" = 'Inbox' AND \"MessageId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistGenres_Artists_ArtistId",
                table: "ArtistGenres",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistGenres_Genres_GenreId",
                table: "ArtistGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CuratedCollectionItems_CuratedCollections_CollectionId",
                table: "CuratedCollectionItems",
                column: "CollectionId",
                principalTable: "CuratedCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CuratedCollectionItems_Products_ProductId",
                table: "CuratedCollectionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseGenres_Genres_GenreId",
                table: "ReleaseGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseGenres_Releases_ReleaseId",
                table: "ReleaseGenres",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
