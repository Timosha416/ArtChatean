using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ArtChatean.Models
{
    public class ArtDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ArtDbContext(DbContextOptions options) : base(options)
        {

        }
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Picture> Pictures { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<Address> Addresses { get; set; } = null!; 
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<PictureOrder> PictureOrders { get; set; } = null!;
        public virtual DbSet<UserFriend> UserFriends { get; set; }= null!;
        public virtual DbSet<Like> Likes { get; set; } = null!;
        public virtual DbSet<Save> Saves { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
	    public virtual DbSet<Artist> Artists { get; set; } = null!;
        public virtual DbSet<Event> Event { get; set; } = null!;
        public virtual DbSet<EventTimeSlot> EventTimeSlot { get; set; } = null!;
        public virtual DbSet<TicketTariff> TicketTariff { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; }
	public virtual DbSet<Painting> Paintings { get; set; } = null!;
        public virtual DbSet<Era> Eras { get; set; } = null!;
        public virtual DbSet<TicketPurchase> TicketPurchases { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфігурація для поля Image у User
            modelBuilder.Entity<User>()
                .Property(u => u.Image)
                .HasColumnType("BLOB"); // Вказуємо тип BLOB для поля Image в SQLite

            // Конфігурація для поля Image у Picture
            modelBuilder.Entity<Picture>()
                .Property(p => p.Image)
                .HasColumnType("BLOB"); // Вказуємо тип BLOB для поля Image в SQLite

            // Конфігурація для зв'язку між User і Picture
            modelBuilder.Entity<Picture>()
                .HasOne(p => p.User)
                .WithMany(u => u.Pictures)
                .HasForeignKey(p => p.UserId);

            // Конфігурація для зв'язків у моделі Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders) // Може бути багато замовлень на одного покупця
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Або інша поведінка при видаленні

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany() // Може бути багато замовлень від одного продавця
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // Або інша поведінка при видаленні

            // Конфігурація для зв'язку "багато до багатьох" між Picture і Order через PictureOrder
            modelBuilder.Entity<PictureOrder>()
                .HasKey(po => po.Id); // Зазначення одиночного первинного ключа

            // Налаштування зв'язку з Picture
            modelBuilder.Entity<PictureOrder>()
                .HasOne(po => po.Picture)
                .WithMany(p => p.PictureOrders)
                .HasForeignKey(po => po.PictureId)
                .OnDelete(DeleteBehavior.Restrict); // Установіть DeleteBehavior відповідно до потреб

            // Налаштування зв'язку з Order
            modelBuilder.Entity<PictureOrder>()
                .HasOne(po => po.Order)
                .WithMany(o => o.PictureOrders)
                .HasForeignKey(po => po.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Налаштування комбінованого унікального ключа UserFriends
            modelBuilder.Entity<UserFriend>()
                .HasIndex(uf => new { uf.UserId, uf.FriendId })
                .IsUnique();

            // Налаштування зв'язку для UserId
            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFriends)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Заборона каскадного видалення

            // Налаштування зв'язку для FriendId
            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.Friend)
                .WithMany() // Це односторонній зв'язок, тому не потрібен зворотній зв'язок
                .HasForeignKey(uf => uf.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like, Picture, User
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PictureId })
                .IsUnique();

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити користувача, видаляються й його лайки

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Picture)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PictureId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити картину, видаляються й лайки, пов'язані з нею

            // Save, Picture, User
            modelBuilder.Entity<Save>()
                .HasIndex(l => new { l.UserId, l.PictureId })
                .IsUnique();

            modelBuilder.Entity<Save>()
                .HasOne(l => l.User)
                .WithMany(u => u.Saves)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити користувача, видаляються й його сейви

            modelBuilder.Entity<Save>()
                .HasOne(l => l.Picture)
                .WithMany(p => p.Saves)
                .HasForeignKey(l => l.PictureId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити картину, видаляються й сейви, пов'язані з нею

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити користувача, видаляються й його коментарі

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Picture)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PictureId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити картину, видаляються й коментарі, пов'язані з нею

		// Конфігурація для Event
            modelBuilder.Entity<Event>()
                .HasKey(e => e.Id); // Первинний ключ

            modelBuilder.Entity<Event>()
                .HasMany(e => e.TimeSlots) // Один Event має багато TimeSlots
                .WithOne(ts => ts.Event) // Один TimeSlot належить одному Event
                .HasForeignKey(ts => ts.EventId)
                .OnDelete(DeleteBehavior.Cascade); // Видалення Event тягне за собою видалення TimeSlots

            // Конфігурація для EventTimeSlot
            modelBuilder.Entity<EventTimeSlot>()
                .HasKey(ts => ts.Id); // Первинний ключ для TimeSlot

            // Встановлюємо зв'язок між ChatMessage і User
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender) // Визначаємо, що одне повідомлення має одного відправника
                .WithMany(u => u.ChatMessages) // Визначаємо, що користувач може мати багато повідомлень
                .HasForeignKey(m => m.SenderId) // Вказуємо зовнішній ключ
                .OnDelete(DeleteBehavior.Restrict); // Забороняємо видалення каскадом для відправника  
        }
    }    
}
