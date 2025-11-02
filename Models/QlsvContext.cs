using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLSV_V1.Models;

public partial class QlsvContext : DbContext
{
    public QlsvContext()
    {
    }

    public QlsvContext(DbContextOptions<QlsvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Advisor> Advisors { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Conduct> Conducts { get; set; }

    public virtual DbSet<Gpa> Gpas { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentSubject> StudentSubjects { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccId);

            entity.ToTable("Account");

            entity.Property(e => e.AccId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("AccID");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Password)
                .HasMaxLength(30)
                .IsFixedLength();
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Username)
                .HasMaxLength(30)
                .IsFixedLength();
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddId);

            entity.ToTable("Address");

            entity.Property(e => e.AddId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("AddID");
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Infor).HasMaxLength(50);
            entity.Property(e => e.Province).HasMaxLength(50);
        });

        modelBuilder.Entity<Advisor>(entity =>
        {
            entity.ToTable("Advisor");

            entity.Property(e => e.AdvisorId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("AdvisorID");
            entity.Property(e => e.UserId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Class__3214EC2794EDE47F");

            entity.ToTable("Class");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.DateCreate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("Date_Create");
            entity.Property(e => e.SemesterId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SemesterID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Open");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SubjectID");
            entity.Property(e => e.TeacherId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("TeacherID");
        });

        modelBuilder.Entity<Conduct>(entity =>
        {
            entity.ToTable("Conduct");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .HasColumnName("ID");
            entity.Property(e => e.SemesterId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SemesterID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Semester).WithMany(p => p.Conducts)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_Conduct_Semester");

            entity.HasOne(d => d.Student).WithMany(p => p.Conducts)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Conduct_Student");
        });

        modelBuilder.Entity<Gpa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GPA__3214EC279EDFB82A");

            entity.ToTable("GPA");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Gpa1).HasColumnName("GPA");
            entity.Property(e => e.Semesterid)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SEMESTERID");
            entity.Property(e => e.Studentid)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("STUDENTID");
            entity.Property(e => e.TongTc).HasColumnName("TongTC");

            entity.HasOne(d => d.Semester).WithMany(p => p.Gpas)
                .HasForeignKey(d => d.Semesterid)
                .HasConstraintName("FK_GPA_Semester");

            entity.HasOne(d => d.Student).WithMany(p => p.Gpas)
                .HasForeignKey(d => d.Studentid)
                .HasConstraintName("FK_GPA_Student");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester");

            entity.Property(e => e.SemesterId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SemesterID");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Year)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("StudentID");
            entity.Property(e => e.AdvisorId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("AdvisorID");
            entity.Property(e => e.UserId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Student_User");
        });

        modelBuilder.Entity<StudentSubject>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.SubjectId, e.SemesterId });

            entity.ToTable("Student_Subject", tb =>
                {
                    tb.HasTrigger("trg_Calc_PointTotal");
                    tb.HasTrigger("trg_Check_Account_Before_Semester");
                });

            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("StudentID");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SubjectID");
            entity.Property(e => e.SemesterId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("SemesterID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Point1).HasColumnName("Point_1");
            entity.Property(e => e.Point2).HasColumnName("Point_2");
            entity.Property(e => e.Point3).HasColumnName("Point_3");
            entity.Property(e => e.PointTotal).HasColumnName("Point_total");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Semester).WithMany(p => p.StudentSubjects)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SS_Semester");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSubjects)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SS_Student");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudentSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SS_Subject");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SoTc).HasColumnName("SoTC");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("Teacher");

            entity.Property(e => e.TeacherId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("TeacherID");
            entity.Property(e => e.UserId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("ID");
            entity.Property(e => e.AccId)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("AccID");
            entity.Property(e => e.AddId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("AddID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Acc).WithMany(p => p.Users)
                .HasForeignKey(d => d.AccId)
                .HasConstraintName("FK_User_Account");

            entity.HasOne(d => d.Add).WithMany(p => p.Users)
                .HasForeignKey(d => d.AddId)
                .HasConstraintName("FK_User_Address");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
