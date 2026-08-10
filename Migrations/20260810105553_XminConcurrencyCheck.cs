using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakerspaceFablabPlatform.Migrations
{
    /// <inheritdoc />
    public partial class XminConcurrencyCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // xmin, PostgreSQL'in her satırda zaten tuttuğu bir sistem sütunudur;
            // burada yeni bir sütun oluşturulmuyor, EF sadece onu concurrency token
            // olarak okumaya başlıyor. Bu yüzden Up/Down kasıtlı olarak boş.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
