/*
 * Copyright (c) 2024 Markus Schmid
 *
 * Dieser Quellcode ist Teil von CLAn - Call Log Analyzer und unterliegt der MIT-Lizenz, die
 * im Wurzelverzeichnis dieses Projekts als LICENSE-Datei hinterlegt ist. 
 * Eine Kopie der Lizenz können Sie unter folgendem Link einsehen:
 *
 * https://opensource.org/licenses/MIT
 */
 
using Microsoft.EntityFrameworkCore;
using CLAn.Entities;

namespace CLAn.Infrastructure.Data
{

    public class SqliteContext : DbContext
    {
        public SqliteContext(DbContextOptions<SqliteContext> options)
            : base(options)
        { }

        public DbSet<Person> Persons { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<LogFile> LogFiles { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<PhoneNumberLogFile> PhoneNumberLogFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Person

            builder.Entity<Person>(entity =>
            {
                entity.ToTable("Person");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasColumnType("varchar(512)")
                    .IsRequired();

                entity.HasMany(p => p.PhoneNumbers)
                    .WithOne(p => p.Person)
                    .HasForeignKey(p => p.PersonId);

                entity.HasIndex(p => p.Name)
                    .IsUnique();
            });

            // PhoneNumberLogFile

            builder.Entity<PhoneNumberLogFile>(entity =>
            {
                entity.HasKey(pl => new { pl.PhoneNumberId, pl.LogFileId });

                entity.HasOne(pl => pl.PhoneNumber)
                    .WithMany(p => p.PhoneNumberLogFiles)
                    .HasForeignKey(pl => pl.PhoneNumberId);

                entity.HasOne(pl => pl.LogFile)
                    .WithMany(l => l.PhoneNumberLogFiles)
                    .HasForeignKey(pl => pl.LogFileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // PhoneNumber

            builder.Entity<PhoneNumber>(entity =>
            {
                entity.ToTable("PhoneNumber");

                entity.HasKey(pn => pn.Id);

                entity.Property(p => p.Number)
                    .HasColumnType("varchar(32)");

                entity.HasIndex(pn => pn.Number)
                    .IsUnique();
            });

            // Group

            builder.Entity<Group>(entity =>
            {
                entity.ToTable("Group");

                entity.HasKey(g => g.Id);

                entity.Property(g => g.Name)
                    .HasColumnType("varchar(512)")
                    .IsRequired();
            });

            // PersonGroup

            builder.Entity<PersonGroup>(entity =>
            {
                entity.ToTable("PersonGroup");

                entity.HasKey(pg => new { pg.PersonId, pg.GroupId });

                entity.HasOne(pg => pg.Person)
                    .WithMany(p => p.PersonGroups)
                    .HasForeignKey(pg => pg.PersonId);

                entity.HasOne(pg => pg.Group)
                    .WithMany(g => g.PersonGroups)
                    .HasForeignKey(pg => pg.GroupId);
            });

            // LogFile

            builder.Entity<LogFile>(entity =>
            {
                entity.ToTable("LogFile");

                entity.HasKey(lf => lf.Id);

                entity.HasOne(lf => lf.Person)
                    .WithMany(p => p.LogFiles)
                    .HasForeignKey(lf => lf.PersonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Log

            builder.Entity<Log>(entity =>
            {
                entity.ToTable("Log");

                entity.HasKey(log => log.Id);

                entity.HasIndex(log => new { log.SenderId, log.ReceiverId, log.Validation, log.DateTime });

                entity.HasIndex(log => log.SenderId);

                entity.HasIndex(log => log.ReceiverId);

                entity.HasIndex(log => log.DateTime);

                entity.HasIndex(log => log.Validation);

                entity.HasIndex(log => log.LogFileOwnerId);

                entity.HasIndex(log => log.LogFileOwnerPhoneNumberId);

                entity.HasOne(log => log.LogFileOwnerPhoneNumber)
                    .WithMany(pn => pn.Logs)
                    .HasForeignKey(log => log.LogFileOwnerPhoneNumberId);

                entity.HasOne(log => log.LogFileOwner)
                    .WithMany(p => p.Logs)
                    .HasForeignKey(log => log.LogFileOwnerId);

                entity.HasOne(log => log.Sender)
                    .WithMany()
                    .HasForeignKey(log => log.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(log => log.Receiver)
                    .WithMany()
                    .HasForeignKey(log => log.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.LogFile)
                    .WithMany(lf => lf.Logs)
                    .HasForeignKey(l => l.LogFileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }

    public static class SqliteConsoleContextFactory
    {
        public static SqliteContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqliteContext>();
            
            optionsBuilder.UseSqlite(connectionString);

            var context = new SqliteContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            context.Database.Migrate();

            return context;
        }
    }
}