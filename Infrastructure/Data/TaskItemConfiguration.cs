using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class TaskItemConfiguration
        : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(t => t.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Priority)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.DueDate)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(t => t.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(t => t.AssignedToUserId)
                .IsRequired(false);

            builder.HasIndex(t => t.ProjectId)
                .IsClustered(false);

            builder.HasIndex(t => t.AssignedToUserId)
                .IsClustered(false);

            builder.HasIndex(t => new
            {
                t.AssignedToUserId,
                t.Status
            });

            builder.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
