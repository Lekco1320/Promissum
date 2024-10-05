using Lekco.Promissum.Model.Sync.Record;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The database context represents a session with the database to sync task.
    /// </summary>
    public class SyncDbContext : DbContext
    {
        /// <summary>
        /// File name of the database.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Indicates whether connect the database in readonly mode.
        /// </summary>
        public bool IsReadonly { get; }

        /// <summary>
        /// Indicates whether using connection pool.
        /// </summary>
        public bool Pooling { get; }

        /// <summary>
        /// Dataset of <see cref="FileRecord"/>.
        /// </summary>
        public DbSet<FileRecord> FileRecords { get; protected set; }

        /// <summary>
        /// Dataset of <see cref="CleanUpRecord"/>.
        /// </summary>
        public DbSet<CleanUpRecord> CleanUpRecords { get; protected set; }

        /// <summary>
        /// Dataset of <see cref="ExecutionRecord"/>.
        /// </summary>
        public DbSet<ExecutionRecord> ExecutionRecords { get; protected set; }

        /// <summary>
        /// Dataset of <see cref="ExceptionRecord"/>.
        /// </summary>
        public DbSet<ExceptionRecord> ExceptionRecords { get; protected set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fileName">File name of the database.</param>
        protected SyncDbContext(string fileName, bool isReadonly, bool pooling)
        {
            FileName = fileName;
            IsReadonly = isReadonly;
            Pooling = pooling;
            Database.OpenConnection();
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            Database.CloseConnection();
        }

        /// <summary>
        /// Get an instance of database context with initialized connection.
        /// </summary>
        /// <param name="fileName">File name of the database.</param>
        /// <param name="isReadonly">Indicates whether connect the database in readonly mode.</param>
        /// <param name="pooling">Indicates whether using connection pool.</param>
        /// <returns>An instance of the database context.</returns>
        public static SyncDbContext GetDbContext(string fileName, bool isReadonly, bool pooling)
        {
            var ret = new SyncDbContext(fileName, isReadonly, pooling);
            ret.Initialize();
            return ret;
        }

        /// <summary>
        /// Initialize the connection.
        /// </summary>
        protected void Initialize()
        {
            Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        }

        /// <summary>
        /// Ensures that the database for the context exists.
        /// </summary>
        public void EnsureCreated()
        {
            if (!IsReadonly)
            {
                Database.EnsureCreated();
            }
        }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connect = $"Data Source = {FileName};";
            if (IsReadonly)
            {
                connect += "Mode = ReadOnly;";
            }
            if (Pooling)
            {
                connect += "Pooling = true;";
            }
            optionsBuilder.UseSqlite(connect);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region FileRecord

            modelBuilder.Entity<FileRecord>()
                .Property(record => record.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<FileRecord>()
                .HasKey(record => record.ID);

            modelBuilder.Entity<FileRecord>()
                .HasIndex(record => record.RelativeFileName);

            #endregion

            #region CleanUpRecord

            var converter = new ValueConverter<List<int>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)!
            );

            modelBuilder.Entity<CleanUpRecord>()
                .Property(record => record.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CleanUpRecord>()
                .Property(record => record.ReservedVersions)
                .HasConversion(converter);

            modelBuilder.Entity<CleanUpRecord>()
                .HasKey(record => record.ID);

            modelBuilder.Entity<CleanUpRecord>()
                .HasIndex(record => record.RelativeFileName);

            #endregion

            #region ExecutionRecord

            modelBuilder.Entity<ExecutionRecord>()
                .Property(record => record.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ExecutionRecord>()
                .HasKey(record => record.ID);

            modelBuilder.Entity<ExecutionRecord>()
                .HasMany(record => record.ExceptionRecords)
                .WithOne(exRecord => exRecord.ExecutionRecord)
                .HasForeignKey(exRecord => exRecord.ExecutionID)
                .IsRequired();

            #endregion

            #region ExceptionRecord

            modelBuilder.Entity<ExceptionRecord>()
                .Property(record => record.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ExceptionRecord>()
                .HasKey(record => record.ID);

            #endregion
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (!IsReadonly)
            {
                SaveChanges();
            }
            GC.SuppressFinalize(this);
        }
    }
}
