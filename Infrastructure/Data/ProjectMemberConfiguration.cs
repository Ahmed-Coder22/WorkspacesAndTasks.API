using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ProjectMemberConfiguration
        : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");

            builder.HasKey(pm => new
            {
                pm.ProjectId,
                pm.UserId
            });

            builder.Property(pm => pm.UserId)
                .IsRequired();

            builder.Property(pm => pm.Role)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Member");

            builder.Property(pm => pm.JoinedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasIndex(pm => pm.UserId)
                .IsClustered(false);

            builder.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}